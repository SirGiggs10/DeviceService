using Ayuda_Help_Desk.Data;
using Ayuda_Help_Desk.Dtos.RoleFunctionality;
using Ayuda_Help_Desk.Interfaces;
using Ayuda_Help_Desk.Models;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Ayuda_Help_Desk.Helpers;
using Ayuda_Help_Desk.Dtos.General;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Http;
using Ayuda_Help_Desk.Dtos.Staff;
using System.Security.Claims;
using System.IO;
using OfficeOpenXml;
using Ayuda_Help_Desk.Dtos.Auth;
using Ayuda_Help_Desk.Dtos;
using Ayuda_Help_Desk.API.Helpers;
using CloudinaryDotNet;
using Microsoft.Extensions.Options;
using CloudinaryDotNet.Actions;
using System.Text;
using Ayuda_Help_Desk.Dtos.Ticket;

namespace Ayuda_Help_Desk.Repositories
{
    public class StaffRepository : IStaffRepository
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IMapper _mapper;
        private readonly IMailController _mailController;
        private readonly IConfiguration _configuration;
        private readonly IGlobalRepository _globalRepository;
        private readonly IAuthRepository _authRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRoleManagementRepository _roleManagementRepository;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private Cloudinary _cloudinary;

        public StaffRepository(DataContext dataContext, IOptions<CloudinarySettings> cloudinaryConfig, UserManager<User> userManager,IMapper mapper, IMailController mailController, IConfiguration configuration, IGlobalRepository globalRepository, RoleManager<Role> roleManager, IAuthRepository authRepository, IHttpContextAccessor httpContextAccessor, IRoleManagementRepository roleManagementRepository)
        {
            _dataContext = dataContext;
            _userManager = userManager;
            _mapper = mapper;
            _mailController = mailController;
            _configuration = configuration;
            _globalRepository = globalRepository;
            _roleManager = roleManager;
            _authRepository = authRepository;
            _httpContextAccessor = httpContextAccessor;
            _roleManagementRepository = roleManagementRepository;
            _cloudinaryConfig = cloudinaryConfig;

            Account acc = new Account(
               _cloudinaryConfig.Value.CloudName,
               _cloudinaryConfig.Value.ApiKey,
               _cloudinaryConfig.Value.ApiSecret
           );

            _cloudinary = new Cloudinary(acc);
        }

