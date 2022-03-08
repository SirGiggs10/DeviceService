using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ayuda_Help_Desk.Dtos.Customer
{
    public class CustomerPasswordRequest
    {
        public string EmailAddress { get; set; }
        public string Password { get; set; }
    }
}
