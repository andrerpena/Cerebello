using System.Collections.Generic;

namespace CerebelloWebRole.Code.Controls
{
    /// <summary>
    /// Modelo das opções do plug-in do jQuery
    /// </summary>
    public class LookupOptions
    {
        public LookupOptions()
        {
            this.columns = new List<string>();
            this.columnHeaders = new List<string>();
        }

        public string contentUrl { get; set; }
        public string inputHiddenId { get; set; }
        public string inputHiddenName { get; set; }
        public object inputHiddenValue { get; set; }
        public string inputTextId { get; set; }
        public string inputTextName { get; set; }
        public string inputTextValue { get; set; }
        public string columnId { get; set; }
        public string columnText { get; set; }
        public List<string> columns { get; set; }
        public List<string> columnHeaders { get; set; }
    }
}
