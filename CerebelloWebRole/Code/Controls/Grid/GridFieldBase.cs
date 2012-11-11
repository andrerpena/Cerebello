using System;

namespace CerebelloWebRole.Code.Controls
{
    public class GridFieldBase
    {
        public Func<dynamic, object> Format { get; set; }
        public String Header { get; set; }
        public bool CanSort { get; set; }
        public bool WordWrap { get; set; }
        public string CssClass { get; set; }
    }
}
