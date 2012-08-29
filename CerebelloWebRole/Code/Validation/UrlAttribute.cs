using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace CallStack.Code.Validators
{
    /// <summary>
    /// Url attribute for validating URLs
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class UrlAttribute : ValidationAttribute
    {
        private const string pattern = @"^[^\W\d](?:[_\.\-]?\w){3,}$";
        private static string message = "O campo '{0}' não é uma URL válida";

        static UrlAttribute()
        {
            // necessary to enable client side validation
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(UrlAttribute), typeof(RegularExpressionAttributeAdapter));
        }

        public UrlAttribute()
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