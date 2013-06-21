using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace Cerebello.SolutionSetup
{
    [XmlRoot("solution-setup", IsNullable = false)]
    public class SetupInfo
    {
        public SetupInfo()
        {
            this.Props = new Properties();
        }

        [XmlElement("version")]
        public string Version { get; set; }

        /// <summary>
        /// Properties to show in the configuration window, in a property grid.
        /// </summary>
        public class Properties
        {
            [XmlElement("operator-name")]
            [Description("Name used to identify the operator of this computer. "
                + "This will be used by default when sending e-mails to clients using firestarter.")]
            public string OperatorName { get; set; }

            [XmlIgnore] // use current path
            [Description("Path of the solution.")]
            [ReadOnly(true)]
            public string SolutionPath { get; set; }

            [XmlIgnore]
            [Description("Path of the desktop for the operator of this computer. "
                + "This is used as an obvious location for debug purposes.")]
            [ReadOnly(true)]
            public string DesktopPath { get; set; }
        }

        [XmlElement("user-values")]
        public Properties Props { get; set; }

        [XmlIgnore]
        public FileInfo[] AzureTargets { get; set; }

        [XmlElement("windows-azure-targets-version")]
        public int WindowsAzureTargetsVersion { get; set; }
    }
}
