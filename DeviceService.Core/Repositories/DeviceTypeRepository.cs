using AutoMapper;
using DeviceService.Core.Data.DataContext;
using DeviceService.Core.Dtos.Device;
using DeviceService.Core.Dtos.DeviceType;
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DeviceService.Core.Helpers.Common.Utils;

namespace DeviceService.Core.Repositories
{
    public class DeviceTypeRepository : IDeviceTypeRepository
    {
        private string className = string.Empty;

        private readonly IMapper _mapper;
        private readonly IGlobalRepository _globalRepository;
        private readonly DeviceContext _deviceContext;

        public DeviceTypeRepository(IMapper mapper, IGlobalRepository globalRepository, DeviceContext deviceContext)
        {
            className = GetType().Name;

            _mapper = mapper;
            _globalRepository = globalRepository;
            _deviceContext = deviceContext;
        }

        public async Task<ReturnResponse<DeviceTypeResponse>> CreateDeviceType(DeviceTypeRequest deviceTypeRequest)
        {
            string methodName = "CreateDeviceType"/*MethodBase.GetCurrentMethod().Name*/, classAndMethodName = $"{className}.{methodName}";

            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Request for Inserting Device Type to The Database. Payload: {JsonConvert.SerializeObject(deviceTypeRequest)}").AppendLine();

            try
            {
                var deviceType = _mapper.Map<DeviceType>(deviceTypeRequest);

                //ADD RECORD TO DEVICE CONTEXT. START TRACKING
                _globalRepository.Add(deviceType);

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About to Save Device Type to the Database").AppendLine();
                var result = await _globalRepository.SaveAll();

                if (!result.HasValue)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Saving to the Database was not Successful").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<DeviceTypeResponse>()
                    {
                        StatusCode = Utils.NotSucceeded,
                        StatusMessage = "Unable to Insert Device Type...Try Again Later",
                        Logs = logs
                    };
                }

                if (!result.Value)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Saving to the Database was not Successful. No Row Affected").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<DeviceTypeResponse>()
                    {
                        StatusCode = Utils.SaveNoRowAffected,
                        StatusMessage = "Unable to Insert Device Type...Try Again Later",
                        Logs = logs
                    };
                }

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Inserting Record to the DB was Successful").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<DeviceTypeResponse>()
                {
                    StatusCode = Utils.StatusCode_Success,
                    StatusMessage = Utils.StatusMessage_Success,
                    ObjectValue = _mapper.Map<DeviceTypeResponse>(deviceType),
                    Logs = logs
                };
            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, "CreateDeviceType Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered while Inserting Device Type to the Database").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<DeviceTypeResponse>()
                {
                    StatusCode = Utils.StatusCode_ExceptionError,
                    StatusMessage = "Unable to Insert Device Type...Try Again Later",
                    Logs = logs
                };
            }
        }

        public async Task<ReturnResponse<List<DeviceTypeResponse>>> DeleteDeviceType(List<int> deviceTypeIds)
        {
            string methodName = "DeleteDeviceType"/*MethodBase.GetCurrentMethod().Name*/, classAndMethodName = $"{className}.{methodName}";

            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Request for Deleting Device Types from The Database. Payload: {JsonConvert.SerializeObject(deviceTypeIds)}").AppendLine();

            var deviceTypesToDelete = new List<DeviceType>();

            try
            {
                foreach (var deviceTypeId in deviceTypeIds)
                {
                    var deviceType = await _deviceContext.DeviceType.Where(a => a.DeviceTypeId == deviceTypeId).Include(b => b.DeviceTypeOperations).FirstOrDefaultAsync();

                    if (deviceType == null)
                    {
                        logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Device Type Not Found. Deleting Record from the Database was not Successful. RecordId: {deviceTypeId}").AppendLine();
                        logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                        logBuilder.ToString().AddToLogs(ref logs);

                        return new ReturnResponse<List<DeviceTypeResponse>>()
                        {
                            StatusCode = Utils.NotFound,
                            StatusMessage = "Device Type Not Found. Unable to Delete Device Types...Try Again Later",
                            Logs = logs
                        };
                    }

                    deviceTypesToDelete.Add(deviceType);
                }

                //DELETE RECORDS FROM DEVICE CONTEXT. START TRACKING
                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About to Delete Device Types from the Database").AppendLine();
                var deletionResult = _globalRepository.Delete(deviceTypesToDelete);

                if (!deletionResult)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Deleting Records from the Database was not Successful.").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<List<DeviceTypeResponse>>()
                    {
                        StatusCode = Utils.NotSucceeded,
                        StatusMessage = "Unable to Delete Device Types...Try Again Later",
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

                    return new ReturnResponse<List<DeviceTypeResponse>>()
                    {
                        StatusCode = Utils.NotSucceeded,
                        StatusMessage = "Unable to Delete Device Types...Try Again Later",
                        Logs = logs
                    };
                }

                if (!result.Value)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Saving to the Database was not Successful. No Row Affected").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<List<DeviceTypeResponse>>()
                    {
                        StatusCode = Utils.NotSucceeded,
                        StatusMessage = "Unable to Delete Device Types...Try Again Later",
                        Logs = logs
                    };
                }

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Deleting Records from the DB was Successful").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<List<DeviceTypeResponse>>()
                {
                    StatusCode = Utils.StatusCode_Success,
                    StatusMessage = Utils.StatusMessage_Success,
                    ObjectValue = _mapper.Map<List<DeviceTypeResponse>>(deviceTypesToDelete),
                    Logs = logs
                };
            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, "DeleteDeviceType Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered while Deleting Device Types from the Database").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<List<DeviceTypeResponse>>()
                {
                    StatusCode = Utils.StatusCode_ExceptionError,
                    StatusMessage = "Unable to Delete Device Types...Try Again Later",
                    Logs = logs
                };
            }
        }

        public async Task<ReturnResponse<DeviceTypeResponse>> GetDeviceType(int deviceTypeId)
        {
            string methodName = "GetDeviceType"/*MethodBase.GetCurrentMethod().Name*/, classAndMethodName = $"{className}.{methodName}";

            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Request for Getting Device Type from The Database. Payload: {deviceTypeId}").AppendLine();

            try
            {
                var deviceType = await _deviceContext.DeviceType.Where(a => a.DeviceTypeId == deviceTypeId).Include(b => b.DeviceTypeOperations).ThenInclude(c => c.DeviceOperation).FirstOrDefaultAsync();

                if (deviceType == null)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Device Type Not Found.").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<DeviceTypeResponse>()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = "Device Type Not Found.",
                        Logs = logs
                    };
                }

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Fetching Record from the DB was Successful").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<DeviceTypeResponse>()
                {
                    StatusCode = Utils.StatusCode_Success,
                    StatusMessage = Utils.StatusMessage_Success,
                    ObjectValue = _mapper.Map<DeviceTypeResponse>(deviceType),
                    Logs = logs
                };
            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, "GetDeviceType Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered while Getting Device Type from the Database").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<DeviceTypeResponse>()
                {
                    StatusCode = Utils.StatusCode_ExceptionError,
                    StatusMessage = "Unable to Get Device Type...Try Again Later",
                    Logs = logs
                };
            }
        }

        public async Task<ReturnResponse<List<DeviceTypeResponse>>> GetDeviceTypes(UserParams userParam)
        {
            string methodName = "GetDeviceTypes"/*MethodBase.GetCurrentMethod().Name*/, classAndMethodName = $"{className}.{methodName}";

            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Request for Getting Device Types from The Database.").AppendLine();

            try
            {
                var deviceTypes = _deviceContext.DeviceType;

                var pagedList = await PagedList<DeviceType>.CreateAsync(deviceTypes, userParam.PageNumber, userParam.PageSize);
                MyHttpContextAccessor.GetHttpContextAccessor().HttpContext.Response.AddPagination(pagedList.CurrentPage, pagedList.PageSize, pagedList.TotalCount, pagedList.TotalPages);
                var listOfDeviceTypesToReturn = pagedList.ToList();

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Fetching Records from the DB was Successful").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<List<DeviceTypeResponse>>()
                {
                    StatusCode = Utils.StatusCode_Success,
                    StatusMessage = Utils.StatusMessage_Success,
                    ObjectValue = _mapper.Map<List<DeviceTypeResponse>>(listOfDeviceTypesToReturn),
                    Logs = logs
                };
            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, "GetDeviceTypes Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered while Getting Device Types from the Database").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<List<DeviceTypeResponse>>()
                {
                    StatusCode = Utils.StatusCode_ExceptionError,
                    StatusMessage = "Unable to Get Device Types...Try Again Later",
                    Logs = logs
                };
            }
        }

        public async Task<ReturnResponse<DeviceTypeResponse>> UpdateDeviceType(int deviceTypeId, DeviceTypeToUpdate deviceTypeToUpdate)
        {
            string methodName = "UpdateDeviceType"/*MethodBase.GetCurrentMethod().Name*/, classAndMethodName = $"{className}.{methodName}";

            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Request for Updating Device Type to The Database. Payload: {JsonConvert.SerializeObject(deviceTypeToUpdate)}").AppendLine();

            if (deviceTypeId != deviceTypeToUpdate.DeviceTypeId)
            {
                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Device Types Ids did not Match.").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<DeviceTypeResponse>()
                {
                    StatusCode = Utils.BadRequest,
                    StatusMessage = "Unable to Update Device Type...Try Again Later",
                    Logs = logs
                };
            }

            try
            {
                var deviceType = await _globalRepository.Get<DeviceType>(deviceTypeId);
                if (deviceType == null)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Device Type Not Found.").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<DeviceTypeResponse>()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = "Device Type Not Found. Unable to Update Device Type...Try Again Later",
                        Logs = logs
                    };
                }

                var deviceTypeToUpdateMain = _mapper.Map(deviceTypeToUpdate, deviceType);
                //UPDATE RECORD TO DEVICE CONTEXT. START TRACKING
                _globalRepository.Update(deviceTypeToUpdateMain);

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About to Save Device Type to the Database").AppendLine();
                var result = await _globalRepository.SaveAll();

                if (!result.HasValue)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Saving to the Database was not Successful").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<DeviceTypeResponse>()
                    {
                        StatusCode = Utils.NotSucceeded,
                        StatusMessage = "Unable to Update Device Type...Try Again Later",
                        Logs = logs
                    };
                }

                if (!result.Value)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Saving to the Database was not Successful. No Row Affected").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<DeviceTypeResponse>()
                    {
                        StatusCode = Utils.SaveNoRowAffected,
                        StatusMessage = "Unable to Update Device Type...Try Again Later",
                        Logs = logs
                    };
                }

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Updating Record to the DB was Successful").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<DeviceTypeResponse>()
                {
                    StatusCode = Utils.StatusCode_Success,
                    StatusMessage = Utils.StatusMessage_Success,
                    ObjectValue = _mapper.Map<DeviceTypeResponse>(deviceType),
                    Logs = logs
                };
            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, "UpdateDeviceType Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered while Updating Device Type to the Database").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<DeviceTypeResponse>()
                {
                    StatusCode = Utils.StatusCode_ExceptionError,
                    StatusMessage = "Unable to Update Device Type...Try Again Later",
                    Logs = logs
                };
            }
        }
    }
}
