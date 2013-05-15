using System.Windows.Forms;

namespace Cerebello.SolutionSetup
{
    public partial class Form1 : Form
    {
        private SetupInfo setupInfo;

        public Form1(SetupInfo setupInfo)
        {
            this.setupInfo = setupInfo;
            this.InitializeComponent();
            this.propertyGrid1.SelectedObject = this.setupInfo.Props;
        }
    }
}
