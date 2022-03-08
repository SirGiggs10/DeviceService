using AutoMapper;
using Ayuda_Help_Desk.Data;
using Ayuda_Help_Desk.Dtos;
using Ayuda_Help_Desk.Dtos.General;
using Ayuda_Help_Desk.Helpers;
using Ayuda_Help_Desk.Interfaces;
using Ayuda_Help_Desk.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Ayuda_Help_Desk.Repositories
{
    public class CustomerTypeRepository : ICustomerTypeRepository
    {
        private readonly DataContext _context;
        private readonly IGlobalRepository _globalRepository;
        public readonly IAuthRepository _authRepository;
        private readonly IMapper _mapper;
        public CustomerTypeRepository(DataContext context, IMapper mapper, IGlobalRepository globalRepository, IAuthRepository authRepository)
        {
            _context = context;
            _globalRepository = globalRepository;
            _mapper = mapper;
            _authRepository = authRepository;
        }

        public async Task<ReturnResponse> GetAllCustomerTypes(UserParams userParams)
        {
            var CustomerTypes = _context.CustomerType.Where(b => b.Deleted != Utils.Deleted);
            if (CustomerTypes == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = "No CustomerType(s) Found!!!",
                    ObjectValue = CustomerTypes
                };
            }
            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = await PagedList<CustomerType>.CreateAsync(CustomerTypes, userParams.PageNumber, userParams.PageSize),
                StatusMessage = Utils.StatusMessageSuccess
            };
        }

        public async Task<ReturnResponse> GetCustomerType(int id)
        {
            var CustomerType = await _context.CustomerType.Where(z => z.Deleted != Utils.Deleted && z.CustomerTypeId == id).FirstOrDefaultAsync();
            if(CustomerType == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = "No CustomerType(s) Found!!!",
                    ObjectValue = CustomerType
                };
            }
            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = CustomerType,
                StatusMessage = Utils.StatusMessageSuccess
            };
        }

        public async Task<ReturnResponse> DeleteCustomerType(List<int> ids)
        {
            foreach (var id in ids)
            {

                var CustomerType = await _context.CustomerType.FindAsync(id);
                if (CustomerType == null)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.ObjectNull,
                        StatusMessage = Utils.StatusMessageObjectNull
                    };
                }

                CustomerType.Deleted = Utils.Deleted;
                CustomerType.DeletedAt = DateTimeOffset.Now;
                _context.Entry(CustomerType).State = EntityState.Modified;

            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                StatusMessage = "CustomerType Deleted Successfully",

            };
        }

        public async Task<ReturnResponse> CreateCustomerType(CustomerTypeRequest CustomerTypeRequest)
        {
            if (CustomerTypeRequest == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            var CustomerType = _mapper.Map<CustomerType>(CustomerTypeRequest);

            _globalRepository.Add(CustomerType);
            var result = await _globalRepository.SaveAll();
            if (result != null)
            {
                if (!result.Value)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.SaveError,
                        StatusMessage = Utils.StatusMessageSaveError
                    };
                }

                return new ReturnResponse()
                {
                    StatusCode = Utils.Success,
                    ObjectValue = _mapper.Map<CustomerTypeResponse>(CustomerType),
                    StatusMessage = "CustomerType(s) Added Successfully!!!"
                };
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.SaveError,
                StatusMessage = Utils.StatusMessageSaveError
            };
        }

        public async Task<ReturnResponse> UpdateCustomerType(int id, CustomerTypeResponse CustomerTypeToUpdate)
        {
            if (id != CustomerTypeToUpdate.CustomerTypeId)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.BadRequest,
                    StatusMessage = Utils.StatusMessageBadRequest
                };
            }


            var CustomerType = await _context.CustomerType.FindAsync(id);
            _mapper.Map(CustomerTypeToUpdate, CustomerType);

            _context.Entry(CustomerType).State = EntityState.Modified;

            try
            {
                //  await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerTypeExists(id))
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = Utils.StatusMessageNotFound
                    };
                }
                else
                {
                    throw;
                }
            }

            await _context.SaveChangesAsync();
            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                StatusMessage = "CustomerType Updated Successfully",

            };
        }

        private bool CustomerTypeExists(int id)
        {
            return _context.CustomerType.Any(e => e.CustomerTypeId == id);
        }

        public async Task<ReturnResponse> SearchCustomerType(string searchParams, UserParams userParams)
        {
            var customerTypes = from sL in _context.CustomerType
                                  select sL;

            if (!String.IsNullOrEmpty(searchParams))
            {
                customerTypes = customerTypes.Where(s => s.CustomerTypeName.Contains(searchParams));
            }


            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                StatusMessage = "Search Successful!!!",
                ObjectValue = await PagedList<CustomerType>.CreateAsync(customerTypes, userParams.PageNumber, userParams.PageSize),
            };
        }

    }
}
