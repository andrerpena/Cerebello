using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CerebelloWebRole.Code.Security.Principals
{
    /// <summary>
    /// User information that is stored in the authenticatiojn 
    /// </summary>
    public class UserData
    {
        public int Id { get; set; }
        public String Email { get; set; }
        public String FullName { get; set; }
    }
}