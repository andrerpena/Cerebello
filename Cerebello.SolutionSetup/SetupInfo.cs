using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
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

        public class Version
        {
            public Version()
            {
            }

            public Version(int value)
            {
                this.Value = value;
            }

            public int? Value { get; set; }

            [XmlAttribute("value")]
            public int XmlValue { get { return this.Value.Value; } set { this.Value = value; } }

            public bool XmlValueSpecified { get { return this.Value.HasValue; } }
        }

        [XmlElement("windows-azure-targets")]
        public Version WindowsAzureTargets { get; set; }

        [XmlElement("firestarter-uncommited-config")]
        public Version FirestarterUncommitedConfig { get; set; }

        [XmlElement("tests-uncommited-config")]
        public Version TestsUncommitedConfig { get; set; }

        public class Properties
        {
            [XmlElement("operator-name")]
            [Description("Name used to identify the operator of this computer. "
                + "This will be used by default when sending e-mails to clients using firestarter.")]
            public string OperatorName { get; set; }

            [XmlIgnore] // use current path
            public string SolutionPath { get; internal set; }

            [Browsable(false)]
            public PathStatic Path { get { return new PathStatic(); } }

            public class PathStatic
            {
                public string Combine(string a, string b)
                {
                    return System.IO.Path.Combine(a, b);
                }
            }
        }

        [XmlElement("user-values")]
        public Properties Props { get; set; }
    }

    public static class SetupInfoExtensions
    {
        public static bool IsGreaterOrEqual(this SetupInfo.Version ver, int value)
        {
            if (ver == null)
                return false;

            return ver.Value >= value;
        }
    }
}
