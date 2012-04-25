using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CerebelloWebRole.Code.Controls
{
    /// <summary>
    /// Representa uma linha do lookup
    /// </summary>
    public class LookupRow
    {
        /// <summary>
        /// Id
        /// </summary>
        public object Id { get; set; }

        /// <summary>
        /// Valor (texto)
        /// </summary>
        public string Value { get; set; }
    }
}
