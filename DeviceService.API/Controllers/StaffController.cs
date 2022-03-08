using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ayuda_Help_Desk.Data;
using Ayuda_Help_Desk.Models;
using Ayuda_Help_Desk.Dtos;
using Ayuda_Help_Desk.Interfaces;
using Ayuda_Help_Desk.Helpers;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Ayuda_Help_Desk.Dtos.Staff;
using Ayuda_Help_Desk.Dtos.Auth;
using Ayuda_Help_Desk.Dtos.General;
using Microsoft.Extensions.Primitives;
using Ayuda_Help_Desk.Dtos.RoleFunctionality;
using Ayuda_Help_Desk.Dtos.Customer;
using Ayuda_Help_Desk.Dtos.UserManagement;
using Ayuda_Help_Desk.Dtos.AuditReport;

namespace Ayuda_Help_Desk.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class StaffController : ControllerBase
    {
        public readonly IStaffRepository _staffRepository;
        public readonly IMapper _mapper;
        private readonly DataContext _dataContext;
        private readonly IAuditReportRepository _auditReportRepository;

        public StaffController(IStaffRepository staffRepository, IMapper mapper, DataContext dataContext, IAuditReportRepository auditReportRepository)
        {
            _staffRepository = staffRepository;
            _mapper = mapper;
            _dataContext = dataContext;
            _auditReportRepository = auditReportRepository;
        }

        // GET: api/Staff
        [RequiredFunctionalityName("GetStaffs")]
        [HttpGet]
        public async Task<ActionResult<ReturnResponse>> GetStaff([FromQuery] UserParams userParams)
        {
            var result = await _staffRepository.GetStaff(userParams);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<StaffWithRoleToReturn>>((List<User>)result.ObjectValue);
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "GetStaffs",
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

        // GET: api/Staff/Mock
        [RequiredFunctionalityName("GetMockStaffRequests")]
        [HttpGet("Mock")]
        public async Task<ActionResult<ReturnResponse>> GetMockStaffRequests()
        {
            var result = await _staffRepository.GetMockStaffRequests();

            if (result.StatusCode == Utils.Success)
            {
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "GetMockStaffRequests",
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

        // GET: api/Staff/unpaginated
        [RequiredFunctionalityName("GetStaffUnpaginated")]
        [HttpGet("unpaginated")]
        public async Task<ActionResult<ReturnResponse>> GetStaffUnpaginated()
        {
            var result = await _staffRepository.GetAllStaffUnpaginated();

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<StaffWithRoleToReturn>>((List<User>)result.ObjectValue);
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "GetStaffUnpaginated",
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

        // GET: api/Staffs/5
        [RequiredFunctionalityName("GetStaff")]
        [HttpGet("{id}")]
        public async Task<ActionResult<ReturnResponse>> GetStaff(int id)
        {
            var result = await _staffRepository.GetStaff(id);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<StaffWithRoleToReturn>((User) result.ObjectValue);
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "GetStaff",
                    AuditReportActivityResourceId = new List<int>() { id }
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

        // GET: api/Staffs/5
        [AllowAnonymous]
        [HttpGet("NotLoggedIn/{id}")]
        public async Task<ActionResult<ReturnResponse>> GetStaffForNotLoggedInUser(int id)
        {
            var result = await _staffRepository.GetStaff(id);

            if (result.StatusCode == Utils.Success)
            {
                var userVal = (User)result.ObjectValue;
                result.ObjectValue = _mapper.Map<StaffWithRoleToReturn>(userVal);
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "GetStaffForNotLoggedInUser",
                    AuditReportActivityResourceId = new List<int>() { id },
                    UserId = userVal.Staff.UserId

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

        // PUT: api/Staffs/
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [RequiredFunctionalityName("PutStaff")]
        [HttpPut]
        public async Task<ActionResult<ReturnResponse>> PutStaff([FromBody] StaffUpdateRequest staff)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _staffRepository.UpdateStaff(staff);

            if (result.StatusCode == Utils.Success)
            {
                var staffDetail = _mapper.Map<StaffResponse>((Staff)result.ObjectValue);
                result.ObjectValue = staffDetail;
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "PutStaff",
                    AuditReportActivityResourceId = new List<int>() { staffDetail.StaffId }
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

        // PUT: api/Staffs/Picture
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [RequiredFunctionalityName("PutStaffPicture")]
        [HttpPut("Picture")]
        public async Task<ActionResult<ReturnResponse>> PutStaffPicture([FromForm] StaffPictureRequest staffPictureRequest)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _staffRepository.SetProfilePicture(staffPictureRequest);

            if (result.StatusCode == Utils.Success)
            {
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "PutStaffPicture",
                    AuditReportActivityResourceId = new List<int>() { }
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

        // PUT: api/Staff/admin/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [RequiredFunctionalityName("AdminPutStaff")]
        [HttpPut("Admin/{id}")]
        public async Task<ActionResult<ReturnResponse>> AdminPutStaff([FromBody] StaffUpdateRequest staff, int id)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _staffRepository.AdminUpdateStaff(staff, id);

            if (result.StatusCode == Utils.Success)
            {
                var staffDetail = _mapper.Map<StaffResponse>((Staff)result.ObjectValue);
                result.ObjectValue = staffDetail;
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "AdminPutStaff",
                    AuditReportActivityResourceId = new List<int>() { staffDetail.StaffId }
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

        // POST: api/Staff/Register
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [RequiredFunctionalityName("PostRegisterStaff")]
        [HttpPost("Register")]
        public async Task<ActionResult<ReturnResponse>> PostRegisterStaff(StaffForRegisterDto staffForRegisterDto)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _staffRepository.RegisterStaff(staffForRegisterDto);

            if (result.StatusCode == Utils.Success)
            {
                /*var userDetails = (UserDetails)result.ObjectValue;
                var userInfoToReturn = _mapper.Map<UserLoginResponse>(userDetails);
                userInfoToReturn.UserProfileInformation = _mapper.Map<List<StaffResponse>>((List<Staff>)userDetails.userProfile);
                result.ObjectValue = userInfoToReturn;*/
                var listOfStaff = _mapper.Map<List<StaffWithRoleToReturn>>((List<User>)result.ObjectValue);
                result.ObjectValue = listOfStaff;
                //AUDIT THIS ACTIVITY FOR THE USER
                //var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                //{
                //    AuditReportActivityFunctionalityName = "PostRegisterStaff",
                //    AuditReportActivityResourceId = listOfStaff.Select(a => a.Staff.StaffId).ToList(),
                //});

                //if (auditResult.StatusCode != Utils.Success)
                //{
                //    await dbTransaction.RollbackAsync();

                //    return StatusCode(StatusCodes.Status400BadRequest, new ReturnResponse()
                //    {
                //        StatusCode = Utils.AuditReportError,
                //        StatusMessage = Utils.StatusMessageAuditReportError
                //    });
                //}

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
        /// SET A NEW PASSWORD FOR A STAFF
        /// </summary>
        [RequiredFunctionalityName("PostSetStaffPassword")]
        [AllowAnonymous]
        [HttpPost("SetPassword")]
        public async Task<ActionResult<ReturnResponse>> PostSetStaffPassword([FromForm] StaffPasswordRequest staffPasswordRequest)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _staffRepository.SetStaffPasswordAndProfilePicture(staffPasswordRequest);

            if (result.StatusCode == Utils.Success)
            {
                result.StatusMessage = "Login Success!!!";
                var userDetails = (UserDetails)result.ObjectValue;
                var userInfoToReturn = _mapper.Map<UserLoginResponse>(userDetails);
                if (userDetails.User.UserType == Utils.Customer)
                {
                    //CUSTOMER
                    userInfoToReturn.UserProfileInformation = _mapper.Map<CustomerResponse>((Customer)userDetails.userProfile);
                }
                else
                {
                    //STAFF
                    userInfoToReturn.UserProfileInformation = _mapper.Map<StaffResponse>((Staff)userDetails.userProfile);
                }

                result.ObjectValue = userInfoToReturn;
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "PostSetStaffPassword",
                    AuditReportActivityResourceId = new List<int>() { ((Staff)userDetails.userProfile).StaffId },
                    UserId = userInfoToReturn.User.Id
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
        /// DELETE A STAFF
        /// </summary>
        // DELETE: api/Staff/5
        [RequiredFunctionalityName("DeleteStaff")]
        [HttpPost("Delete")]
        public async Task<ActionResult<ReturnResponse>> DeleteStaff([FromBody] List<StaffResponse> staff)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _staffRepository.DeleteStaff(staff);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<StaffResponse>>((List<Staff>)result.ObjectValue);
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "DeleteStaff",
                    AuditReportActivityResourceId = staff.Select(a => a.StaffId).ToList()
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

        // GET: api/Staffs/SupportLevel/Count
        [RequiredFunctionalityName("GetStaffCountInSupportLevel")]
        [HttpGet("SupportLevel/Count")]
        public async Task<ActionResult<ReturnResponse>> GetStaffCountInSupportLevel()
        {
            var result = await _staffRepository.GetSupportLevelStaffCount();

            if (result.StatusCode == Utils.Success)
            {
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "GetStaffCountInSupportLevel",
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
        /// SEARCH FOR A STAFF
        /// </summary>
        // GET: api/Staff/Search/{searchParams}
        [RequiredFunctionalityName("SearchStaff")]
        [HttpGet("Search/{searchParams}")]
        public async Task<ActionResult> SearchStaff(string searchParams, [FromQuery] UserParams userParams)
        {
            var result = await _staffRepository.SearchStaff(searchParams, userParams);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<StaffWithRoleToReturn>>((List<User>)result.ObjectValue);
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "SearchStaff",
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
