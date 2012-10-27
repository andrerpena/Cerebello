using System;
using System.Data;
using System.Data.Metadata.Edm;
using System.Data.Objects;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Code.Access;

namespace CerebelloWebRole.Code.Filters
{
    public class AccessDbObjectAttribute : FilterAttribute, IActionFilter
    {
        private readonly Type dbObjectType;
        private readonly string routeKeyName;

        public AccessDbObjectAttribute(Type dbObjectType, string routeKeyName)
        {
            this.dbObjectType = dbObjectType;
            this.routeKeyName = routeKeyName;
        }

        // reference:
        // http://farm-fresh-code.blogspot.com.br/2009/11/customizing-authorization-in-aspnet-mvc.html

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.Result == null)
            {
                var cerebelloController = filterContext.Controller as CerebelloController;

                if (cerebelloController != null)
                {
                    CerebelloEntities db = cerebelloController.InitDb();
                    cerebelloController.InitDbUser(filterContext.RequestContext);

                    Debug.Assert(db != null, "db must not be null");
                    Debug.Assert(cerebelloController.DbUser != null, "cerebelloController.DbUser must not be null");

                    // Getting the object by id from the database.
                    var keyName = db
                        .MetadataWorkspace
                        .GetEntityContainer(db.DefaultContainerName, DataSpace.CSpace)
                        .BaseEntitySets
                        .First(meta => meta.ElementType.Name == this.dbObjectType.Name)
                        .ElementType
                        .KeyMembers
                        .Select(k => k.Name)
                        .FirstOrDefault();

                    int keyValue = int.Parse((string)filterContext.RouteData.Values[this.routeKeyName]);

                    object obj;
                    var exists = db.TryGetObjectByKey(new EntityKey(
                        string.Format("{0}.{1}", db.DefaultContainerName, GetEntitySetName(db, this.dbObjectType)),
                        keyName,
                        keyValue), out obj);

                    // Determining whether the current user can reach the given object or not.
                    // When an object is not reachable, for the user, it is like the object didn't exist.
                    // That's why this is not an IAuthorizationFilter.
                    bool canAccess = false;
                    if (exists)
                    {
                        var method = typeof(AccessManager.Reach)
                            .GetMethod("Check", new[] { typeof(CerebelloEntities), typeof(User), this.dbObjectType });

                        canAccess = (bool)method.Invoke(null, new[] { db, cerebelloController.DbUser, obj });
                    }

                    if (canAccess)
                    {
                        // Sets the object to controller.DbObject so that it does not have to query it again to use it.
                        cerebelloController.DbObject = obj;
                    }
                    else
                    {
                        if (filterContext.HttpContext.Request.IsAjaxRequest())
                        {
                            filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                            filterContext.Result =
                                new JsonResult
                                {
                                    Data = new
                                    {
                                        error = true,
                                        errorType = "not found",
                                        errorMessage =
                             "The object you are trying to access does not exist."
                                    },
                                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                                };
                        }
                        else
                        {
                            filterContext.Result = new HttpNotFoundResult();
                        }
                    }
                }
                else
                    throw new Exception("The AccessDbObjectAttribute cannot be used on actions of this controller.");
            }
        }

        /// <summary>
        /// Returns entity set name for a given entity type
        /// </summary>
        /// <param name="context">An ObjectContext which defines the entity set for entityType. Must be non-null.</param>
        /// <param name="entityType">An entity type. Must be non-null and have an entity set defined in the context argument.</param>
        /// <exception cref="ArgumentException">If entityType is not an entity or has no entity set defined in context.</exception>
        /// <returns>String name of the entity set.</returns>
        private static string GetEntitySetName(ObjectContext context, Type entityType)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (entityType == null)
            {
                throw new ArgumentNullException("entityType");
            }
            // when POCO proxies are enabled, "entityType" may be a subtype of the mapped type.
            Type nonProxyEntityType = ObjectContext.GetObjectType(entityType);
            if (entityType == null)
            {
                throw new ArgumentException(string.Format("Not an entity type {0}.", entityType.Name));
            }

            var container = context.MetadataWorkspace.GetEntityContainer(context.DefaultContainerName, DataSpace.CSpace);
            var result = (from entitySet in container.BaseEntitySets
                          where entitySet.ElementType.Name.Equals(nonProxyEntityType.Name)
                          select entitySet.Name).SingleOrDefault();
            if (string.IsNullOrEmpty(result))
            {
                throw new ArgumentException(string.Format("Not an entity type {0}.", entityType.Name));
            }
            return result;
        }
    }
}
