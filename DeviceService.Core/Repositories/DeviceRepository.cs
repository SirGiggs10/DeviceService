using AutoMapper;
using CloudinaryDotNet.Actions;
using DeviceService.Core.Data.DataContext;
using DeviceService.Core.Dtos.Device;
using DeviceService.Core.Dtos.Global;
using DeviceService.Core.Entities;
using DeviceService.Core.Helpers.Common;
using DeviceService.Core.Helpers.Extensions;
using DeviceService.Core.Helpers.Logging.Logger;
using DeviceService.Core.Helpers.Pagination;
using DeviceService.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static DeviceService.Core.Helpers.Common.Utils;

namespace DeviceService.Core.Repositories
{
    public class DeviceRepository : IDeviceRepository
    {
        private string className = string.Empty;

        private readonly IMapper _mapper;
        private readonly IGlobalRepository _globalRepository;
        private readonly DeviceContext _deviceContext;
        private readonly ICloudinaryRepository _cloudinaryRepository;

        public DeviceRepository(IMapper mapper, IGlobalRepository globalRepository, DeviceContext deviceContext, ICloudinaryRepository cloudinaryRepository)
        {
            className = GetType().Name;

            _mapper = mapper;
            _globalRepository = globalRepository;
            _deviceContext = deviceContext;
            _cloudinaryRepository = cloudinaryRepository;
        }

        public async Task<ReturnResponse<DeviceResponse>> CreateDevice(DeviceRequest deviceRequest)
        {
            string methodName = "CreateDevice"/*MethodBase.GetCurrentMethod().Name*/, classAndMethodName = $"{className}.{methodName}";

            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Request for Inserting Device to The Database. Payload: {JsonConvert.SerializeObject(deviceRequest)}").AppendLine();

            try
            {
                var deviceToStore = _mapper.Map<Device>(deviceRequest);

                //GET LOGGED IN USER
                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About to Get Logged In User Claim.").AppendLine();
                var userClaim = MyHttpContextAccessor.GetHttpContextAccessor()?.HttpContext?.User?.Claims?.FirstOrDefault(a => a.Type == ClaimTypes.Name);

                if(userClaim == null)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Logged In User Claim Not Found").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<DeviceResponse>()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = "Unable to Create Device...Try Again Later",
                        Logs = logs
                    };
                }

