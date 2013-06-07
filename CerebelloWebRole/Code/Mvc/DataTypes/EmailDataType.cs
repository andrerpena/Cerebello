using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CerebelloWebRole.Code
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple  = false)]
    public class EmailAddressAttribute : DataTypeAttribute
    {
        private readonly Regex regex = new Regex(@"^\S+@\S+\.\S+", RegexOptions.Compiled);

        public EmailAddressAttribute() : base(DataType.EmailAddress)
        {

        }

        public override bool IsValid(object value)
        {
            if (value == null)
                return true;

            Match match = this.regex.Match((string) value);
            return match.Success;
        }

        public override string FormatErrorMessage(string name)
        {
            // ToDo: Make this validation message come from the RESX
            return string.Format("O campo '{0}' não possui um endereço de e-mail válido", name);
        }
    }
}
