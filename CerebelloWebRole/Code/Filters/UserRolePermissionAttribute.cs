using System;
using System.Web.Mvc;
using Cerebello.Model;

namespace CerebelloWebRole.Code.Filters
{
    /// <summary>
    /// Specifies that an user with any of the roles specified in the clinic has access to this resource.
    /// This is an OR operation, not an AND.
    /// </summary>
    public class UserRolePermissionAttribute : PermissionAttribute
    {
        public UserRoleFlags RoleFlags { get; set; }

        public override bool CanAccessResource(PermissionContext permissionContext)
        {
            var user = permissionContext.User;
            UserRoleFlags userRoles = 0;
            userRoles |= user.IsOwner ? UserRoleFlags.Owner : 0;
            userRoles |= user.AdministratorId != null ? UserRoleFlags.Administrator : 0;
            userRoles |= user.SecretaryId != null ? UserRoleFlags.Secretary : 0;
            userRoles |= user.DoctorId != null ? UserRoleFlags.Doctor : 0;

            return (userRoles & this.RoleFlags) != 0;
        }
    }

    [Flags]
    public enum UserRoleFlags
    {
        None = 0,
        Secretary = 1,
        Administrator = 2,
        Owner = 4,
        Doctor = 8,
    }
}
