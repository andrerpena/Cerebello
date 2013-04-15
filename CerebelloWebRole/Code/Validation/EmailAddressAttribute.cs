using System;
using System.ComponentModel.DataAnnotations;

namespace CerebelloWebRole.Code.Validation
{
    /// <summary>
    /// E-mail attribute for validating e-mails
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class EmailAddressAttribute : RegularExpressionAttribute
    {
        private const string PATTERN = @"^\w+([-+.]*[\w-]+)*@(\w+([-.]?\w+)){1,}\.\w{2,4}$";
        private const string MESSAGE = "O campo '{0}' não é um endereço de e-mail válido";

        public EmailAddressAttribute()
            : base(PATTERN)
        {
            // Setting the default error message.
            this.ErrorMessage = MESSAGE;
        }
    }
}