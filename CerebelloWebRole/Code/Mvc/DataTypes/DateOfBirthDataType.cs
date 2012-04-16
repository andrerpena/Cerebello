using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Web.Mvc;
using CerebelloWebRole.Code;

namespace CommonLib.Mvc.DataTypes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple  = false)]
    public class DateOfBirthAttribute : DataTypeAttribute
    {
        public DateOfBirthAttribute() : base(DataType.Date)
        {

        }

        public override bool IsValid(object value)
        {
            if (value == null)
                return true;

            var dateOfBirth = (DateTime) value;
            return (dateOfBirth < DateTimeHelper.GetTimeZoneNow()) && (DateTimeHelper.GetTimeZoneNow().Year - dateOfBirth.Year < 150);
        }

        public override string FormatErrorMessage(string name)
        {
            // ToDo: Make this validation message come from the RESX
            return string.Format("O campo '{0}' não possui uma data de nascimento válida", name);
        }
    }
}
