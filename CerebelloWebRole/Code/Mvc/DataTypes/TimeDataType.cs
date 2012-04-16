using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Web.Mvc;

namespace CommonLib.Mvc.DataTypes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple  = false)]
    public class TimeDataTypeAttribute : DataTypeAttribute
    {
        public static Regex Regex = new Regex(@"^(\d{2}):(\d{2})$", RegexOptions.Compiled);

        public TimeDataTypeAttribute()
            : base("TimeString")
        {

        }

        public override bool IsValid(object value)
        {
            if (value == null)
                return true;

            Match match = Regex.Match((string) value);
            return match.Success && (int.Parse(match.Groups[1].Value) >= 0 && int.Parse(match.Groups[1].Value) <= 23) && (int.Parse(match.Groups[2].Value) >= 0 && int.Parse(match.Groups[2].Value) <= 59);
        }

        public override string FormatErrorMessage(string name)
        {
            // ToDo: Make this validation message come from the RESX
            return string.Format("O campo '{0}' não possui um valor válido", name);
        }
    }
}
