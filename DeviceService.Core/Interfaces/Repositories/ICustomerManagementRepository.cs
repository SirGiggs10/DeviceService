using Ayuda_Help_Desk.Dtos.Customer;
using Ayuda_Help_Desk.Dtos.General;
using Ayuda_Help_Desk.Helpers;
using Ayuda_Help_Desk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ayuda_Help_Desk.Interfaces
{
    public interface ICustomerManagementRepository
    {
        public Task<ReturnResponse> GetAllCustomers(int customerType);
        public Task<ReturnResponse> GetCustomers(UserParams userParams, int customertype);
        public Task<ReturnResponse> CreateCustomer(CustomerForCreationDto customerForCreation);
        public Task<ReturnResponse> SearchCustomer(string searchParams, UserParams userParams);
    }
}
