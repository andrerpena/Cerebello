using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace CerebelloWebRole.Code.Validation
{
    /// <summary>
    /// Specifies that a property is an user-name that can be used to login.
    /// User-names follow some restrictions:
    /// - must start with letter;
    /// - may contain letters, digits or '-', '.' or '_';
    /// - may not contain '-', '.' or '_' close to each other;
    /// - may not end with '-', '.' or '_';
    /// - at least 4 letters/digits.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class UserNameAttribute : RegularExpressionAttribute
    {
        private const string pattern = @"^[^\W\d](?:[_\.\-]?\w){3,}$";
        private static string message = "Nome de usuário não é valido. Tem que ter no mínimo 4 caracteres, começar com letra, "
                        + "e só pode conter letras, números e os caracteres '_', '-' e '.', sendo que esses últimos "
                        + "não podem aparecer em sequência, e nem no final.";

        static UserNameAttribute()
        {
            // necessary to enable client side validation
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(UserNameAttribute), typeof(RegularExpressionAttributeAdapter));
        }

        public UserNameAttribute()
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