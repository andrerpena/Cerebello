using System;
using System.Transactions;

namespace CerebelloWebRole.Code
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class TransactionScopeAttribute : Attribute
    {
        public TransactionScopeOption ScopeOption { get; set; }

        public TransactionScopeAttribute()
        {
            this.ScopeOption = TransactionScopeOption.Required;
        }

        public TransactionScopeAttribute(TransactionScopeOption scopeOption)
        {
            this.ScopeOption = scopeOption;
        }
    }
}
