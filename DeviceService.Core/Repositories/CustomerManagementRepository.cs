using AutoMapper;
using Ayuda_Help_Desk.Data;
using Ayuda_Help_Desk.Dtos.Customer;
using Ayuda_Help_Desk.Dtos.General;
using Ayuda_Help_Desk.Helpers;
using Ayuda_Help_Desk.Interfaces;
using Ayuda_Help_Desk.Models;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Ayuda_Help_Desk.Repositories
{
    public class CustomerManagementRepository : ICustomerManagementRepository
    {
        private readonly DataContext _context;
        private readonly IGlobalRepository _globalRepository;
        private readonly IMailController _mailController;
        private readonly IConfiguration _configuration;
        public readonly IAuthRepository _authRepository;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        
        public CustomerManagementRepository(DataContext context, IMapper mapper, UserManager<User> userManager,  IGlobalRepository globalRepository, IMailController mailController, IConfiguration configuration, IAuthRepository authRepository)
        {
            _context = context;
            _globalRepository = globalRepository;
            _mapper = mapper;
            _mailController = mailController;
            _userManager = userManager;
            _configuration = configuration;
            _authRepository = authRepository;
        }

        public async Task<ReturnResponse> GetCustomers(UserParams userParams, int customerType)
        {
            var customers = _context.Customer
                .Where(uT => uT.CustomerTypeId == customerType)
                .Include(a => a.CustomerType);

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = await PagedList<Customer>.CreateAsync(customers, userParams.PageNumber, userParams.PageSize),
                StatusMessage = Utils.StatusMessageSuccess
            };
        }

        public async Task<ReturnResponse> GetAllCustomers(int customerType)
        {
            var customers = _context.Customer
                .Where(uT => uT.CustomerTypeId == customerType)
                .Include(a => a.CustomerType);

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = customers,
                StatusMessage = Utils.StatusMessageSuccess
            };
        }

        public async Task<ReturnResponse> CreateCustomer(CustomerForCreationDto customerForCreation)
        {
            if (customerForCreation == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            if (await CustomerExists(customerForCreation.EmailAddress))
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectExists,
                    StatusMessage = Utils.StatusMessageObjectExists
                };
            }

            var customer = new Customer
            {
                EmailAddress = customerForCreation.EmailAddress,
                FullName = customerForCreation.FullName,
                PhoneNumber = customerForCreation.PhoneNumber,
                CustomerTypeId = customerForCreation.CustomerType
            };

            _globalRepository.Add(customer);
            var saveVal = await _globalRepository.SaveAll();


            if (saveVal != null)
            {
                if (!saveVal.Value)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.SaveNoRowAffected,
                        StatusMessage = Utils.StatusMessageSaveNoRowAffected
                    };
                }
                var user = new User
                {
                    UserName = customerForCreation.EmailAddress,
                    Email = customerForCreation.EmailAddress,
                    UserTypeId = customer.CustomerId,
                    UserType = Utils.Customer
                };
                var password = (new Helper()).RandomPassword();

                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    //ASSIGN (INDIVIDUAL OR CORPORATE) ROLE TO USER (CUSTOMER)
                    string customerTypeRole;
                    if (customerForCreation.CustomerType == Utils.Individual)
                    {
                        customerTypeRole = Utils.CustomerIndividualRole;
                    }
                    else if (customerForCreation.CustomerType == Utils.Corporate)
                    {
                        customerTypeRole = Utils.CustomerCorporateRole;
                    }
                    else
                    {
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.BadRequest,
                            StatusMessage = Utils.StatusMessageBadRequest
                        };
                    }

                    var assignmentResult = await _userManager.AddToRoleAsync(user, customerTypeRole);
                    if (assignmentResult.Succeeded)
                    {
                        //THEN UPDATE CUSTOMER TABLE USERID COLUMN WITH NEWLY CREATED USER ID
                        customer.UserId = user.Id;
                        var customerUpdateResult = _globalRepository.Update(customer);
                        if (!customerUpdateResult)
                        {
                            return new ReturnResponse()
                            {
                                StatusCode = Utils.NotSucceeded,
                                StatusMessage = Utils.StatusMessageNotSucceeded
                            };
                        }

                        var customerUpdateSaveResult = await _globalRepository.SaveAll();
                        if (!customerUpdateSaveResult.HasValue)
                        {
                            return new ReturnResponse()
                            {
                                StatusCode = Utils.SaveError,
                                StatusMessage = Utils.StatusMessageSaveError
                            };
                        }

                        if (!customerUpdateSaveResult.Value)
                        {
                            return new ReturnResponse()
                            {
                                StatusCode = Utils.SaveNoRowAffected,
                                StatusMessage = Utils.StatusMessageSaveNoRowAffected
                            };
                        }

                        //SEND MAIL TO CUSTOMER TO CONFIRM EMAIL
                        var userTokenVal = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        string hashedEmail = GetHashedEmail(user.Email);
                        /*var shortToken = userTokenVal.Substring(0, 7);
                        user.ShortToken = shortToken;
                        user.LongToken = userTokenVal;
                        _ = _globalRepository.SaveAll();*/
                        var fullToken = userTokenVal + "#" + hashedEmail;

                        var emailVerificationLink = _authRepository.GetUserEmailVerificationLink(fullToken);
                        if (emailVerificationLink == null)
                        {
                            return new ReturnResponse()
                            {
                                StatusCode = Utils.ObjectNull,
                                StatusMessage = Utils.StatusMessageObjectNull
                            };
                        }

                        var customerName = customerForCreation.FullName.Split();

                        var emailMessage1 = "Please click the button below to complete your registration and activate your account.";
                        var emailMessage2 = "Your Password is "+password;

                        string emailBody = _globalRepository.GetMailBodyTemplate(customerName[0], customerName[1], emailVerificationLink, emailMessage1, emailMessage2, "activation.html");

                        //string emailBody = "Email Verification Link: "+emailVerificationLink+"\nToken: "+hashedEmail+ "\nUsername: "+customerForCreation.EmailAddress+"\nPassword: "+password;
                        var emailSubject = "CONFIRM YOUR EMAIL ADDRESS";
                        //SEND MAIL TO CUSTOMER TO VERIFY EMAIL
                        MailModel mailObj = new MailModel(_configuration.GetValue<string>("AyudaEmailAddress"), _configuration.GetValue<string>("AyudaEmailName"), customer.EmailAddress, emailSubject, emailBody);
                        var response = await _mailController.SendMail(mailObj);
                        if (response.StatusCode.Equals(HttpStatusCode.Accepted))
                        {
                            return new ReturnResponse()
                            {
                                StatusCode = Utils.Success,
                                StatusMessage = "Registration Successful!!!",
                                ObjectValue = customer
                            };
                        }
                        else
                        {
                            return new ReturnResponse()
                            {
                                StatusCode = Utils.MailFailure,
                                StatusMessage = Utils.StatusMessageMailFailure
                            };
                        }
                    }
                    else
                    {
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.NotSucceeded,
                            StatusMessage = Utils.StatusMessageNotSucceeded
                        };
                    }

                }

                return new ReturnResponse()
                {
                    StatusCode = Utils.NotSucceeded,
                    StatusMessage = Utils.StatusMessageNotSucceeded
                };
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.SaveError,
                StatusMessage = Utils.StatusMessageSaveError
            };
        }

        public async Task<bool> CustomerExists(string email)
        {
            return await _userManager.Users.AnyAsync(x => x.Email.ToUpper() == email.ToUpper());
        }

        private string GetHashedEmail(string emailVal)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(emailVal));
        }
       
        public async Task<ReturnResponse> SearchCustomer(string searchParams, UserParams userParams)
        {
            var customers = from c in _context.Customer
                            .Include(c => c.CustomerType)
                         select c;

            if (!String.IsNullOrEmpty(searchParams))
            {
                customers = customers.Where(s => s.EmailAddress.Contains(searchParams) 
                || s.FullName.Contains(searchParams) 
                || s.PhoneNumber.Contains(searchParams));
            }

           
            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                StatusMessage = "Search Successful!!!",
                ObjectValue = await PagedList<Customer>.CreateAsync(customers, userParams.PageNumber, userParams.PageSize),
            };
        }
    }
}
