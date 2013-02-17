using System;
using System.ComponentModel.DataAnnotations;

namespace CerebelloWebRole.Code.Mvc
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class DateOfBirthAttribute : DataTypeAttribute
    {
        public DateOfBirthAttribute()
            : base(DataType.Date)
        {

        }

        public override bool IsValid(object value)
        {
            if (value == null)
                return true;

            var dateOfBirth = (DateTime)value;

            // todo: this must be the current practice local time-zone date and time.
            var now = DateTime.Now + DebugConfig.CurrentTimeOffset;

            return (dateOfBirth < now + TimeSpan.FromHours(3)) && (now.Year - dateOfBirth.Year < 150);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            return base.IsValid(value, validationContext);
        }

        public override string FormatErrorMessage(string name)
        {
            // ToDo: Make this validation message come from the RESX
            return string.Format("O campo '{0}' não possui uma data de nascimento válida", name);
        }
    }
}
