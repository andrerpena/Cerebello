using System;
using System.ComponentModel.DataAnnotations;

namespace CerebelloWebRole.Code
{
    /// <summary>
    /// Url attribute for validating URLs
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class UrlAttribute : ValidationAttribute
    {
        public UrlAttribute()
        {
        }

        public override bool IsValid(object value)
        {
            var text = value as string;
            Uri uri;

            return (!string.IsNullOrWhiteSpace(text) && Uri.TryCreate(text, UriKind.Absolute, out uri));
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format("O campo {0} não é uma URL válida", name);
        }
    }
}