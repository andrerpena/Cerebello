using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace CerebelloWebRole.Code.Security
{
    public class SecurityTokenHelper
    {
        public static string ToString(SecurityToken securityToken)
        {
            string plainSecurityToken = new JavaScriptSerializer().Serialize(securityToken);
            return CipherHelper.EncryptToBase64(plainSecurityToken);
        }

        public static SecurityToken FromString(string ciphertext)
        {
            try
            {
                var data = CipherHelper.DecryptFromBase64(ciphertext);
                return (SecurityToken)new JavaScriptSerializer().Deserialize(data, typeof(SecurityToken));
            }
            catch
            {
                throw new Exception("Invalid SecurityToken");
            }
        }
    }
}