        public async Task<ReturnResponse> RegisterStaff(StaffForRegisterDto staffForRegisterDto)
        {
            //FINALLY REGISTER STAFF
            if (staffForRegisterDto.Staff == null || staffForRegisterDto.Staff.Any(a => string.IsNullOrWhiteSpace(a.EmailAddress)))
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = "Staff Information is Null"
                };
            }

            //GET STAFF ROLE
            var staffRole = await _roleManager.FindByNameAsync(Utils.StaffRole);
            if (staffRole == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = "Staff Role is Not Found"
                };
            }

            var staffRoleForAssignment = new RoleResponse()
            {
                Id = staffRole.Id
            };
            var allStaffRegistered = new List<User>();

            foreach (var staffForRegister in staffForRegisterDto.Staff)
            {
                if (await StaffExists(staffForRegister.EmailAddress))
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.ObjectExists,
                        StatusMessage = "One or more of the provided resources already exist(s)!"
                    };
                }

                var staff = new Staff
                {
                    EmailAddress = staffForRegister.EmailAddress,
                    FullName = staffForRegister.FullName,
                    BranchId = staffForRegister.BranchId,
                    SubUnitId = staffForRegister.SubUnitId,
                    DepartmentId = staffForRegister.DepartmentId,
                    PhoneNumber = staffForRegister.PhoneNumber,
                    ResidentialAddress = staffForRegister.ResidentialAddress,
                    DateOfBirth = staffForRegister.DateOfBirth,
                    SupervisorId = staffForRegister.SupervisorId,
                    CompanyId = staffForRegister.CompanyId
                };

                _globalRepository.Add(staff);
                var saveResult = await _globalRepository.SaveAll();
                if (saveResult.HasValue)
                {
                    if (!saveResult.Value)
                    {
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.SaveNoRowAffected,
                            StatusMessage = "Staff Information Could Not Save"
                        };
                    }

                    var user = new User
                    {
                        UserName = staffForRegister.EmailAddress,
                        Email = staffForRegister.EmailAddress,
                        UserTypeId = staff.StaffId,
                        UserType = Utils.Staff
                        //FullName = staffForRegister.FirstName + "|" + staffForRegister.MiddleName + "|" + staffForRegister.LastName
                    };
                    var password = (new Helper()).RandomPassword();

                    var result = _userManager.CreateAsync(user, password).Result;
                    if (result.Succeeded)
                    {
                        //THEN UPDATE STAFF TABLE USERID COLUMN WITH NEWLY CREATED USER ID
                        staff.UserId = user.Id;
                        var staffUpdateResult = _globalRepository.Update(staff);
                        if(!staffUpdateResult)
                        {
                            return new ReturnResponse()
                            {
                                StatusCode = Utils.NotSucceeded,
                                StatusMessage = "Error Occured while saving Staff Information"
                            };
                        }

                        var staffUpdateSaveResult = await _globalRepository.SaveAll();
                        if(!staffUpdateSaveResult.HasValue)
                        {
                            return new ReturnResponse()
                            {
                                StatusCode = Utils.SaveError,
                                StatusMessage = "Error Occured while saving Staff Information"
                            };
                        }

                        if(!staffUpdateSaveResult.Value)
                        {
                            return new ReturnResponse()
                            {
                                StatusCode = Utils.SaveNoRowAffected,
                                StatusMessage = "Error Occured while saving Staff Information"
                            };
                        }

                        //ASSIGN ROLES FROM THE REQUEST DTO TO USER
                        /*var rolesToAssign = new List<string>();
                        foreach (var t in staffForRegister.Roles)
                        {
                            var role = await _roleManager.FindByIdAsync(Convert.ToString(t.Id));
                            if (role == null)
                            {
                                await dbTransaction.RollbackAsync();
                                return new ReturnResponse()
                                {
                                    StatusCode = Utils.NotFound
                                };
                            }

                            rolesToAssign.Add(role.Name);
                        }
                        */
                        //IF NO ROLE CAME WITH THE STAFF REGISTER REQUEST (IN CASES OF REGISTRATION FROM API OR EXCEL) ASSIGN DEFAULT ROLE OF STAFF TO THAT USER
                        if(staffForRegister.Roles == null)
                        {
                            staffForRegister.Roles = new List<RoleResponse>()
                            {
                                staffRoleForAssignment
                            };
                        }

                        //var assignmentResult = await _userManager.AddToRolesAsync(user, rolesToAssign.AsEnumerable());
                        var assignmentResult = await _roleManagementRepository.AssignRolesToUser(new RoleUserAssignmentRequest()
                        {
                            Users = new List<UserToReturn>() { 
                                new UserToReturn()
                                {
                                    Id = user.Id
                                }
                            },
                            Roles = staffForRegister.Roles
                        });
                        if (assignmentResult.StatusCode == Utils.Success)
                        {
                            var userTokenVal = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                          
                            //string emailBody = emailVerificationLink;
                            var staffName = staffForRegister.FullName.Split();

                            var emailMessage1 = "Please click the button below to complete your registration and activate your account.";
                            var emailMessage2 = "Your Password is " + password;

                            var originUrls = new StringValues();
                            //CHECK LATER TO SEE IF ANY ORIGIN HEADER WILL BE SENT WITH THE REQUEST IF THE FRONTEND AND BACKEND ARE IN THE SAME DOMAIN...THAT IS IF THERE IS NO CORS
                            var originHeadersGotten = _httpContextAccessor.HttpContext.Request.Headers.TryGetValue("Origin", out originUrls);
                            var originUrl = "";
                            if (originHeadersGotten)
                            {
                                originUrl = originUrls.FirstOrDefault();
                            }


                            string emailBody = _globalRepository.GetMailBodyTemplate(staffName[0], staffName[1], originUrl+"/login", emailMessage1, emailMessage2, "activation.html");

                            var emailSubject = "CONFIRM YOUR EMAIL ADDRESS";
                            //SEND MAIL TO STAFF TO VERIFY EMAIL
                            MailModel mailObj = new MailModel(_configuration.GetValue<string>("AyudaEmailAddress"), _configuration.GetValue<string>("AyudaEmailName"), staff.EmailAddress, emailSubject, emailBody);
                            var response = await _mailController.SendMail(mailObj);
                            if (response.StatusCode.Equals(HttpStatusCode.Accepted))
                            {
                                var staffToReturn = await GetStaff(staff.StaffId);
                                if(staffToReturn.StatusCode != Utils.Success)
                                {
                                    return new ReturnResponse()
                                    {
                                        StatusCode = Utils.NotSucceeded,
                                        StatusMessage = "Error Occured while saving Staff Information"
                                    };
                                }

                                allStaffRegistered.Add((User)staffToReturn.ObjectValue);
                            }
                            else
                            {
                                return new ReturnResponse()
                                {
                                    StatusCode = Utils.MailFailure,
                                    StatusMessage = "Error Occured while sending Mail to Staff"
                                };
                            }
                        }
                        else
                        {
                            return new ReturnResponse()
                            {
                                StatusCode = Utils.NotSucceeded,
                                StatusMessage = "Error Occured while saving Staff Information"
                            };
                        }
                    }
                    else
                    {
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.NotSucceeded,
                            StatusMessage = "Error Occured while saving Staff Information"
                        };
                    }
                }
                else
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.SaveError,
                        StatusMessage = "Error Occured while saving Staff Information"
                    };
                }
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = allStaffRegistered,
                StatusMessage = "Staff Created Successfully!!!"
            };
        }

        public async Task<ReturnResponse> GetMockStaffRequests()
        {
            int departmentCount = _dataContext.Department.Count();
            int subunitCount = _dataContext.SubUnit.Count();
            int branchCount = _dataContext.Branch.Count();
            int roleCount = _dataContext.Roles.Count();
          
            var staffRequests = new List<StaffMockDataRequest>();

            for(int i = 0; i < 30; i++)
            {
                Random r = new Random();
                int departmentOffset = r.Next(0, departmentCount);
                int subUnitOffset = r.Next(0, subunitCount);
                int branchOffset = r.Next(0, branchCount);
                int roleOffset = r.Next(0, roleCount);


                var staffRequest = new StaffMockDataRequest();
                int length = 7;

                // creating a StringBuilder object()
                StringBuilder str_build = new StringBuilder();
                Random random = new Random();

                char letter;

                for (int k = 0; k < length; k++)
                {
                    double flt = random.NextDouble();
                    int shift = Convert.ToInt32(Math.Floor(25 * flt));
                    letter = Convert.ToChar(shift + 65);
                    str_build.Append(letter);
                }
                staffRequest.DepartmentId = (await _dataContext.Department.Skip(departmentOffset).FirstOrDefaultAsync()).DepartmentId;
                staffRequest.SubUnitId = (await _dataContext.SubUnit.Skip(subUnitOffset).FirstOrDefaultAsync()).SubUnitId;
                staffRequest.BranchId = (await _dataContext.Branch.Skip(branchOffset).FirstOrDefaultAsync()).BranchId;
                staffRequest.FullName = "Mike Jerry";
                staffRequest.CompanyId = "";
                staffRequest.SupervisorId = 0;
                staffRequest.PhoneNumber = "07065436827";
                staffRequest.EmailAddress = str_build.ToString()+"@gmail.com";
                staffRequest.ResidentialAddress = "";
                staffRequest.DateOfBirth = DateTimeOffset.Now;
                var staffRole = await _dataContext.Roles.Include(s => s.SupportLevel).Skip(roleOffset).FirstOrDefaultAsync();
                var staffRoleResponse = new MockRoleRequest();
                staffRoleResponse.Id = staffRole.Id;
                staffRoleResponse.Name = staffRole.Name;
                var staffRoles = new List<MockRoleRequest>();
                staffRoles.Add(staffRoleResponse);
                staffRequest.Roles = staffRoles;
                staffRequests.Add(staffRequest);
        }
      
            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = staffRequests
            };
        }

        private async Task<bool> StaffExists(string username)
        {
            return await _userManager.Users.AnyAsync(x => x.Email.ToUpper() == username.ToUpper());
        }

        private string GetHashedEmail(string emailVal)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(emailVal));
        }

        public async Task<ReturnResponse> SetStaffPasswordAndProfilePicture(StaffPasswordRequest staffPasswordRequest)
        {
            if (staffPasswordRequest.EmailAddress == null || string.IsNullOrWhiteSpace(staffPasswordRequest.OldPassword) || string.IsNullOrWhiteSpace(staffPasswordRequest.NewPassword))
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.BadRequest,
                    StatusMessage = Utils.StatusMessageBadRequest
                };
            }

            if (staffPasswordRequest.OldPassword == staffPasswordRequest.NewPassword)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.PreviousPasswordStorageError,
                    StatusMessage = Utils.StatusMessagePreviousPasswordStorageError
                };
            }
            var userDetail = await _userManager.Users
                .Where(a => a.NormalizedEmail == staffPasswordRequest.EmailAddress.ToUpper())
                .Include(b => b.Staff).FirstOrDefaultAsync();

            //HASH THE NEW PASSWORD
            var newPasswordHash = _userManager.PasswordHasher.HashPassword(userDetail, staffPasswordRequest.NewPassword);


            //CHECK USERPREVIOUSPASSWORD TABLE TO MAKE SURE NEWPASSWORD HAS NOT BEEN USED BEFORE
            var allPreviousPasswordsForUser = await _dataContext.UserPreviousPassword
                .Where(a => a.UserId == userDetail.Id).Select(b => b.HashedPreviousPassword)
                .ToListAsync();
            if (allPreviousPasswordsForUser.Any(a => a == newPasswordHash))
            {
                //NEW PASSWORD HAS BEEN USED BEFORE
                return new ReturnResponse()
                {
                    StatusCode = Utils.NewPasswordError,
                    StatusMessage = Utils.StatusMessageNewPasswordError
                };
            }


             if (userDetail == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

            if(userDetail.Staff == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(userDetail, staffPasswordRequest.OldPassword, staffPasswordRequest.NewPassword);

            if (staffPasswordRequest.StaffProfilePicture != null)
            {
                var file = staffPasswordRequest.StaffProfilePicture;

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
                var staffDetails = await _dataContext.Staff.Where(s => s.EmailAddress == staffPasswordRequest.EmailAddress).FirstOrDefaultAsync();
                staffDetails.StaffProfilePictureUrl = uploadResult.Uri.ToString();
                staffDetails.StaffProfilePicturePublicId = uploadResult.PublicId;
                _dataContext.Entry(staffDetails).State = EntityState.Modified;
        }
            var result = await _userManager.UpdateAsync(userDetail);
            if (result.Succeeded && changePasswordResult.Succeeded)
            {
                try
                {
                    await _dataContext.SaveChangesAsync();

                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
                bool emailTokenConfirmed;

                var userTokenVal = await _userManager.GenerateEmailConfirmationTokenAsync(userDetail);
                IdentityResult identityResult = await _userManager.ConfirmEmailAsync(userDetail, userTokenVal);
                if (identityResult.Succeeded)
                {
                    emailTokenConfirmed = true;
                }
                else
                {
                    emailTokenConfirmed = false;
                }

                //SEND MAIL TO STAFF
                // string emailBody = "MESSAGE BODY";

                var staffName = userDetail.Staff.FullName.Split();

                var emailMessage1 = "You have Successfully Set your Password and Profile Picture";
                //Please click the button below to complete your registration and activate you account.
                var emailMessage2 = "";

                string emailBody = _globalRepository.GetMailBodyTemplate(staffName[0], staffName[1], "", emailMessage1, emailMessage2, "index.html");


                var emailSubject = "PASSWORD AND PROFILE PICTURE SET SUCCESSFUL";
                MailModel mailObj = new MailModel(_configuration.GetValue<string>("AyudaEmailAddress"), _configuration.GetValue<string>("AyudaEmailName"), userDetail.Staff.EmailAddress, emailSubject, emailBody);
                var response = await _mailController.SendMail(mailObj);
                if (response.StatusCode.Equals(HttpStatusCode.Accepted))
                {
                    var loginResult = await _authRepository.LoginUser(new UserForLoginDto()
                    {
                        EmailAddress = staffPasswordRequest.EmailAddress,
                        Password = staffPasswordRequest.NewPassword
                    }, _configuration.GetValue<string>("AppSettings:Secret"));

                    return loginResult;
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
            else {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotSucceeded,
                    StatusMessage = Utils.StatusMessageNotSucceeded
                };
            }
         }

        public async Task<ReturnResponse> SetProfilePicture(StaffPictureRequest staffPasswordRequest)
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
            if (userTypeVal == Utils.Staff)
            {
                userStaffDetails = await _userManager.Users.Where(a => (a.UserTypeId == userTypeIdVal) && (a.UserType == userTypeVal)).Include(b => b.Staff).FirstOrDefaultAsync();
                try
                {
                    if ((userStaffDetails == null) || (userStaffDetails.Staff == null))
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
                        StatusCode = Utils.NotFound
                    };
                }
            }

            if (staffPasswordRequest.StaffProfilePicture != null)
            {
                var file = staffPasswordRequest.StaffProfilePicture;

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
                userStaffDetails.Staff.StaffProfilePictureUrl = uploadResult.Uri.ToString();
                userStaffDetails.Staff.StaffProfilePicturePublicId = uploadResult.PublicId;
                _dataContext.Entry(userStaffDetails.Staff).State = EntityState.Modified;
            }
            var result = await _userManager.UpdateAsync(userStaffDetails);
            if (result.Succeeded)
            {
                try
                {
                    await _dataContext.SaveChangesAsync();

                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
                //SEND MAIL TO STAFF
                //string emailBody = "MESSAGE BODY";

                var staffName = userStaffDetails.Staff.FullName.Split();

                var emailMessage1 = "You have Successfully Set your Profile Picture";
                //Please click the button below to complete your registration and activate you account.
                var emailMessage2 = "";

                string emailBody = _globalRepository.GetMailBodyTemplate(staffName[0], staffName[1], "", emailMessage1, emailMessage2, "index.html");

                var emailSubject = "PROFILE PICTURE UPDATED SUCCESSFULLY";

                userStaffDetails = await _userManager.Users.Where(a => (a.UserTypeId == userTypeIdVal) && (a.UserType == userTypeVal)).Include(b => b.Staff).FirstOrDefaultAsync();
                object UserProfileInformation = _mapper.Map<StaffResponse>(userStaffDetails.Staff);
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

        public async Task<ReturnResponse> GetStaff(UserParams userParams)
        {
            var allStaff = _userManager.Users.Where(a => (a.UserType == Utils.Staff) && (!a.Deleted)).Include(c => c.UserRoles).ThenInclude(d => d.Role).ThenInclude(j => j.SupportLevel).Include(b => b.Staff).ThenInclude(e => e.Department).Include(b => b.Staff).ThenInclude(f => f.SubUnit).Include(b => b.Staff).ThenInclude(g => g.Branch);
            if (allStaff == null || !(await allStaff.AnyAsync()))
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

            var allStaffToReturn = await PagedList<User>.CreateAsync(allStaff, userParams.PageNumber, userParams.PageSize);
            var listOfStaffToReturn = allStaffToReturn.ToList();
            listOfStaffToReturn.ForEach(SetStaffSupervisor);
            _httpContextAccessor.HttpContext.Response.AddPagination(allStaffToReturn.CurrentPage, allStaffToReturn.PageSize, allStaffToReturn.TotalCount, allStaffToReturn.TotalPages);

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = listOfStaffToReturn,
                StatusMessage = Utils.StatusMessageSuccess
            };
        }

        public async Task<ReturnResponse> GetAllStaffUnpaginated()
        {
            var allStaff = _userManager.Users.Where(a => (a.UserType == Utils.Staff) && (!a.Deleted))
                .Include(c => c.UserRoles)
                .ThenInclude(d => d.Role)
                .ThenInclude(j => j.SupportLevel)
                .Include(b => b.Staff)
                .ThenInclude(e => e.Department)
                .Include(b => b.Staff)
                .ThenInclude(f => f.SubUnit)
                .Include(b => b.Staff)
                .ThenInclude(g => g.Branch);
            if (allStaff == null || !(await allStaff.AnyAsync()))
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

            //var allStaffToReturn = await PagedList<User>.CreateAsync(allStaff, userParams.PageNumber, userParams.PageSize);
            var listOfStaffToReturn = allStaff.ToList();
            listOfStaffToReturn.ForEach(SetStaffSupervisor);
            //_httpContextAccessor.HttpContext.Response.AddPagination(allStaffToReturn.CurrentPage, allStaffToReturn.PageSize, allStaffToReturn.TotalCount, allStaffToReturn.TotalPages);

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = listOfStaffToReturn,
                StatusMessage = Utils.StatusMessageSuccess
            };
        }

        public async Task<ReturnResponse> GetStaff(int id)
        {
            var allStaff = await _userManager.Users
                .Where(a => (a.UserTypeId == id) && (a.UserType == Utils.Staff) && (!a.Deleted))
                .Include(c => c.UserRoles).ThenInclude(d => d.Role)
                .ThenInclude(j => j.SupportLevel).Include(b => b.Staff)
                .ThenInclude(e => e.Department).Include(b => b.Staff)
                .ThenInclude(f => f.SubUnit).Include(b => b.Staff)
                .ThenInclude(g => g.Branch).ToListAsync();

            if (allStaff == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

            allStaff.ForEach(SetStaffSupervisor);
            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = allStaff.FirstOrDefault(),
                StatusMessage = Utils.StatusMessageSuccess
            };
        }
        
        public async Task<ReturnResponse> UpdateStaff(StaffUpdateRequest staff)
        {
            if(staff == null)
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

            int staffId = Convert.ToInt32(userStaffId);
          

            //UPDATE STAFF INFORMATION
            var staffToUpdate = await _globalRepository.Get<Staff>(staffId);
            if(staffToUpdate == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

            var updatedStaff = _mapper.Map(staff, staffToUpdate);
            _globalRepository.Update(updatedStaff);
            var saveResult = await _globalRepository.SaveAll();
            if(saveResult.HasValue)
            {
                if(!saveResult.Value)
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
                    ObjectValue = updatedStaff,
                    StatusMessage = Utils.StatusMessageSuccess
                };
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.SaveError,
                StatusMessage = Utils.StatusMessageSaveError
            };
        }

        public async Task<ReturnResponse> AdminUpdateStaff(StaffUpdateRequest staff, int id)
        {
            if (staff == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            
            if (id != staff.StaffId)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.BadRequest,
                    StatusMessage = Utils.StatusMessageBadRequest
                };
            }

            //UPDATE STAFF INFORMATION
            var staffToUpdate = await _globalRepository.Get<Staff>(id);
            if (staffToUpdate == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

            var updatedStaff = _mapper.Map(staff, staffToUpdate);
            _globalRepository.Update(updatedStaff);
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
                    ObjectValue = updatedStaff,
                    StatusMessage = Utils.StatusMessageSuccess
                };
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.SaveError,
                StatusMessage = Utils.StatusMessageSaveError
            };
        }

        public async Task<ReturnResponse> DeleteStaff(List<StaffResponse> staff)
        {
            if(staff == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            var deletedStaff = new List<Staff>();
            foreach(var t in staff)
            {
                /*var staffToDelete = await _globalRepository.Get<Staff>(t.StaffId);
                if(staffToDelete == null)
                {
                    await dbTransaction.RollbackAsync();
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotFound
                    };
                }*/

                var userToDelete = await _userManager.Users.Where(a => (a.UserTypeId == t.StaffId) && (a.UserType == Utils.Staff) && (!a.Deleted)).Include(b => b.Staff).FirstOrDefaultAsync();
                if((userToDelete == null) || (userToDelete.Staff == null))
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = Utils.StatusMessageNotFound
                    };
                }

                //DELETE USER..UPDATE THE DELETED COLUMN OF THE USER IN THE DATABASE
                //var deleteResult = await _userManager.DeleteAsync(userToDelete);
                userToDelete.Deleted = true;
                userToDelete.Staff.DeletedAt = DateTimeOffset.Now;
                var deleteResult = await _userManager.UpdateAsync(userToDelete);
                /*
                _globalRepository.Update(userToDelete.Staff);
                var updateSaveResult = await _globalRepository.SaveAll();
                if(updateSaveResult.HasValue)
                {
                    if(!updateSaveResult.Value)
                    {
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.SaveNoRowAffected
                        };
                    }
                }
                else
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.SaveError
                    };
                }
                */
                if(!deleteResult.Succeeded)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotSucceeded,
                        StatusMessage = Utils.StatusMessageNotSucceeded
                    };
                }

                //DELETE USER'S ROLES
                var usersCurrentRoles = await _userManager.GetRolesAsync(userToDelete);
                var rolesDeleteResult = await _userManager.RemoveFromRolesAsync(userToDelete, usersCurrentRoles.AsEnumerable());
                if(!rolesDeleteResult.Succeeded)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotSucceeded,
                        StatusMessage = Utils.StatusMessageNotSucceeded
                    };
                }

                deletedStaff.Add(userToDelete.Staff);
            }
            /*
            _globalRepository.Delete(deletedStaff);
            var saveResult = await _globalRepository.SaveAll();
            if(saveResult.HasValue)
            {
                if(!saveResult.Value)
                {
                    await dbTransaction.RollbackAsync();
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.SaveNoRowAffected
                    };
                }

                await dbTransaction.CommitAsync();
                return new ReturnResponse()
                {
                    StatusCode = Utils.Success,
                    ObjectValue = deletedStaff
                };
            }

            await dbTransaction.RollbackAsync();
            return new ReturnResponse()
            {
                StatusCode = Utils.SaveError
            };
            */
            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = deletedStaff,
                StatusMessage = Utils.StatusMessageSuccess
            };
        }

        private void SetStaffSupervisor(User staff)
        {
            var staffSupervisor = _userManager.Users.Where(a => (a.UserTypeId == staff.Staff.SupervisorId) && (a.UserType == Utils.Staff) && (!a.Deleted)).Include(b => b.Staff).FirstOrDefault();
            staff.Staff.Supervisor = staffSupervisor;
        }

        public async Task<ReturnResponse> GetSupportLevelStaffCount()
        {
            var userType = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == Utils.ClaimType_UserType);
            if (userType == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

            var userTypeVal = Convert.ToInt32(userType.Value);
            if(userTypeVal != Utils.Staff)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.BadRequest,
                    StatusMessage = Utils.StatusMessageBadRequest
                };
            }

            var allSupportLevelsInSystem = await _dataContext.SupportLevel.Select(a => new { SupportLevelId = a.SupportLevelId, SupportLevelName = a.SupportLevelName }).ToListAsync();
            var supportLevelStaffCount = _userManager.Users.Where(f => f.UserType == Utils.Staff).Include(a => a.UserRoles).ThenInclude(b => b.Role).AsEnumerable().Select(c => c.UserRoles[0].Role.SupportLevelId).GroupBy(d => d).Select(e => new { SupportLevelId = e.Key, StaffCountInSupportLevel = e.Count() }).ToList();
            var supportLevelStaffObjects = new List<SupportLevelStaffCountResponse>();
            foreach (var supportLevelVal in allSupportLevelsInSystem)
            {
                var supportLevelCountObject = new SupportLevelStaffCountResponse() { SupportLevelId = supportLevelVal.SupportLevelId, SupportLevelName = supportLevelVal.SupportLevelName };
                var supportLevelCount = supportLevelStaffCount.Where(a => a.SupportLevelId == supportLevelVal.SupportLevelId).Select(b => b.StaffCountInSupportLevel).FirstOrDefault();
                supportLevelCountObject.StaffCountInSupportLevel = supportLevelCount;
                supportLevelStaffObjects.Add(supportLevelCountObject);
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = supportLevelStaffObjects,
                StatusMessage = Utils.StatusMessageSuccess
            };
        }

        private async Task<bool> StoreUserPreviousPassword(User user)
        {
            var userPreviousPassword = new UserPreviousPassword()
            {
                UserId = user.Id,
                HashedPreviousPassword = user.PasswordHash,
                CreatedAt = DateTimeOffset.Now
            };
            _globalRepository.Add(userPreviousPassword);
            var result = await _globalRepository.SaveAll();

            if (result != null)
            {
                if (!result.Value)
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        public async Task<ReturnResponse> SearchStaff(string searchParams, UserParams userParams)
        {
           // var staffs = from c in _dataContext.Customer
            //                select c;

            var staffs = from s in _userManager.Users
               .Where(a => (a.UserType == Utils.Staff) && (!a.Deleted))
               select s;

            if (!String.IsNullOrEmpty(searchParams))
            {
                staffs = staffs.Where(s => s.Email.Contains(searchParams) 
                || s.Staff.FullName.Contains(searchParams) 
                || s.PhoneNumber.Contains(searchParams))
                    .Include(c => c.UserRoles).ThenInclude(d => d.Role)
               .ThenInclude(j => j.SupportLevel).Include(b => b.Staff)
               .ThenInclude(e => e.Department).Include(b => b.Staff)
               .ThenInclude(f => f.SubUnit).Include(b => b.Staff)
               .ThenInclude(g => g.Branch);
            }
            var allStaffToReturn = await PagedList<User>.CreateAsync(staffs, userParams.PageNumber, userParams.PageSize);
            var listOfStaffToReturn = allStaffToReturn.ToList();
            listOfStaffToReturn.ForEach(SetStaffSupervisor);
            _httpContextAccessor.HttpContext.Response.AddPagination(allStaffToReturn.CurrentPage, allStaffToReturn.PageSize, allStaffToReturn.TotalCount, allStaffToReturn.TotalPages);

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                StatusMessage = "Search Successful!!!",
                ObjectValue = listOfStaffToReturn
            };
        }
    }
}
