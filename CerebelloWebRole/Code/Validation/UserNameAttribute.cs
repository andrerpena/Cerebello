using System;
using System.ComponentModel.DataAnnotations;

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
        private const string PATTERN = @"^[^\W\d](?:[_\.\-]?\w){3,}$";
        private const string MESSAGE = "Nome de usuário não é valido. Tem que ter no mínimo 4 caracteres, começar com letra, " + "e só pode conter letras, números e os caracteres '_', '-' e '.', sendo que esses últimos " + "não podem aparecer em sequência, e nem no final.";

        public UserNameAttribute()
            : base(PATTERN)
        {
            // Setting the default error message.
            this.ErrorMessage = MESSAGE;
        }
    }
}