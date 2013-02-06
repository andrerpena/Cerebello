using System.Configuration;
using System.Web;

namespace CerebelloWebRole.Code
{
    // todo: this class must be injectable... remove the Instance property and pass it everywhere.
    public class Configuration
    {
        public static Configuration Instance
        {
            get { return new Configuration(); }
        }

        public virtual bool IsLocalPresentation
        {
            get
            {
                return (HttpContext.Current == null || HttpContext.Current.Request.Url.IsLoopback)
                    && ConfigurationManager.AppSettings["IsLocalPresentation"].ToLowerInvariant() == "true";
            }
        }
    }
}