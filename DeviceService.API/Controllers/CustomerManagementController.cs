using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ayuda_Help_Desk.Data;
using Ayuda_Help_Desk.Models;
using Ayuda_Help_Desk.Interfaces;
using Ayuda_Help_Desk.Dtos.Customer;
using Ayuda_Help_Desk.Helpers;
using Microsoft.AspNetCore.Authorization;
using Ayuda_Help_Desk.Dtos.General;
using Microsoft.Extensions.Configuration;
using Ayuda_Help_Desk.Dtos;
using AutoMapper;
using Ayuda_Help_Desk.Dtos.Auth;
using Ayuda_Help_Desk.Dtos.UserManagement;
using Ayuda_Help_Desk.Dtos.AuditReport;

namespace Ayuda_Help_Desk.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class CustomerManagementController : ControllerBase
    {
        private readonly DataContext _context;
        public readonly ICustomerManagementRepository _customerManagementRepository;
        public readonly IAuthRepository _authRepository;
        public readonly IConfiguration _configuration;
        public readonly IMapper _mapper;
        private readonly IAuditReportRepository _auditReportRepository;

        public CustomerManagementController(DataContext context, ICustomerManagementRepository customerManagementRepository, IAuthRepository authRepository, IConfiguration configuration, IMapper mapper, IAuditReportRepository auditReportRepository)
        {
            _context = context;
            _customerManagementRepository = customerManagementRepository;
            _authRepository = authRepository;
            _configuration = configuration;
            _mapper = mapper;
            _auditReportRepository = auditReportRepository;
        }

        // GET: api/CustomerManagement/{customerType}
        [RequiredFunctionalityName("GetCustomersFromManagement")]
        [HttpGet("{customerType}")]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomer([FromQuery] UserParams userParams, int customerType)
        {
            var result = await _customerManagementRepository.GetCustomers(userParams, customerType);

            if (result.StatusCode == Utils.Success)
            {
                var customers = (PagedList<Customer>)result.ObjectValue;
                result.ObjectValue = _mapper.Map<List<CustomerResponse>>(customers.ToList());
                Response.AddPagination(customers.CurrentPage, customers.PageSize, customers.TotalCount, customers.TotalPages);

                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "GetCustomersFromManagement",
                    AuditReportActivityResourceId = new List<int>() { }
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new ReturnResponse()
                    {
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }

                return StatusCode(StatusCodes.Status200OK, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }

        }

        // GET: api/CustomerManagement/All/{customerType}
        [RequiredFunctionalityName("GetAllCustomersFromManagement")]
        [HttpGet("All/{customerType}")]
        public async Task<ActionResult<IEnumerable<Customer>>> GetAllCustomer([FromQuery]  int customerType)
        {
            var result = await _customerManagementRepository.GetAllCustomers(customerType);

            if (result.StatusCode == Utils.Success)
            {
                var customers = (PagedList<Customer>)result.ObjectValue;
                result.ObjectValue = _mapper.Map<List<CustomerResponse>>(customers);
               
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "GetCustomersFromManagement",
                    AuditReportActivityResourceId = new List<int>() { }
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new ReturnResponse()
                    {
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }

                return StatusCode(StatusCodes.Status200OK, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }

        }

        /// <summary>
        /// REGISTER A NEW CUSTOMER TO THE SYSTEM
        /// </summary>
        ///  // POST: api/CustomerManagement/Create
        [RequiredFunctionalityName("PostCreateCustomerFromManagement")]
        [HttpPost("Create/")]
        public async Task<ActionResult> PostCreateCustomer(CustomerForCreationDto customerForCreationDto)
        {
            var dbTransaction = await _context.Database.BeginTransactionAsync();
            var result = await _customerManagementRepository.CreateCustomer(customerForCreationDto);

            if (result.StatusCode == Utils.Success)
            {
                var customer = _mapper.Map<CustomerResponse>((Customer)result.ObjectValue);
                result.ObjectValue = customer;
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "GetCustomersFromManagement",
                    AuditReportActivityResourceId = new List<int>() { customer.CustomerId }
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    await dbTransaction.RollbackAsync();

                    return StatusCode(StatusCodes.Status400BadRequest, new ReturnResponse()
                    {
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }

                await dbTransaction.CommitAsync();

                return StatusCode(StatusCodes.Status200OK, result);
            }
            else
            {
                await dbTransaction.RollbackAsync();

                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        /// <summary>
        /// SEARCH FOR A CUSTOMER IN THE SYSTEM
        /// </summary>
        // GET: api/CustomerManagement/Search/{searchParams}
        [RequiredFunctionalityName("SearchCustomerFromManagement")]
        [HttpGet("Search/{searchParams}")]
        public async Task<ActionResult> SearchCustomer(string searchParams, [FromQuery] UserParams userParams)
        {
            var result = await _customerManagementRepository.SearchCustomer(searchParams, userParams);

            if (result.StatusCode == Utils.Success)
            {
                var customers = (PagedList<Customer>)result.ObjectValue;
                result.ObjectValue = _mapper.Map<List<CustomerResponse>>(customers.ToList());
                Response.AddPagination(customers.CurrentPage, customers.PageSize, customers.TotalCount, customers.TotalPages);

                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "SearchCustomerFromManagement",
                    AuditReportActivityResourceId = new List<int>() { }
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new ReturnResponse()
                    {
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }

                return StatusCode(StatusCodes.Status200OK, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }
    }
}