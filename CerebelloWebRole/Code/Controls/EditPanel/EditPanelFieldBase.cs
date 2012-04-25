using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CerebelloWebRole.Code.Controls
{
    public class EditPanelFieldBase
    {
        /// <summary>
        /// formato do conteúdo
        /// </summary>
        public Func<dynamic, object> Format { get; set; }

        /// <summary>
        /// Formato da descrição
        /// </summary>
        public Func<dynamic, object> FormatDescription { get; set; }

        public String Header { get; set; }
        public EditPanelFieldSize Size { get; set; }

        /// <summary>
        /// ಠ.ಠ
        /// Determina se este campo aparece sozinho na linha
        /// </summary>
        public bool ForeverAlone { get; set; }
    }
}