                //VALIDATE DEVICE TYPE
                var deviceType = await _globalRepository.Get<DeviceType>(deviceRequest.DeviceTypeId);
                if(deviceType == null)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Device Type Not Found").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<DeviceResponse>()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = "Device Type Not Found...Try Again Later",
                        Logs = logs
                    };
                }

                deviceToStore.DeviceTypeId = deviceType.DeviceTypeId;
                deviceToStore.UserId = Convert.ToInt32(userClaim.Value);

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About to Upload Device Icon Image to Cloudinary Server.").AppendLine();
                //STORE DEVICE ICON TO CLOUDINARY
                var cloudinaryResult = _cloudinaryRepository.UploadFilesToCloudinary(deviceRequest.DeviceIcon);
                if (cloudinaryResult.StatusCode != Utils.Success)
                {
                    if (cloudinaryResult.StatusCode != Utils.ObjectNull)
                    {
                        logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Uploading File to Cloudinary was not Successful.").AppendLine();
                        logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                        logBuilder.ToString().AddToLogs(ref logs);

                        return new ReturnResponse<DeviceResponse>()
                        {
                            StatusCode = Utils.CloudinaryFileUploadError,
                            StatusMessage = Utils.StatusMessageCloudinaryFileUploadError,
                            Logs = logs
                        };
                    }
                }
                else
                {
                    var cloudinaryUploadResult = (RawUploadResult)cloudinaryResult.ObjectValue;
                    deviceToStore.DeviceIconPublicId = cloudinaryUploadResult.PublicId;
                    deviceToStore.DeviceIconUrl = cloudinaryUploadResult.SecureUrl.ToString().Split(cloudinaryUploadResult.PublicId)[0] + cloudinaryUploadResult.PublicId + Path.GetExtension(deviceRequest.DeviceIcon.FileName);
                    deviceToStore.DeviceIconFileName = deviceRequest.DeviceIcon.FileName;
                }

                //ADD RECORD TO DEVICE CONTEXT. START TRACKING
                _globalRepository.Add(deviceToStore);

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About to Save Device to the Database").AppendLine();
                var result = await _globalRepository.SaveAll();

                if (!result.HasValue)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Saving to the Database was not Successful").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<DeviceResponse>()
                    {
                        StatusCode = Utils.NotSucceeded,
                        StatusMessage = "Unable to Insert Device...Try Again Later",
                        Logs = logs
                    };
                }

                if (!result.Value)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Saving to the Database was not Successful. No Row Affected").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<DeviceResponse>()
                    {
                        StatusCode = Utils.SaveNoRowAffected,
                        StatusMessage = "Unable to Insert Device...Try Again Later",
                        Logs = logs
                    };
                }

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Inserting Record to the DB was Successful").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<DeviceResponse>()
                {
                    StatusCode = Utils.StatusCode_Success,
                    StatusMessage = Utils.StatusMessage_Success,
                    ObjectValue = _mapper.Map<DeviceResponse>(deviceToStore),
                    Logs = logs
                };
            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, "CreateDevice Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered while Inserting Device to the Database").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<DeviceResponse>()
                {
                    StatusCode = Utils.StatusCode_ExceptionError,
                    StatusMessage = "Unable to Insert Device...Try Again Later",
                    Logs = logs
                };
            }
        }

        public async Task<ReturnResponse<List<DevicePartialResponse>>> DeleteDevice(List<int> deviceIds)
        {
            string methodName = "DeleteDevice"/*MethodBase.GetCurrentMethod().Name*/, classAndMethodName = $"{className}.{methodName}";

            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Request for Deleting Devices from The Database. Payload: {JsonConvert.SerializeObject(deviceIds)}").AppendLine();

            var devicesToDelete = new List<Device>();

            try
            {
                //GET LOGGED IN USER
                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About to Get Logged In User Claim.").AppendLine();
                
                var userClaim = MyHttpContextAccessor.GetHttpContextAccessor()?.HttpContext?.User?.Claims?.FirstOrDefault(a => a.Type == ClaimTypes.Name);

                if (userClaim == null)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Logged In User Claim Not Found").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<List<DevicePartialResponse>>()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = "User Not Found...Try Again Later",
                        Logs = logs
                    };
                }

                var userId = Convert.ToInt32(userClaim.Value);

                foreach (var deviceId in deviceIds)
                {
                    var device = await _deviceContext.Device.Where(a => a.DeviceId == deviceId).FirstOrDefaultAsync();

                    if (device == null)
                    {
                        logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Device Not Found. Deleting Record from the Database was not Successful. RecordId: {deviceId}").AppendLine();
                        logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                        logBuilder.ToString().AddToLogs(ref logs);

                        return new ReturnResponse<List<DevicePartialResponse>>()
                        {
                            StatusCode = Utils.NotFound,
                            StatusMessage = "Device Not Found. Unable to Delete Devices...Try Again Later",
                            Logs = logs
                        };
                    }

                    if(device.UserId != userId)
                    {
                        logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} LoggedIn User Id did not Match Device UserId. RecordId: {deviceId}").AppendLine();
                        logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                        logBuilder.ToString().AddToLogs(ref logs);

                        return new ReturnResponse<List<DevicePartialResponse>>()
                        {
                            StatusCode = Utils.BadRequest,
                            StatusMessage = "User Not Allowed To Delete Devices...Try Again Later",
                            Logs = logs
                        };
                    }

                    devicesToDelete.Add(device);
                }

                //DELETE RECORDS FROM DEVICE CONTEXT. START TRACKING
                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About to Delete Devices from the Database").AppendLine();
                var deletionResult = _globalRepository.Delete(devicesToDelete);

                if (!deletionResult)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Deleting Records from the Database was not Successful.").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<List<DevicePartialResponse>>()
                    {
                        StatusCode = Utils.NotSucceeded,
                        StatusMessage = "Unable to Delete Devices...Try Again Later",
                        Logs = logs
                    };
                }

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About to Save to the Database").AppendLine();
                var result = await _globalRepository.SaveAll();

                if (!result.HasValue)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Saving to the Database was not Successful.").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<List<DevicePartialResponse>>()
                    {
                        StatusCode = Utils.NotSucceeded,
                        StatusMessage = "Unable to Delete Devices...Try Again Later",
                        Logs = logs
                    };
                }

                if (!result.Value)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Saving to the Database was not Successful. No Row Affected").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<List<DevicePartialResponse>>()
                    {
                        StatusCode = Utils.NotSucceeded,
                        StatusMessage = "Unable to Delete Devices...Try Again Later",
                        Logs = logs
                    };
                }

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Deleting Records from the DB was Successful").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<List<DevicePartialResponse>>()
                {
                    StatusCode = Utils.StatusCode_Success,
                    StatusMessage = Utils.StatusMessage_Success,
                    ObjectValue = _mapper.Map<List<DevicePartialResponse>>(devicesToDelete),
                    Logs = logs
                };
            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, "DeleteDevice Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered while Deleting Devices from the Database").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<List<DevicePartialResponse>>()
                {
                    StatusCode = Utils.StatusCode_ExceptionError,
                    StatusMessage = "Unable to Delete Devices...Try Again Later",
                    Logs = logs
                };
            }
        }

        public async Task<ReturnResponse<DeviceResponse>> GetDevice(int deviceId)
        {
            string methodName = "GetDevice"/*MethodBase.GetCurrentMethod().Name*/, classAndMethodName = $"{className}.{methodName}";

            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Request for Getting Device from The Database. Payload: {deviceId}").AppendLine();

            try
            {
                var device = await _deviceContext.Device.Where(a => a.DeviceId == deviceId).Include(b => b.DeviceType).ThenInclude(c => c.DeviceTypeOperations).ThenInclude(d => d.DeviceOperation).Include(e => e.User).FirstOrDefaultAsync();

                if (device == null)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Device Not Found.").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<DeviceResponse>()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = "Device Not Found.",
                        Logs = logs
                    };
                }

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Fetching Record from the DB was Successful").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<DeviceResponse>()
                {
                    StatusCode = Utils.StatusCode_Success,
                    StatusMessage = Utils.StatusMessage_Success,
                    ObjectValue = _mapper.Map<DeviceResponse>(device),
                    Logs = logs
                };
            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, "GetDevice Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered while Getting Device from the Database").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<DeviceResponse>()
                {
                    StatusCode = Utils.StatusCode_ExceptionError,
                    StatusMessage = "Unable to Get Device...Try Again Later",
                    Logs = logs
                };
            }
        }

        public async Task<ReturnResponse<List<DeviceResponse>>> GetDevices(UserParams userParam)
        {
            string methodName = "GetDevices"/*MethodBase.GetCurrentMethod().Name*/, classAndMethodName = $"{className}.{methodName}";

            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Request for Getting Devices from The Database.").AppendLine();

            try
            {
                var devices = _deviceContext.Device;

                var pagedList = await PagedList<Device>.CreateAsync(devices, userParam.PageNumber, userParam.PageSize);
                MyHttpContextAccessor.GetHttpContextAccessor().HttpContext.Response.AddPagination(pagedList.CurrentPage, pagedList.PageSize, pagedList.TotalCount, pagedList.TotalPages);
                var listOfDevicesToReturn = pagedList.ToList();

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Fetching Records from the DB was Successful").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<List<DeviceResponse>>()
                {
                    StatusCode = Utils.StatusCode_Success,
                    StatusMessage = Utils.StatusMessage_Success,
                    ObjectValue = _mapper.Map<List<DeviceResponse>>(listOfDevicesToReturn),
                    Logs = logs
                };
            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, "GetDevices Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered while Getting Devices from the Database").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<List<DeviceResponse>>()
                {
                    StatusCode = Utils.StatusCode_ExceptionError,
                    StatusMessage = "Unable to Get Devices...Try Again Later",
                    Logs = logs
                };
            }
        }

        public async Task<ReturnResponse<List<DevicePartialResponse>>> GetDevicesByStatus(UserParams userParam, Utils.DeviceStatus deviceStatus)
        {
            string methodName = "GetDevicesByStatus"/*MethodBase.GetCurrentMethod().Name*/, classAndMethodName = $"{className}.{methodName}";

            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Request for Getting Devices from The Database.").AppendLine();

            try
            {
                var devices = _deviceContext.Device.Where(a => a.Status == deviceStatus.ToString());

                var pagedList = await PagedList<Device>.CreateAsync(devices, userParam.PageNumber, userParam.PageSize);
                MyHttpContextAccessor.GetHttpContextAccessor().HttpContext.Response.AddPagination(pagedList.CurrentPage, pagedList.PageSize, pagedList.TotalCount, pagedList.TotalPages);
                var listOfDevicesToReturn = pagedList.ToList();

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Fetching Records from the DB was Successful").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<List<DevicePartialResponse>>()
                {
                    StatusCode = Utils.StatusCode_Success,
                    StatusMessage = Utils.StatusMessage_Success,
                    ObjectValue = _mapper.Map<List<DevicePartialResponse>>(listOfDevicesToReturn),
                    Logs = logs
                };
            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, "GetDevicesByStatus Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered while Getting Devices from the Database").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<List<DevicePartialResponse>>()
                {
                    StatusCode = Utils.StatusCode_ExceptionError,
                    StatusMessage = "Unable to Get Devices...Try Again Later",
                    Logs = logs
                };
            }
        }

        public async Task<ReturnResponse<DeviceWithRelatedDevicesResponse>> GetDeviceWithRelatedDevices(int deviceId)
        {
            string methodName = "GetDeviceWithRelatedDevices"/*MethodBase.GetCurrentMethod().Name*/, classAndMethodName = $"{className}.{methodName}";

            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Request for Getting Device With Related Devices from The Database. Payload: {deviceId}").AppendLine();

            try
            {
                var device = await _deviceContext.Device.Where(a => a.DeviceId == deviceId).Include(b => b.DeviceType).ThenInclude(c => c.DeviceTypeOperations).ThenInclude(d => d.DeviceOperation).Include(e => e.User).FirstOrDefaultAsync();

                if (device == null)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Device Not Found.").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<DeviceWithRelatedDevicesResponse>()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = "Device Not Found.",
                        Logs = logs
                    };
                }

                var relatedDevices = await _deviceContext.Device.Where(a => (a.DeviceId != device.DeviceId) && (a.DeviceTypeId == device.DeviceTypeId)).ToListAsync();

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Fetching Record from the DB was Successful").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                var deviceWithRelatedDevices = _mapper.Map<DeviceWithRelatedDevicesResponse>(device);
                deviceWithRelatedDevices.RelatedDevices = _mapper.Map<List<DevicePartialResponse>>(relatedDevices);

                return new ReturnResponse<DeviceWithRelatedDevicesResponse>()
                {
                    StatusCode = Utils.StatusCode_Success,
                    StatusMessage = Utils.StatusMessage_Success,
                    ObjectValue = deviceWithRelatedDevices,
                    Logs = logs
                };
            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, "GetDeviceWithRelatedDevices Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered while Getting Device from the Database").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<DeviceWithRelatedDevicesResponse>()
                {
                    StatusCode = Utils.StatusCode_ExceptionError,
                    StatusMessage = "Unable to Get Device With Related Devices...Try Again Later",
                    Logs = logs
                };
            }
        }

        public async Task<ReturnResponse<List<DeviceResponse>>> SearchUserDevice(DeviceSearchRequest deviceSearchRequest, UserParams userParam)
        {
            string methodName = "SearchUserDevice"/*MethodBase.GetCurrentMethod().Name*/, classAndMethodName = $"{className}.{methodName}";

            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Request for Searching User Devices from The Database.").AppendLine();

            try
            {
                //GET LOGGED IN USER
                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About to Get Logged In User Claim.").AppendLine();

                var userClaim = MyHttpContextAccessor.GetHttpContextAccessor()?.HttpContext?.User?.Claims?.FirstOrDefault(a => a.Type == ClaimTypes.Name);

                if (userClaim == null)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Logged In User Claim Not Found").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<List<DeviceResponse>>()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = "User Not Found...Try Again Later",
                        Logs = logs
                    };
                }

                var userId = Convert.ToInt32(userClaim.Value);

                var devices = _deviceContext.Device.Include(t => t.DeviceType).Where(a => (a.UserId == userId) && ((a.Status.ToLower().Contains(deviceSearchRequest.SearchString.ToLower())) || (a.DeviceName.ToLower().Contains(deviceSearchRequest.SearchString.ToLower())) || (a.DeviceType.DeviceTypeName.ToLower().Contains(deviceSearchRequest.SearchString.ToLower()))));

                var pagedList = await PagedList<Device>.CreateAsync(devices, userParam.PageNumber, userParam.PageSize);
                MyHttpContextAccessor.GetHttpContextAccessor().HttpContext.Response.AddPagination(pagedList.CurrentPage, pagedList.PageSize, pagedList.TotalCount, pagedList.TotalPages);
                var listOfDevicesToReturn = pagedList.ToList();

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Searching Records on the DB was Successful").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<List<DeviceResponse>>()
                {
                    StatusCode = Utils.StatusCode_Success,
                    StatusMessage = Utils.StatusMessage_Success,
                    ObjectValue = _mapper.Map<List<DeviceResponse>>(listOfDevicesToReturn),
                    Logs = logs
                };
            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, "SearchUserDevice Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered while Searching User Devices in the Database").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<List<DeviceResponse>>()
                {
                    StatusCode = Utils.StatusCode_ExceptionError,
                    StatusMessage = "Unable to Search User Devices...Try Again Later",
                    Logs = logs
                };
            }
        }

        public async Task<ReturnResponse<DevicePartialResponse>> UpdateDeviceName(int deviceId, DeviceToUpdate deviceToUpdate)
        {
            string methodName = "UpdateDeviceName"/*MethodBase.GetCurrentMethod().Name*/, classAndMethodName = $"{className}.{methodName}";

            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Request for Updating Device to The Database. Payload: {JsonConvert.SerializeObject(deviceToUpdate)}").AppendLine();

            if (deviceId != deviceToUpdate.DeviceId)
            {
                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Devices Ids did not Match.").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<DevicePartialResponse>()
                {
                    StatusCode = Utils.BadRequest,
                    StatusMessage = "Unable to Update Device...Try Again Later",
                    Logs = logs
                };
            }

            try
            {
                var device = await _globalRepository.Get<Device>(deviceId);
                if (device == null)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Device Not Found.").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<DevicePartialResponse>()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = "Device Not Found. Unable to Update Device...Try Again Later",
                        Logs = logs
                    };
                }

                var deviceToUpdateMain = _mapper.Map(deviceToUpdate, device);
                //UPDATE RECORD TO DEVICE CONTEXT. START TRACKING
                _globalRepository.Update(deviceToUpdateMain);

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About to Save Device to the Database").AppendLine();
                var result = await _globalRepository.SaveAll();

                if (!result.HasValue)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Saving to the Database was not Successful").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<DevicePartialResponse>()
                    {
                        StatusCode = Utils.NotSucceeded,
                        StatusMessage = "Unable to Update Device...Try Again Later",
                        Logs = logs
                    };
                }

                if (!result.Value)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Saving to the Database was not Successful. No Row Affected").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<DevicePartialResponse>()
                    {
                        StatusCode = Utils.SaveNoRowAffected,
                        StatusMessage = "Unable to Update Device...Try Again Later",
                        Logs = logs
                    };
                }

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Updating Record to the DB was Successful").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<DevicePartialResponse>()
                {
                    StatusCode = Utils.StatusCode_Success,
                    StatusMessage = Utils.StatusMessage_Success,
                    ObjectValue = _mapper.Map<DevicePartialResponse>(device),
                    Logs = logs
                };
            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, "UpdateDeviceName Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered while Updating Device to the Database").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<DevicePartialResponse>()
                {
                    StatusCode = Utils.StatusCode_ExceptionError,
                    StatusMessage = "Unable to Update Device...Try Again Later",
                    Logs = logs
                };
            }
        }

        public async Task<ReturnResponse<DevicePartialResponse>> UpdateDeviceStatus(int deviceId, StatusUpdate statusUpdate)
        {
            string methodName = "UpdateDeviceStatus"/*MethodBase.GetCurrentMethod().Name*/, classAndMethodName = $"{className}.{methodName}";

            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Request for Updating Device to The Database. Payload: {JsonConvert.SerializeObject(statusUpdate)}").AppendLine();

            if (deviceId != statusUpdate.DeviceId)
            {
                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Devices Ids did not Match.").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<DevicePartialResponse>()
                {
                    StatusCode = Utils.BadRequest,
                    StatusMessage = "Unable to Update Device...Try Again Later",
                    Logs = logs
                };
            }

            try
            {
                var device = await _globalRepository.Get<Device>(deviceId);
                if (device == null)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Device Not Found.").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<DevicePartialResponse>()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = "Device Not Found. Unable to Update Device...Try Again Later",
                        Logs = logs
                    };
                }

                var deviceToUpdateMain = device;
                deviceToUpdateMain.Status = statusUpdate.Status.ToString();

                //UPDATE RECORD TO DEVICE CONTEXT. START TRACKING
                _globalRepository.Update(deviceToUpdateMain);

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About to Save Device to the Database").AppendLine();
                var result = await _globalRepository.SaveAll();

                if (!result.HasValue)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Saving to the Database was not Successful").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<DevicePartialResponse>()
                    {
                        StatusCode = Utils.NotSucceeded,
                        StatusMessage = "Unable to Update Device...Try Again Later",
                        Logs = logs
                    };
                }

                if (!result.Value)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Saving to the Database was not Successful. No Row Affected").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<DevicePartialResponse>()
                    {
                        StatusCode = Utils.SaveNoRowAffected,
                        StatusMessage = "Unable to Update Device...Try Again Later",
                        Logs = logs
                    };
                }

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Updating Record to the DB was Successful").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<DevicePartialResponse>()
                {
                    StatusCode = Utils.StatusCode_Success,
                    StatusMessage = Utils.StatusMessage_Success,
                    ObjectValue = _mapper.Map<DevicePartialResponse>(device),
                    Logs = logs
                };
            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, "UpdateDeviceStatus Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered while Updating Device to the Database").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<DevicePartialResponse>()
                {
                    StatusCode = Utils.StatusCode_ExceptionError,
                    StatusMessage = "Unable to Update Device...Try Again Later",
                    Logs = logs
                };
            }
        }

        public async Task<ReturnResponse<DevicePartialResponse>> UpdateDeviceTemperature(int deviceId, TemperatureUpdate temperatureUpdate)
        {
            string methodName = "UpdateDeviceTemperature"/*MethodBase.GetCurrentMethod().Name*/, classAndMethodName = $"{className}.{methodName}";

            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Request for Updating Device to The Database. Payload: {JsonConvert.SerializeObject(temperatureUpdate)}").AppendLine();

            if (deviceId != temperatureUpdate.DeviceId)
            {
                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Devices Ids did not Match.").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<DevicePartialResponse>()
                {
                    StatusCode = Utils.BadRequest,
                    StatusMessage = "Unable to Update Device...Try Again Later",
                    Logs = logs
                };
            }

            try
            {
                var device = await _globalRepository.Get<Device>(deviceId);
                if (device == null)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Device Not Found.").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<DevicePartialResponse>()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = "Device Not Found. Unable to Update Device...Try Again Later",
                        Logs = logs
                    };
                }

                var deviceToUpdateMain = _mapper.Map(temperatureUpdate, device);
                //UPDATE RECORD TO DEVICE CONTEXT. START TRACKING
                _globalRepository.Update(deviceToUpdateMain);

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About to Save Device to the Database").AppendLine();
                var result = await _globalRepository.SaveAll();

                if (!result.HasValue)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Saving to the Database was not Successful").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<DevicePartialResponse>()
                    {
                        StatusCode = Utils.NotSucceeded,
                        StatusMessage = "Unable to Update Device...Try Again Later",
                        Logs = logs
                    };
                }

                if (!result.Value)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Saving to the Database was not Successful. No Row Affected").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<DevicePartialResponse>()
                    {
                        StatusCode = Utils.SaveNoRowAffected,
                        StatusMessage = "Unable to Update Device...Try Again Later",
                        Logs = logs
                    };
                }

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Updating Record to the DB was Successful").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<DevicePartialResponse>()
                {
                    StatusCode = Utils.StatusCode_Success,
                    StatusMessage = Utils.StatusMessage_Success,
                    ObjectValue = _mapper.Map<DevicePartialResponse>(device),
                    Logs = logs
                };
            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, "UpdateDeviceTemperature Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered while Updating Device to the Database").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<DevicePartialResponse>()
                {
                    StatusCode = Utils.StatusCode_ExceptionError,
                    StatusMessage = "Unable to Update Device...Try Again Later",
                    Logs = logs
                };
            }
        }

        public async Task<ReturnResponse<DevicePartialResponse>> UpdateDeviceUsageHours(int deviceId, UsageUpdate usageUpdate)
        {
            string methodName = "UpdateDeviceUsageHours"/*MethodBase.GetCurrentMethod().Name*/, classAndMethodName = $"{className}.{methodName}";

            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Request for Updating Device to The Database. Payload: {JsonConvert.SerializeObject(usageUpdate)}").AppendLine();

            if (deviceId != usageUpdate.DeviceId)
            {
                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Devices Ids did not Match.").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<DevicePartialResponse>()
                {
                    StatusCode = Utils.BadRequest,
                    StatusMessage = "Unable to Update Device...Try Again Later",
                    Logs = logs
                };
            }

            try
            {
                var device = await _globalRepository.Get<Device>(deviceId);
                if (device == null)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Device Not Found.").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<DevicePartialResponse>()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = "Device Not Found. Unable to Update Device...Try Again Later",
                        Logs = logs
                    };
                }

                var deviceToUpdateMain = _mapper.Map(usageUpdate, device);
                //UPDATE RECORD TO DEVICE CONTEXT. START TRACKING
                _globalRepository.Update(deviceToUpdateMain);

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About to Save Device to the Database").AppendLine();
                var result = await _globalRepository.SaveAll();

                if (!result.HasValue)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Saving to the Database was not Successful").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<DevicePartialResponse>()
                    {
                        StatusCode = Utils.NotSucceeded,
                        StatusMessage = "Unable to Update Device...Try Again Later",
                        Logs = logs
                    };
                }

                if (!result.Value)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Saving to the Database was not Successful. No Row Affected").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<DevicePartialResponse>()
                    {
                        StatusCode = Utils.SaveNoRowAffected,
                        StatusMessage = "Unable to Update Device...Try Again Later",
                        Logs = logs
                    };
                }

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Updating Record to the DB was Successful").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<DevicePartialResponse>()
                {
                    StatusCode = Utils.StatusCode_Success,
                    StatusMessage = Utils.StatusMessage_Success,
                    ObjectValue = _mapper.Map<DevicePartialResponse>(device),
                    Logs = logs
                };
            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, "UpdateDeviceUsageHours Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered while Updating Device to the Database").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<DevicePartialResponse>()
                {
                    StatusCode = Utils.StatusCode_ExceptionError,
                    StatusMessage = "Unable to Update Device...Try Again Later",
                    Logs = logs
                };
            }
        }

        public async Task<ReturnResponse<List<DeviceResponse>>> GetDevicesForUser(UserParams userParam)
        {
            string methodName = "GetDevicesForUser"/*MethodBase.GetCurrentMethod().Name*/, classAndMethodName = $"{className}.{methodName}";

            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Request for Getting Devices from The Database.").AppendLine();

            try
            {
                //GET LOGGED IN USER
                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About to Get Logged In User Claim.").AppendLine();

                var userClaim = MyHttpContextAccessor.GetHttpContextAccessor()?.HttpContext?.User?.Claims?.FirstOrDefault(a => a.Type == ClaimTypes.Name);

                if (userClaim == null)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Logged In User Claim Not Found").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<List<DeviceResponse>>()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = "User Not Found...Try Again Later",
                        Logs = logs
                    };
                }

                var userId = Convert.ToInt32(userClaim.Value);

                var devices = _deviceContext.Device.Where(a => a.UserId == userId);

                var pagedList = await PagedList<Device>.CreateAsync(devices, userParam.PageNumber, userParam.PageSize);
                MyHttpContextAccessor.GetHttpContextAccessor().HttpContext.Response.AddPagination(pagedList.CurrentPage, pagedList.PageSize, pagedList.TotalCount, pagedList.TotalPages);
                var listOfDevicesToReturn = pagedList.ToList();

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Fetching Records from the DB was Successful").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<List<DeviceResponse>>()
                {
                    StatusCode = Utils.StatusCode_Success,
                    StatusMessage = Utils.StatusMessage_Success,
                    ObjectValue = _mapper.Map<List<DeviceResponse>>(listOfDevicesToReturn),
                    Logs = logs
                };
            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, "GetDevicesForUser Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered while Getting Devices from the Database").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<List<DeviceResponse>>()
                {
                    StatusCode = Utils.StatusCode_ExceptionError,
                    StatusMessage = "Unable to Get Devices...Try Again Later",
                    Logs = logs
                };
            }
        }

        public async Task<ReturnResponse<List<DevicePartialResponse>>> GetDevicesByStatusForUser(UserParams userParam, DeviceStatus deviceStatus)
        {
            string methodName = "GetDevicesByStatusForUser"/*MethodBase.GetCurrentMethod().Name*/, classAndMethodName = $"{className}.{methodName}";

            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Request for Getting Devices from The Database.").AppendLine();

            try
            {
                //GET LOGGED IN USER
                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About to Get Logged In User Claim.").AppendLine();

                var userClaim = MyHttpContextAccessor.GetHttpContextAccessor()?.HttpContext?.User?.Claims?.FirstOrDefault(a => a.Type == ClaimTypes.Name);

                if (userClaim == null)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Logged In User Claim Not Found").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<List<DevicePartialResponse>>()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = "User Not Found...Try Again Later",
                        Logs = logs
                    };
                }

                var userId = Convert.ToInt32(userClaim.Value);

                var devices = _deviceContext.Device.Where(a => (a.Status == deviceStatus.ToString()) && (a.UserId == userId));

                var pagedList = await PagedList<Device>.CreateAsync(devices, userParam.PageNumber, userParam.PageSize);
                MyHttpContextAccessor.GetHttpContextAccessor().HttpContext.Response.AddPagination(pagedList.CurrentPage, pagedList.PageSize, pagedList.TotalCount, pagedList.TotalPages);
                var listOfDevicesToReturn = pagedList.ToList();

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Fetching Records from the DB was Successful").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<List<DevicePartialResponse>>()
                {
                    StatusCode = Utils.StatusCode_Success,
                    StatusMessage = Utils.StatusMessage_Success,
                    ObjectValue = _mapper.Map<List<DevicePartialResponse>>(listOfDevicesToReturn),
                    Logs = logs
                };
            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, "GetDevicesByStatusForUser Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered while Getting Devices from the Database").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<List<DevicePartialResponse>>()
                {
                    StatusCode = Utils.StatusCode_ExceptionError,
                    StatusMessage = "Unable to Get Devices...Try Again Later",
                    Logs = logs
                };
            }
        }

        public async Task<ReturnResponse<List<DevicePartialResponse>>> GetDevicesByDeviceType(UserParams userParam, int deviceTypeId)
        {
            string methodName = "GetDevicesByDeviceType"/*MethodBase.GetCurrentMethod().Name*/, classAndMethodName = $"{className}.{methodName}";

            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Request for Getting Devices from The Database.").AppendLine();

            try
            {
                var devices = _deviceContext.Device.Where(a => a.DeviceTypeId == deviceTypeId);

                var pagedList = await PagedList<Device>.CreateAsync(devices, userParam.PageNumber, userParam.PageSize);
                MyHttpContextAccessor.GetHttpContextAccessor().HttpContext.Response.AddPagination(pagedList.CurrentPage, pagedList.PageSize, pagedList.TotalCount, pagedList.TotalPages);
                var listOfDevicesToReturn = pagedList.ToList();

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Fetching Records from the DB was Successful").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<List<DevicePartialResponse>>()
                {
                    StatusCode = Utils.StatusCode_Success,
                    StatusMessage = Utils.StatusMessage_Success,
                    ObjectValue = _mapper.Map<List<DevicePartialResponse>>(listOfDevicesToReturn),
                    Logs = logs
                };
            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, "GetDevicesByDeviceType Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered while Getting Devices from the Database").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<List<DevicePartialResponse>>()
                {
                    StatusCode = Utils.StatusCode_ExceptionError,
                    StatusMessage = "Unable to Get Devices...Try Again Later",
                    Logs = logs
                };
            }
        }

        public async Task<ReturnResponse<List<DevicePartialResponse>>> GetDevicesByDeviceTypeForUser(UserParams userParam, int deviceTypeId)
        {
            string methodName = "GetDevicesByDeviceTypeForUser"/*MethodBase.GetCurrentMethod().Name*/, classAndMethodName = $"{className}.{methodName}";

            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Request for Getting Devices from The Database.").AppendLine();

            try
            {
                //GET LOGGED IN USER
                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About to Get Logged In User Claim.").AppendLine();

                var userClaim = MyHttpContextAccessor.GetHttpContextAccessor()?.HttpContext?.User?.Claims?.FirstOrDefault(a => a.Type == ClaimTypes.Name);

                if (userClaim == null)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Logged In User Claim Not Found").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<List<DevicePartialResponse>>()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = "User Not Found...Try Again Later",
                        Logs = logs
                    };
                }

                var userId = Convert.ToInt32(userClaim.Value);

                var devices = _deviceContext.Device.Where(a => (a.DeviceTypeId == deviceTypeId) && (a.UserId == userId));

                var pagedList = await PagedList<Device>.CreateAsync(devices, userParam.PageNumber, userParam.PageSize);
                MyHttpContextAccessor.GetHttpContextAccessor().HttpContext.Response.AddPagination(pagedList.CurrentPage, pagedList.PageSize, pagedList.TotalCount, pagedList.TotalPages);
                var listOfDevicesToReturn = pagedList.ToList();

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Fetching Records from the DB was Successful").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<List<DevicePartialResponse>>()
                {
                    StatusCode = Utils.StatusCode_Success,
                    StatusMessage = Utils.StatusMessage_Success,
                    ObjectValue = _mapper.Map<List<DevicePartialResponse>>(listOfDevicesToReturn),
                    Logs = logs
                };
            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, "GetDevicesByDeviceTypeForUser Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered while Getting Devices from the Database").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<List<DevicePartialResponse>>()
                {
                    StatusCode = Utils.StatusCode_ExceptionError,
                    StatusMessage = "Unable to Get Devices...Try Again Later",
                    Logs = logs
                };
            }
        }
    }
}
