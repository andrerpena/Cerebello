namespace CerebelloWebRole.Code
{
    /// <summary>
    /// Specifies that only the owner of the clinic has access to this resource.
    /// </summary>
    public class IsOwnerPermissionAttribute : PermissionAttribute
    {
        public override bool CanAccessResource(PermissionContext permissionContext)
        {
            var user = permissionContext.User;
            return user.IsOwner;
        }
    }
}
