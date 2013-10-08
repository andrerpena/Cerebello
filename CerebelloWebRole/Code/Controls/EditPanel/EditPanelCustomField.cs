using System;

namespace CerebelloWebRole.Code
{
    public class EditPanelCustomField : EditPanelFieldBase
    {
        public EditPanelCustomField(Func<dynamic, object> format = null, string header = null, bool wholeRow = false)
        {
            this.Format = format;
            this.Header = header;
            this.WholeRow = wholeRow;
        }
    }
}