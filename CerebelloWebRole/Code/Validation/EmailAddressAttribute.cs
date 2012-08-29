using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace CallStack.Code.Validators
{
    /// <summary>
    /// E-mail attribute for validating e-mails
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class EmailAddressAttribute : RegularExpressionAttribute
    {
        private const string pattern = @"^\w+([-+.]*[\w-]+)*@(\w+([-.]?\w+)){1,}\.\w{2,4}$";
        private static string message = "O campo '{0}' não é um endereço de e-mail válido";

        static EmailAddressAttribute()
        {
            // necessary to enable client side validation
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(EmailAddressAttribute), typeof(RegularExpressionAttributeAdapter));
        }

        public EmailAddressAttribute()
            : base(pattern)
        {
            // Setting the default error message.
            this.ErrorMessage = message;
        }
        public override bool IsValid(object value)
        {
            return base.IsValid(value);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            return base.IsValid(value, validationContext);
        }
    }
}