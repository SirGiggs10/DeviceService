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
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly DataContext _context;
        public readonly ICustomerRepository _customerRepository;
        public readonly IAuthRepository _authRepository;
        public readonly IConfiguration _configuration;
        public readonly IMapper _mapper;
        private readonly IAuditReportRepository _auditReportRepository;

        public CustomersController(DataContext context, ICustomerRepository customerRepository, IAuthRepository authRepository, IConfiguration configuration, IMapper mapper, IAuditReportRepository auditReportRepository)
        {
            _context = context;
            _customerRepository = customerRepository;
            _authRepository = authRepository;
            _configuration = configuration;
            _mapper = mapper;
            _auditReportRepository = auditReportRepository;
        }

        // GET: api/Customers
        [RequiredFunctionalityName("GetCustomers")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomer()
        {
            var customers =  await _context.Customer.ToListAsync();
            var customersToReturn = _mapper.Map<List<CustomerResponse>>(customers);
            //AUDIT THIS ACTIVITY FOR THE USER
            var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
            {
                AuditReportActivityFunctionalityName = "GetCustomers",
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

            return StatusCode(StatusCodes.Status200OK, customersToReturn);
        }

        // GET: api/Customers/5
        [RequiredFunctionalityName("GetCustomer")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomer(int id)
        {
            var customer = await _context.Customer.FindAsync(id);

            if (customer == null)
            {
                return NotFound();
            }

            var customerToReturn = _mapper.Map<CustomerResponse>(customer);
            //AUDIT THIS ACTIVITY FOR THE USER
            var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
            {
                AuditReportActivityFunctionalityName = "GetCustomer",
                AuditReportActivityResourceId = new List<int>() { customerToReturn.CustomerId }
            });

            if (auditResult.StatusCode != Utils.Success)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ReturnResponse()
                {
                    StatusCode = Utils.AuditReportError,
                    StatusMessage = Utils.StatusMessageAuditReportError
                });
            }

            return StatusCode(StatusCodes.Status200OK, customerToReturn);
        }

        // PUT: api/Customers/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [RequiredFunctionalityName("PutCustomer")]
        [HttpPut]
        public async Task<IActionResult> PutCustomer([FromBody] CustomerUpdateRequest customerUpdateRequest)
        {
            var dbTransaction = await _context.Database.BeginTransactionAsync();
            var result = await _customerRepository.UpdateCustomer(customerUpdateRequest);

            if (result.StatusCode == Utils.Success)
            {
                var customer = _mapper.Map<CustomerResponse>((Customer)result.ObjectValue);
                result.ObjectValue = customer;
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "PutCustomer",
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

        // PUT: api/Customers/Picture
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [RequiredFunctionalityName("PutCustomerPicture")]
        [HttpPut("Picture")]
        public async Task<ActionResult<ReturnResponse>> PutCustomerPicture([FromForm] CustomerPictureRequest customerPictureRequest)
        {
            var dbTransaction = await _context.Database.BeginTransactionAsync();
            var result = await _customerRepository.SetProfilePicture(customerPictureRequest);

            if (result.StatusCode == Utils.Success)
            {
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "PutCustomerPicture",
                    AuditReportActivityResourceId = new List<int>() { ((CustomerResponse)((object)result.ObjectValue)).CustomerId }
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
        /// REGISTER A NEW CUSTOMER TO THE SYSTEM
        /// </summary>
        [AllowAnonymous]
        //[RequiredFunctionalityName("PostRegisterCustomer")]
        [HttpPost("Register")]
        public async Task<ActionResult> PostRegisterCustomer(CustomerForRegisterDto customerForRegisterDto)
        {
            var dbTransaction = await _context.Database.BeginTransactionAsync();
            var result = await _customerRepository.CreateCustomer(customerForRegisterDto);

            if (result.StatusCode == Utils.Success)
            {
                var customerObj = (Customer)result.ObjectValue;
                var customer = _mapper.Map<CustomerResponse>(customerObj);
                result.ObjectValue = customer;
               // AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "PostRegisterCustomer",
                    AuditReportActivityResourceId = new List<int>() { customer.CustomerId },
                    UserId = customerObj.UserId
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

        // DELETE: api/Customers/5
        [RequiredFunctionalityName("DeleteCustomer")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Customer>> DeleteCustomer(int id)
        {
            var dbTransaction = await _context.Database.BeginTransactionAsync();
            var customer = await _context.Customer.FindAsync(id);
            if (customer == null)
            {
                await dbTransaction.RollbackAsync();

                return NotFound();
            }

            _context.Customer.Remove(customer);
            await _context.SaveChangesAsync();
            //AUDIT THIS ACTIVITY FOR THE USER
            var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
            {
                AuditReportActivityFunctionalityName = "DeleteCustomer",
                AuditReportActivityResourceId = new List<int>() { id }
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

            return customer;
        }

        private bool CustomerExists(int id)
        {
            return _context.Customer.Any(e => e.CustomerId == id);
        }
    }
}
