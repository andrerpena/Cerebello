using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CerebelloWebRole.Code.Security
{
    public enum CreateUserResult
    {
        Ok,
        UserNameAlreadyInUse,
        CouldNotCreateUrlIdentifier,
        InvalidUserName
    }
}
