using Cerebello.Model;

namespace CerebelloWebRole.Code.Filters
{
    /// <summary>
    /// Specifies that only users allowed to manager the users of the clinic have access to this resource.
    /// </summary>
    public class UsersManagementPermissionAttribute : PermissionAttribute
    {
        public override bool CanAccessResource(PermissionContext permissionContext)
        {
            var user = permissionContext.User;
            return user.AdministratorId != null || user.IsOwner;
        }
    }
}
