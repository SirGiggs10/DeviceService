using AutoMapper;
using Ayuda_Help_Desk.Data;
using Ayuda_Help_Desk.Dtos;
using Ayuda_Help_Desk.Dtos.Customer;
using Ayuda_Help_Desk.Dtos.General;
using Ayuda_Help_Desk.Helpers;
using Ayuda_Help_Desk.Interfaces;
using Ayuda_Help_Desk.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Threading.Tasks;
using System.Security.Claims;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Ayuda_Help_Desk.API.Helpers;
using Microsoft.Extensions.Options;
using OtpNet;
using Microsoft.Extensions.Caching.Memory;

namespace Ayuda_Help_Desk.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IGlobalRepository _globalRepository;
        private IMemoryCache _cache;
        private readonly IMailController _mailController;
        private readonly IConfiguration _configuration;
        public readonly IAuthRepository _authRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private Cloudinary _cloudinary;

        public CustomerRepository(DataContext context, IMemoryCache cache, IGlobalRepository globalRepository, IOptions<CloudinarySettings> cloudinaryConfig, IMapper mapper, IHttpContextAccessor httpContextAccessor, UserManager<User> userManager, IMailController mailController, IConfiguration configuration, IAuthRepository authRepository)
        {
            _context = context;
            _cache = cache;
            _userManager = userManager; 
            _globalRepository = globalRepository;
            _mailController = mailController;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _authRepository = authRepository;
            _cloudinaryConfig = cloudinaryConfig;

            Account acc = new Account(
               _cloudinaryConfig.Value.CloudName,
               _cloudinaryConfig.Value.ApiKey,
               _cloudinaryConfig.Value.ApiSecret
           );

            _cloudinary = new Cloudinary(acc);
        }

        public async Task<ReturnResponse> CreateCustomer(CustomerForRegisterDto customerForRegister)
        {
            if (customerForRegister == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

           if (await CustomerExists(customerForRegister.EmailAddress))
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectExists,
                    StatusMessage = "A Customer with this Email Address already Exists!!!"
                };
            }

            var customer = new Customer
            {
                EmailAddress = customerForRegister.EmailAddress,
                FullName = customerForRegister.FullName,
                PhoneNumber = customerForRegister.PhoneNumber,
                CustomerTypeId = customerForRegister.CustomerType
            };

            _globalRepository.Add(customer);
            var saveVal = await _globalRepository.SaveAll();            
            
            if (saveVal.HasValue)
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
                    UserName = customerForRegister.EmailAddress,
                    Email = customerForRegister.EmailAddress,
                    UserTypeId = customer.CustomerId,
                    UserType = Utils.Customer
                    //FullName = customerForRegister.FullName
                };

                var result = await _userManager.CreateAsync(user, customerForRegister.Password);
                if (result.Succeeded)
                {
                    //ASSIGN (INDIVIDUAL OR CORPORATE) ROLE TO USER (CUSTOMER)
                    var customerTypeRole = "";
                    if(customerForRegister.CustomerType == Utils.Individual)
                    {
                        customerTypeRole = Utils.CustomerIndividualRole;
                    }
                    else if(customerForRegister.CustomerType == Utils.Corporate)
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

                       //if (customerForRegister.IsMobile != 1) {
                       //     //SEND MAIL TO CUSTOMER TO CONFIRM EMAIL
                       //     var userTokenVal = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                       //     string hashedEmail = GetHashedEmail(user.Email);
                       //     /*var shortToken = userTokenVal.Substring(0, 7);
                       //     user.ShortToken = shortToken;
                       //     user.LongToken = userTokenVal;
                       //     var saveThis = _globalRepository.SaveAll();*/
                       //     string fullToken = userTokenVal + "#" + hashedEmail;
                       //     var emailVerificationLink = _authRepository.GetUserEmailVerificationLink(fullToken);
                       //     if (emailVerificationLink == null)
                       //     {
                       //         return new ReturnResponse()
                       //         {
                       //             StatusCode = Utils.ObjectNull,
                       //             StatusMessage = Utils.StatusMessageObjectNull
                       //         };
                       //     }
                       // // string emailBody = emailVerificationLink;
                       // var customerName = customerForRegister.FullName.Split();

                       // var emailMessage1 = "";
                       // var emailMessage2 = "Please click the button below to complete your registration and activate your account.";

                       // string emailBody = _globalRepository.GetMailBodyTemplate(customerName[0], customerName[1], emailVerificationLink, emailMessage1, emailMessage2, "activation.html");

                       // var emailSubject = "CONFIRM YOUR EMAIL ADDRESS";
                       // //SEND MAIL TO CUSTOMER TO VERIFY EMAIL
                       // MailModel mailObj = new MailModel(_configuration.GetValue<string>("AyudaEmailAddress"), _configuration.GetValue<string>("AyudaEmailName"), customer.EmailAddress, emailSubject, emailBody);
                       // var response = await _mailController.SendMail(mailObj);
                       // if (response.StatusCode.Equals(HttpStatusCode.Accepted))
                       // {
                       //     return new ReturnResponse()
                       //     {
                       //         StatusCode = Utils.Success,
                       //         StatusMessage = "Registration Successful!!!",
                       //         ObjectValue = customer
                       //     };
                       // }
                       // else
                       // {
                       //     return new ReturnResponse()
                       //     {
                       //         StatusCode = Utils.MailFailure,
                       //         StatusMessage = Utils.StatusMessageMailFailure
                       //     };
                       // }
                       // }
                       // else
                       // {
                            var key = _configuration.GetValue<string>("AppSettings:Secret");
                        // Creating byte array of string length 
                        byte[] secretKey = new byte[key.Length];

                        // converting each character into byte 
                        // and store it
                        for (int i = 0; i < key.Length; i++)
                        {
                            secretKey[i] = Convert.ToByte(key[i]);
                        }

                           var totp = new Totp(secretKey);
                        // string emailBody = emailVerificationLink;
                        var customerName = customerForRegister.FullName.Split();

                        var emailMessage1 = "Please use the code below to complete your registration and activate your account.";
                       
                         var otpCode = totp.ComputeTotp();
                            var emailMessage2 = otpCode;

                            string emailBody = _globalRepository.GetMailBodyTemplate(customerName[0], customerName[1], "", emailMessage1, emailMessage2, "activationMobile.html");

                        var emailSubject = "CONFIRM YOUR EMAIL ADDRESS";
                        //SEND MAIL TO CUSTOMER TO VERIFY EMAIL
                        MailModel mailObj = new MailModel(_configuration.GetValue<string>("AyudaEmailAddress"), _configuration.GetValue<string>("AyudaEmailName"), customer.EmailAddress, emailSubject, emailBody);
                        var response = await _mailController.SendMail(mailObj);
                        if (response.StatusCode.Equals(HttpStatusCode.Accepted))
                        {
                            var cacheEntry = otpCode;
                                var cacheExpiryOptions = new MemoryCacheEntryOptions
                                {
                                    AbsoluteExpiration = DateTime.Now.AddMinutes(30),
                                    Priority = CacheItemPriority.High,
                                    SlidingExpiration = TimeSpan.FromMinutes(2),
                                    Size = 1024,
                                };

                                string code = string.Empty;
                                _cache.TryGetValue(customer.EmailAddress, out code);

                                if (code is null)
                                {
                                    _cache.Set(customer.EmailAddress, otpCode, cacheExpiryOptions);
                                }
                                else
                                {
                                    return new ReturnResponse()
                                    {
                                        StatusCode = Utils.BadRequest,
                                        StatusMessage = "OTP has already been sent!!!",
                                    };
                                }

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
                        //}



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
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(emailVal));//.Replace("=", "ngiSlauqe");
        }

        public async Task<ReturnResponse> UpdateCustomer(CustomerUpdateRequest customerUpdateRequest)
        { 
            if (customerUpdateRequest == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            var userStaffId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;

            if (userStaffId == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            int customerId = Convert.ToInt32(userStaffId);
         
            //UPDATE STAFF INFORMATION
            var customerToUpdate = await _globalRepository.Get<Customer>(customerId);
            if (customerToUpdate == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

            var updatedCustomer = _mapper.Map(customerUpdateRequest, customerToUpdate);
            _globalRepository.Update(updatedCustomer);
            var saveResult = await _globalRepository.SaveAll();
            if (saveResult.HasValue)
            {
                if (!saveResult.Value)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.SaveNoRowAffected,
                        StatusMessage = Utils.StatusMessageSaveNoRowAffected
                    };
                }

                return new ReturnResponse()
                {
                    StatusCode = Utils.Success,
                    ObjectValue = updatedCustomer,
                    StatusMessage = Utils.StatusMessageSuccess
                };
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.SaveError,
                StatusMessage = Utils.StatusMessageSaveError
            };
        }

        public async Task<ReturnResponse> SetProfilePicture(CustomerPictureRequest customerPictureRequest)
        {
            var userType = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == Utils.ClaimType_UserType);
            var userTypeId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.NameIdentifier).Value;

            if (userTypeId == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }
            var userTypeVal = Convert.ToInt32(userType.Value);
            var userTypeIdVal = int.Parse(userTypeId);
            var userStaffDetails = new User { };
            if (userTypeVal == Utils.Customer)
            {
                userStaffDetails = await _userManager.Users.Where(a => (a.UserTypeId == userTypeIdVal) && (a.UserType == userTypeVal)).Include(b => b.Customer).FirstOrDefaultAsync();
                try
                {
                    if ((userStaffDetails == null) || (userStaffDetails.Customer == null))
                    {
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.NotFound,
                            StatusMessage = Utils.StatusMessageNotFound
                        };
                    }
                }
                catch (Exception)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = Utils.StatusMessageNotFound
                    };
                }
            }

            if (customerPictureRequest.CustomerProfilePicture != null)
            {
                var file = customerPictureRequest.CustomerProfilePicture;

                var uploadResult = new ImageUploadResult();

                if (file.Length > 0)
                {
                    using (var stream = file.OpenReadStream())
                    {
                        var uploadParams = new ImageUploadParams()
                        {
                            File = new FileDescription(file.Name, stream),
                        };

                        uploadResult = _cloudinary.Upload(uploadParams);
                    }
                }
                userStaffDetails.Customer.CustomerProfilePictureUrl = uploadResult.Uri.ToString();
                userStaffDetails.Customer.CustomerProfilePicturePublicId = uploadResult.PublicId;
                _context.Entry(userStaffDetails.Customer).State = EntityState.Modified;
            }
            var result = await _userManager.UpdateAsync(userStaffDetails);
            if (result.Succeeded)
            {
                try
                {
                    await _context.SaveChangesAsync();

                } 
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
                //SEND MAIL TO STAFF
                var emailSubject = "PROFILE PICTURE UPDATED SUCCESSFULLY";
                var userName = userStaffDetails.Customer.FullName.Split();

                string link = "";
                var emailMessage1 = "";
                var emailMessage2 = "Your Profile Picture was Set Successfully.";

                userStaffDetails = await _userManager.Users.Where(a => (a.UserTypeId == userTypeIdVal) && (a.UserType == userTypeVal)).Include(b => b.Customer).FirstOrDefaultAsync();

                string emailBody = _globalRepository.GetMailBodyTemplate(userName[0], userName[1], link, emailMessage1, emailMessage2, "index.html");
                object UserProfileInformation = _mapper.Map<CustomerResponse>(userStaffDetails.Customer);
                return new ReturnResponse()
                    {
                        StatusCode = Utils.Success,
                        StatusMessage = "Profile Picture Updated Successfully",
                        ObjectValue = UserProfileInformation
                };
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

        public static class CacheKeys
        {
            public static string Entry { get { return "_Entry"; } }
            public static string CallbackEntry { get { return "_Callback"; } }
            public static string CallbackMessage { get { return "_CallbackMessage"; } }
            public static string Parent { get { return "_Parent"; } }
            public static string Child { get { return "_Child"; } }
            public static string DependentMessage { get { return "_DependentMessage"; } }
            public static string DependentCTS { get { return "_DependentCTS"; } }
            public static string Ticks { get { return "_Ticks"; } }
            public static string CancelMsg { get { return "_CancelMsg"; } }
            public static string CancelTokenSource { get { return "_CancelTokenSource"; } }
        }
    }
}
