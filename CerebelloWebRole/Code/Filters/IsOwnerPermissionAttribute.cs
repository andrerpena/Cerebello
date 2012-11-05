using System.Web.Mvc;
using Cerebello.Model;

namespace CerebelloWebRole.Code.Filters
{
    /// <summary>
    /// Specifies that only the owner of the clinic has access to this resource.
    /// </summary>
    public class IsOwnerPermissionAttribute : PermissionAttribute
    {
        public override bool CanAccessResource(User user)
        {
            return user.IsOwner;
        }
    }
}
