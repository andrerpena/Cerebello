using System;
using System.Web.Script.Serialization;

namespace CerebelloWebRole.Code
{
    public static class SecurityTokenHelper
    {
        public static string ToString(SecurityToken securityToken)
        {
            var plainSecurityToken = new JavaScriptSerializer().Serialize(securityToken);
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