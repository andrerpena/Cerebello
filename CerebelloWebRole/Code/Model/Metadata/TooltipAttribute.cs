using System;
using JetBrains.Annotations;

namespace CerebelloWebRole.Code
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
            this.HelpMessage = helpMessage;
            if (helpMessage == null) throw new ArgumentNullException("helpMessage");
        }
    }
}