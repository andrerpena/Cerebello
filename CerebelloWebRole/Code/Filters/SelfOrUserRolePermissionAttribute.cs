namespace CerebelloWebRole.Code.Filters
{
    /// <summary>
    /// Specifies that only the owner of the informations can access them,
    /// or a member of the specified groups of users.
    /// </summary>
    public class SelfOrUserRolePermissionAttribute : UserRolePermissionAttribute
    {
        public SelfOrUserRolePermissionAttribute(UserRoleFlags roleFlags)
            : base(roleFlags)
        {
        }

        public override bool CanAccessResource(PermissionContext permissionContext)
        {
            return SelfPermissionAttribute.CanAccessResourceCore(permissionContext)
                   || base.CanAccessResource(permissionContext);
        }
    }
}
