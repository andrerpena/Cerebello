using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CerebelloWebRole.Tests
{
    internal static class ExceptionExtensions
    {
        public static string FlattenMessages(this Exception ex, string separator = "\n\n")
        {
            List<string> messages = new List<string>();
            var ex1 = ex;
            while (ex1 != null)
            {
                messages.Add(ex1.Message);
                ex1 = ex1.InnerException;
            }
            var result = string.Join(separator, messages);
            return result;
        }
    }
}
