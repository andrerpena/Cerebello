using System.Xml.Serialization;

namespace Cerebello.Firestarter
{
    [XmlRoot("firestarter", IsNullable = false)]
    public class FirestarterConfigData
    {
        [XmlAttribute("configLocation")]
        public string ConfigLocation { get; set; }
        
        [XmlElement("formSendEmail")]
        public FormSendEmailConfigData FormSendEmail { get; set; }
    }
}
