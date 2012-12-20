using System.Linq;
using System.Web.Mvc;
using Cerebello.Model;

namespace CerebelloWebRole.Code.Filters
{
    /// <summary>
    /// Specifies that only the owner of the informations can access them.
    /// </summary>
    public class SelfOrUserRolePermissionAttribute : UserRolePermissionAttribute
    {
        public override bool CanAccessResource(PermissionContext permissionContext)
        {
            var user = permissionContext.User;
            var controller = permissionContext.ControllerContext.Controller;
            if (controller is DoctorController)
            {
                var doctorController = controller as DoctorController;
                doctorController.InitDoctor();
                var result = doctorController.Doctor.Users.Any(u => u.Id == user.Id);
                return result;
            }

            return base.CanAccessResource(permissionContext);
        }
    }
}
