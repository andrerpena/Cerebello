using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JetBrains.Annotations;

namespace CerebelloWebRole.Code.Model.Metadata
{
    [AttributeUsage(AttributeTargets.Property)]
    public class TooltipAttribute : Attribute
    {
        /// <summary>
        /// Help message
        /// </summary>
        public string HelpMessage { get; set; }

        public TooltipAttribute([NotNull] string helpMessage)
        {
            HelpMessage = helpMessage;
            if (helpMessage == null) throw new ArgumentNullException("helpMessage");
        }
    }
}