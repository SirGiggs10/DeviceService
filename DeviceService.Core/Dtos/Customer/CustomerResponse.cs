using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ayuda_Help_Desk.Dtos.Customer
{
    public class CustomerResponse
    {
        public int CustomerId { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public int CustomerTypeId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string CustomerProfilePictureUrl { get; set; }
        public string CustomerProfilePicturePublicId { get; set; }
        public CustomerTypeResponse CustomerType { get; set; }
    }
}
