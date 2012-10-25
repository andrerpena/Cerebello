using System;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Cerebello.Model;
using CerebelloWebRole.Code.Filters;
using JetBrains.Annotations;

namespace CerebelloWebRole.Code.Access
{
    public static class AccessManager
    {
        /// <summary>
        /// Global rules of access to objects stored in the database.
        /// These rules are the most relaxed access rules.
        /// Most of them will only check for objects being of the same practice.
        /// Some may restrict a little more.
        /// </summary>
        public static class Reach
        {
            public static bool Check(CerebelloEntities db, User op, ActiveIngredient obj)
            {
                return db.Users.Any(u => u.Id == op.Id
                                         &&
                                         u.Practice.Users.Any(
                                             u2 => u2.Doctor.ActiveIngredients.Any(ai => ai.Id == obj.Id)));
            }

            public static bool Check(CerebelloEntities db, User op, Address obj)
            {
                return db.Users.Any(u => u.Id == op.Id
                                         && u.Practice.Users.Any(u2 => u2.Person.Address.Id == obj.Id));
            }

            public static bool Check(CerebelloEntities db, User op, Administrator obj)
            {
                return db.Users.Any(u => u.Id == op.Id
                                         && u.Practice.Users.Any(u2 => u2.AdministratorId == obj.Id));
            }

            public static bool Check(CerebelloEntities db, User op, Anamnese obj)
            {
                return db.Users.Any(u => u.Id == op.Id
                                         &&
                                         u.Practice.Users.Any(
                                             u2 => u2.Doctor.Patients.Any(p => p.Anamneses.Any(an => an.Id == obj.Id))));
            }

            public static bool Check(CerebelloEntities db, User op, Appointment obj)
            {
                return db.Users.Any(u => u.Id == op.Id
                                         &&
                                         u.Practice.Users.Any(u2 => u2.Doctor.Appointments.Any(ap => ap.Id == obj.Id)));
            }

            public static bool Check(CerebelloEntities db, User op, CFG_DayOff obj)
            {
                return db.Users.Any(u => u.Id == op.Id
                                         &&
                                         u.Practice.Users.Any(u2 => u2.Doctor.CFG_DayOff.Any(doff => doff.Id == obj.Id)));
            }

            public static bool Check(CerebelloEntities db, User op, CFG_Documents obj)
            {
                // Only the doctor can change his documents configuration.
                return db.Users.Any(u => u.Id == op.Id
                                         && u.DoctorId == obj.DoctorId);
            }

            public static bool Check(CerebelloEntities db, User op, CFG_Schedule obj)
            {
                // Only the doctor can change his schedule.
                return db.Users.Any(u => u.Id == op.Id
                                         && u.DoctorId == obj.DoctorId);
            }

            public static bool Check(CerebelloEntities db, User op, ChatMessage obj)
            {
                // Only the user itself, or admin or owner can see messages.
                var query = from u in db.Users
                            let u2 = db.Users.FirstOrDefault(u2 => u2.Id == obj.Id)
                            where u.Id == op.Id
                                  && u.PracticeId == obj.PracticeId
                                  &&
                                  (u.IsOwner || u.AdministratorId != null || u.Id == obj.UserToId ||
                                   u.Id == obj.UserFromId)
                            select u;

                return query.Any();
            }

            public static bool Check(CerebelloEntities db, User op, User obj)
            {
                // If both users are in the same practice, then they can see each other.
                return db.Users.Any(u => u.Id == op.Id
                                         && u.Practice.Users.Any(u2 => u2.Id == obj.Id));
            }

            public static bool Check(CerebelloEntities db, User op, Secretary obj)
            {
                return db.Users.Any(u => u.Id == op.Id
                                         && u.Practice.Users.Any(u2 => u2.SecretaryId == obj.Id));
            }

            public static bool Check(CerebelloEntities db, User op, Doctor obj)
            {
                return db.Users.Any(u => u.Id == op.Id
                                         && u.Practice.Users.Any(u2 => u2.DoctorId == obj.Id));
            }
        }

