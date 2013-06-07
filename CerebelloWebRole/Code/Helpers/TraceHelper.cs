using System;
using System.Text;

namespace CerebelloWebRole.Code
{
    public static class TraceHelper
    {
        public static string GetExceptionMessage(Exception ex)
        {
            var result = new StringBuilder();
            var curEx = ex;
            while (curEx != null)
            {
                if (curEx != ex)
                    result.Append(' ');

                result.AppendFormat("[{0}] {1}", curEx.GetType().Name, curEx.Message);

                curEx = curEx.InnerException;
            }

            return result.ToString();
        }
    }
}
