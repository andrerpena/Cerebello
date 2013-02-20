using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;

namespace CerebelloWebRole.Models
{
    public class InternalUpgradeEmailViewModel : UserEmailViewModel
    {
        public InternalUpgradeEmailViewModel(User user, ChangeContractViewModel upgradeData)
            : base(user)
        {
            this.Upgrade = upgradeData;
        }

        public ChangeContractViewModel Upgrade { get; set; }
    }
}
