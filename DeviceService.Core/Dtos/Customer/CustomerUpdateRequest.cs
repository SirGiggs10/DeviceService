using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ayuda_Help_Desk.Dtos.Customer
{
    public class CustomerUpdateRequest
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public int CustomerTypeId { get; set; }
    }
}
