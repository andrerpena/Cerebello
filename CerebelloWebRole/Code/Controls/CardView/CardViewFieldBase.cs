using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CerebelloWebRole.Code.Controls
{
    public class CardViewFieldBase
    {
        public Func<dynamic, object> Format { get; set; }
        public String Header { get; set; }

        /// <summary>
        /// ಠ.ಠ
        /// Determina se este campo aparece sozinho na linha
        /// </summary>
        public bool WholeRow { get; set; }
    }
}