        /// <summary>
        /// Finds out whether user can access the specified action, by looking at PermissionAttribute attributes.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="user"></param>
        /// <param name="action"></param>
        /// <param name="controller"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static bool CanAccessAction(
            this ControllerContext @this,
            User user,
            [AspMvcAction]string action = null,
            [AspMvcController]string controller = null,
            string method = "GET")
        {
            // TODO: must cache all of these informations

            if (@this == null)
                throw new ArgumentNullException("this");

            if (user == null)
                throw new ArgumentNullException("user");

            var attributes = @this.GetAttributesOfAction(action, controller, method)
                .OfType<PermissionAttribute>()
                .ToArray();

            var result = !attributes.Any()
                || attributes.All(pa => pa.CanAccessResource(user));

            return result;
        }

        /// <summary>
        /// Returns the attributes places on an action method.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="action"></param>
        /// <param name="controller"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static Attribute[] GetAttributesOfAction(
            this ControllerContext @this,
            [AspMvcAction]string action = null,
            [AspMvcController]string controller = null,
            string method = "GET")
        {
            // TODO: must cache all of these informations

            if (@this == null)
                throw new ArgumentNullException("this");

            var actionName = action
                ?? @this.RouteData.GetRequiredString("action");

            var controllerName = controller
                ?? @this.RouteData.GetRequiredString("controller");

            const bool testable = true;
            var controllerFactory =
                testable
                ? ((ControllerBuilder)typeof(MvcHandler)
                    .GetProperty("ControllerBuilder", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(@this.HttpContext.Handler))
                    .GetControllerFactory()
                : ControllerBuilder.Current.GetControllerFactory();

            var otherController = (ControllerBase)controllerFactory
                .CreateController(
                    new RequestContext(@this.HttpContext, new RouteData()),
                    controllerName);

            var controllerDescriptor = new ReflectedControllerDescriptor(
                otherController.GetType());

            var controllerContextWithMethodParam = new ControllerContext(
                new MockHttpContextWrapper(
                    @this.HttpContext.ApplicationInstance.Context,
                    method),
                new RouteData(),
                otherController);

            var actionDescriptor = controllerDescriptor
                .FindAction(controllerContextWithMethodParam, actionName);

            var attributes = actionDescriptor.GetCustomAttributes(true)
                .Cast<Attribute>()
                .ToArray();

            return attributes;
        }

        /// <summary>
        /// Checks whether the current user can access an action.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="action"></param>
        /// <param name="controller"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static bool CanAccessAction(
            this WebViewPage @this,
            [AspMvcAction]string action = null,
            [AspMvcController]string controller = null,
            string method = "GET")
        {
            // TODO: must cache all of these informations

            if (@this == null)
                throw new ArgumentNullException("this");

            var cerebelloController = @this.ViewContext.Controller as CerebelloController;

            if (cerebelloController != null)
            {
                cerebelloController.InitDb();
                cerebelloController.InitDbUser(@this.Request.RequestContext);

                var result = @this.ViewContext.Controller.ControllerContext
                    .CanAccessAction(cerebelloController.DbUser, action, controller, method);

                return result;
            }

            return false;
        }

        class MockHttpContextWrapper : HttpContextWrapper
        {
            public MockHttpContextWrapper(HttpContext httpContext, string method)
                : base(httpContext)
            {
                this.request = new MockHttpRequestWrapper(httpContext.Request, method);
            }

            private readonly HttpRequestBase request;
            public override HttpRequestBase Request
            {
                get { return request; }
            }

            class MockHttpRequestWrapper : HttpRequestWrapper
            {
                public MockHttpRequestWrapper(HttpRequest httpRequest, string httpMethod)
                    : base(httpRequest)
                {
                    this.httpMethod = httpMethod;
                }

                private readonly string httpMethod;
                public override string HttpMethod
                {
                    get { return httpMethod; }
                }
            }
        }
    }
}
