using Ayuda_Help_Desk.Dtos.General;
using Ayuda_Help_Desk.Dtos.Customer;
using Ayuda_Help_Desk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ayuda_Help_Desk.Interfaces
{
    public interface ICustomerRepository
    {
        public Task<ReturnResponse> CreateCustomer(CustomerForRegisterDto customer);
        public Task<ReturnResponse> UpdateCustomer(CustomerUpdateRequest customerUpdateRequest);
        public Task<ReturnResponse> SetProfilePicture(CustomerPictureRequest customerPictureRequest);
    }
}
