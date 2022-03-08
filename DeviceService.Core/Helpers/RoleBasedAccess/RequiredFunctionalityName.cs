using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceService.Core.Helpers.RoleBasedAccess
{
    public class RequiredFunctionalityName : AuthorizeAttribute
    {
        string functionalityName;
        public RequiredFunctionalityName(string functionalityName)
        {
            FunctionalityName = functionalityName;
        }

        public string FunctionalityName
        {
            get
            {
                return functionalityName;
            }
            set
            {
                functionalityName = value;
                Policy = $"Functionality.{functionalityName}";
            }
        }
    }
}
