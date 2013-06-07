using System.Collections.Generic;

namespace CerebelloWebRole.Code
{
    /// <summary>
    /// Modelo das opções do plug-in do jQuery
    /// </summary>
    public class AutocompleteOptions
    {
        public AutocompleteOptions()
        {
            this.columns = new List<string>();
            this.columnHeaders = new List<string>();
        }

        // ReSharper disable InconsistentNaming
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public bool noFilterOnDropDown { get; set; }
        public string contentUrl { get; set; }
        public string inputHiddenId { get; set; }
        public string inputHiddenName { get; set; }
        public object inputHiddenValue { get; set; }
        public string inputTextId { get; set; }
        public string inputTextName { get; set; }
        public string inputTextValue { get; set; }
        public string columnId { get; set; }
        public string columnText { get; set; }
        public List<string> columns { get; private set; }
        public List<string> columnHeaders { get; private set; }
        public string newWindowUrl { get; set; }
        public int newWindowWidth { get; set; }
        public int newWindowMinHeight { get; set; }
        public string newWindowTitle { get; set; }
        // ReSharper restore InconsistentNaming
        // ReSharper restore UnusedAutoPropertyAccessor.Global
    }
}
