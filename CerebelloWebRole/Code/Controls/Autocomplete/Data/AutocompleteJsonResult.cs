using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace CerebelloWebRole.Code.Controls
{
    /// <summary>
    /// Representa o retorno JSON de uma chamada Ajax para popular o Lookup
    /// </summary>
    public class AutocompleteJsonResult
    {
        public AutocompleteJsonResult()
        {
            this.Rows = new ArrayList();
        }

        public int Count { get; set; }
        public ArrayList Rows { get; set; }
    }
}
