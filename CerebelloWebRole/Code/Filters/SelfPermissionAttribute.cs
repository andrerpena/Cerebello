namespace CerebelloWebRole.Code.Filters
{
    /// <summary>
    /// Specifies that only the owner of the informations can access them.
    /// </summary>
    public class SelfPermissionAttribute : PermissionAttribute
    {
        public override bool CanAccessResource(PermissionContext permissionContext)
        {
            return CanAccessResourceCore(permissionContext);
        }

        internal static bool CanAccessResourceCore(PermissionContext permissionContext)
        {
            var user = permissionContext.User;
            var controller = permissionContext.ControllerContext.Controller;
            if (controller is PracticeController)
            {
                var practiceController = controller as PracticeController;
                practiceController.InitSelfUserId();
                var result = practiceController.SelfUserId == user.Id;
                return result;
            }

            return false;
        }
    }
}
