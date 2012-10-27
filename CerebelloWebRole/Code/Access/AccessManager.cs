using System;
using System.Linq;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Code.Filters;
using CerebelloWebRole.Code.Helpers;
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
                if (obj == null) return true;

                return db.Users.Any(u => u.Id == op.Id
                                         &&
                                         u.Practice.Users.Any(
                                             u2 => u2.Doctor.ActiveIngredients.Any(ai => ai.Id == obj.Id)));
            }

            public static bool Check(CerebelloEntities db, User op, Address obj)
            {
                if (obj == null) return true;

                return db.Users.Any(u => u.Id == op.Id
                                         && u.Practice.Users.Any(u2 => u2.Person.Address.Id == obj.Id));
            }

            public static bool Check(CerebelloEntities db, User op, Administrator obj)
            {
                if (obj == null) return true;

                return db.Users.Any(u => u.Id == op.Id
                                         && u.Practice.Users.Any(u2 => u2.AdministratorId == obj.Id));
            }

            public static bool Check(CerebelloEntities db, User op, Anamnese obj)
            {
                if (obj == null) return true;

                return db.Users.Any(u => u.Id == op.Id
                                         &&
                                         u.Practice.Users.Any(
                                             u2 => u2.Doctor.Patients.Any(p => p.Anamneses.Any(an => an.Id == obj.Id))));
            }

            public static bool Check(CerebelloEntities db, User op, Appointment obj)
            {
                if (obj == null) return true;

                return db.Users.Any(u => u.Id == op.Id
                                         &&
                                         u.Practice.Users.Any(u2 => u2.Doctor.Appointments.Any(ap => ap.Id == obj.Id)));
            }

            public static bool Check(CerebelloEntities db, User op, CFG_DayOff obj)
            {
                if (obj == null) return true;

                return db.Users.Any(u => u.Id == op.Id
                                         &&
                                         u.Practice.Users.Any(u2 => u2.Doctor.CFG_DayOff.Any(doff => doff.Id == obj.Id)));
            }

            public static bool Check(CerebelloEntities db, User op, CFG_Documents obj)
            {
                if (obj == null) return true;

                // Only the doctor can change his documents configuration.
                return db.Users.Any(u => u.Id == op.Id
                                         && u.DoctorId == obj.DoctorId);
            }

            public static bool Check(CerebelloEntities db, User op, CFG_Schedule obj)
            {
                if (obj == null) return true;

                // Only the doctor can change his schedule.
                return db.Users.Any(u => u.Id == op.Id
                                         && u.DoctorId == obj.DoctorId);
            }

            public static bool Check(CerebelloEntities db, User op, ChatMessage obj)
            {
                if (obj == null) return true;

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
                if (obj == null) return true;

                // If both users are in the same practice, then they can see each other.
                return db.Users.Any(u => u.Id == op.Id
                                         && u.Practice.Users.Any(u2 => u2.Id == obj.Id));
            }

            public static bool Check(CerebelloEntities db, User op, Secretary obj)
            {
                if (obj == null) return true;

                return db.Users.Any(u => u.Id == op.Id
                                         && u.Practice.Users.Any(u2 => u2.SecretaryId == obj.Id));
            }

            public static bool Check(CerebelloEntities db, User op, Doctor obj)
            {
                if (obj == null) return true;

                return db.Users.Any(u => u.Id == op.Id
                                         && u.Practice.Users.Any(u2 => u2.DoctorId == obj.Id));
            }
        }

        /// <summary>
        /// Finds out whether user can access the specified action.
        /// At this moment it looks only at PermissionAttribute attributes.
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

            // TODO: there is much to be improved in this method:
            // - Use global filters
            // - Use the controller itself as a filter
            // - Use attributes that are filters, not only derived from PermissionAttribute.

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
    }
}
