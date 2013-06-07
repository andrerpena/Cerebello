using System.ComponentModel;

namespace CerebelloWebRole.Code
{
    public class JsonDeleteMessage
    {
        public bool success { get; set; }

        [Localizable(true)]
        public string text { get; set; }
    }
}