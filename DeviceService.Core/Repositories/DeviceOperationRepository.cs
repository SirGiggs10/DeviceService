using AutoMapper;
using DeviceService.Core.Data.DataContext;
using DeviceService.Core.Dtos.DeviceOperation;
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
    public class DeviceOperationRepository : IDeviceOperationRepository
    {
        private string className = string.Empty;

        private readonly IMapper _mapper;
        private readonly IGlobalRepository _globalRepository;
        private readonly DeviceContext _deviceContext;

        public DeviceOperationRepository(IMapper mapper, IGlobalRepository globalRepository, DeviceContext deviceContext)
        {
            className = GetType().Name;

            _mapper = mapper;
            _globalRepository = globalRepository;
            _deviceContext = deviceContext;
        }

        public async Task<ReturnResponse<DeviceOperationResponse>> CreateDeviceOperation(DeviceOperationRequest deviceOperationRequest)
        {
            string methodName = "CreateDeviceOperation"/*MethodBase.GetCurrentMethod().Name*/, classAndMethodName = $"{className}.{methodName}";

            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Request for Inserting Device Operation to The Database. Payload: {JsonConvert.SerializeObject(deviceOperationRequest)}").AppendLine();

            try
            {
                var deviceOperation = _mapper.Map<DeviceOperation>(deviceOperationRequest);

                //ADD RECORD TO DEVICE CONTEXT. START TRACKING
                _globalRepository.Add(deviceOperation);

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About to Save Device Operation to the Database").AppendLine();
                var result = await _globalRepository.SaveAll();

                if(!result.HasValue)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Saving to the Database was not Successful").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<DeviceOperationResponse>()
                    {
                        StatusCode = Utils.NotSucceeded,
                        StatusMessage = "Unable to Insert Device Operation...Try Again Later",
                        Logs = logs
                    };
                }

                if(!result.Value)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Saving to the Database was not Successful. No Row Affected").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<DeviceOperationResponse>()
                    {
                        StatusCode = Utils.SaveNoRowAffected,
                        StatusMessage = "Unable to Insert Device Operation...Try Again Later",
                        Logs = logs
                    };
                }

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Inserting Record to the DB was Successful").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<DeviceOperationResponse>()
                {
                    StatusCode = Utils.StatusCode_Success,
                    StatusMessage = Utils.StatusMessage_Success,
                    ObjectValue = _mapper.Map<DeviceOperationResponse>(deviceOperation),
                    Logs = logs
                };
            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, "CreateDeviceOperation Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered while Inserting Device Operation to the Database").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<DeviceOperationResponse>()
                {
                    StatusCode = Utils.StatusCode_ExceptionError,
                    StatusMessage = "Unable to Insert Device Operation...Try Again Later",
                    Logs = logs
                };
            }
        }

        public async Task<ReturnResponse<List<DeviceOperationResponse>>> DeleteDeviceOperation(List<int> deviceOperationIds)
        {
            string methodName = "DeleteDeviceOperation"/*MethodBase.GetCurrentMethod().Name*/, classAndMethodName = $"{className}.{methodName}";

            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Request for Deleting Device Operations from The Database. Payload: {JsonConvert.SerializeObject(deviceOperationIds)}").AppendLine();

            var deviceOperationsToDelete = new List<DeviceOperation>();

            try
            {
                foreach(var deviceOperationId in deviceOperationIds)
                {
                    var deviceOperation = await _globalRepository.Get<DeviceOperation>(deviceOperationId);
                    
                    if(deviceOperation == null)
                    {
                        logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Device Operation Not Found. Deleting Record from the Database was not Successful. RecordId: {deviceOperationId}").AppendLine();
                        logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                        logBuilder.ToString().AddToLogs(ref logs);

                        return new ReturnResponse<List<DeviceOperationResponse>>()
                        {
                            StatusCode = Utils.NotFound,
                            StatusMessage = "Device Operation Not Found. Unable to Delete Device Operations...Try Again Later",
                            Logs = logs
                        };
                    }

                    deviceOperationsToDelete.Add(deviceOperation);
                }

                //DELETE RECORDS FROM DEVICE CONTEXT. START TRACKING
                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About to Delete Device Operations from the Database").AppendLine();
                var deletionResult = _globalRepository.Delete(deviceOperationsToDelete);

                if(!deletionResult)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Deleting Records from the Database was not Successful.").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<List<DeviceOperationResponse>>()
                    {
                        StatusCode = Utils.NotSucceeded,
                        StatusMessage = "Unable to Delete Device Operations...Try Again Later",
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

                    return new ReturnResponse<List<DeviceOperationResponse>>()
                    {
                        StatusCode = Utils.NotSucceeded,
                        StatusMessage = "Unable to Delete Device Operations...Try Again Later",
                        Logs = logs
                    };
                }

                if (!result.Value)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Saving to the Database was not Successful. No Row Affected").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<List<DeviceOperationResponse>>()
                    {
                        StatusCode = Utils.NotSucceeded,
                        StatusMessage = "Unable to Delete Device Operations...Try Again Later",
                        Logs = logs
                    };
                }

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Deleting Records from the DB was Successful").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<List<DeviceOperationResponse>>()
                {
                    StatusCode = Utils.StatusCode_Success,
                    StatusMessage = Utils.StatusMessage_Success,
                    ObjectValue = _mapper.Map<List<DeviceOperationResponse>>(deviceOperationsToDelete),
                    Logs = logs
                };
            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, "DeleteDeviceOperation Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered while Deleting Device Operations from the Database").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<List<DeviceOperationResponse>>()
                {
                    StatusCode = Utils.StatusCode_ExceptionError,
                    StatusMessage = "Unable to Delete Device Operations...Try Again Later",
                    Logs = logs
                };
            }
        }

        public async Task<ReturnResponse<DeviceOperationResponse>> GetDeviceOperation(int deviceOperationId)
        {
            string methodName = "GetDeviceOperation"/*MethodBase.GetCurrentMethod().Name*/, classAndMethodName = $"{className}.{methodName}";

            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Request for Getting Device Operation from The Database. Payload: {deviceOperationId}").AppendLine();

            try
            {
                var deviceOperation = await _deviceContext.DeviceOperation.Where(a => a.DeviceOperationId == deviceOperationId).FirstOrDefaultAsync();
                
                if (deviceOperation == null)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Device Operation Not Found.").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<DeviceOperationResponse>()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = "Device Operation Not Found.",
                        Logs = logs
                    };
                }

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Fetching Record from the DB was Successful").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<DeviceOperationResponse>()
                {
                    StatusCode = Utils.StatusCode_Success,
                    StatusMessage = Utils.StatusMessage_Success,
                    ObjectValue = _mapper.Map<DeviceOperationResponse>(deviceOperation),
                    Logs = logs
                };
            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, "GetDeviceOperation Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered while Getting Device Operation from the Database").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<DeviceOperationResponse>()
                {
                    StatusCode = Utils.StatusCode_ExceptionError,
                    StatusMessage = "Unable to Get Device Operation...Try Again Later",
                    Logs = logs
                };
            }
        }

        public async Task<ReturnResponse<List<DeviceOperationResponse>>> GetDeviceOperations(UserParams userParam)
        {
            string methodName = "GetDeviceOperations"/*MethodBase.GetCurrentMethod().Name*/, classAndMethodName = $"{className}.{methodName}";

            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Request for Getting Device Operations from The Database.").AppendLine();

            try
            {
                var deviceOperations = _deviceContext.DeviceOperation;

                var pagedList = await PagedList<DeviceOperation>.CreateAsync(deviceOperations, userParam.PageNumber, userParam.PageSize);
                MyHttpContextAccessor.GetHttpContextAccessor().HttpContext.Response.AddPagination(pagedList.CurrentPage, pagedList.PageSize, pagedList.TotalCount, pagedList.TotalPages);
                var listOfDeviceOperationsToReturn = pagedList.ToList();

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Fetching Records from the DB was Successful").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<List<DeviceOperationResponse>>()
                {
                    StatusCode = Utils.StatusCode_Success,
                    StatusMessage = Utils.StatusMessage_Success,
                    ObjectValue = _mapper.Map<List<DeviceOperationResponse>>(listOfDeviceOperationsToReturn),
                    Logs = logs
                };
            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, "GetDeviceOperations Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered while Getting Device Operations from the Database").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<List<DeviceOperationResponse>>()
                {
                    StatusCode = Utils.StatusCode_ExceptionError,
                    StatusMessage = "Unable to Get Device Operations...Try Again Later",
                    Logs = logs
                };
            }
        }

        public async Task<ReturnResponse<DeviceOperationResponse>> UpdateDeviceOperation(int deviceOperationId, DeviceOperationToUpdate deviceOperationToUpdate)
        {
            string methodName = "UpdateDeviceOperation"/*MethodBase.GetCurrentMethod().Name*/, classAndMethodName = $"{className}.{methodName}";

            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Request for Updating Device Operation to The Database. Payload: {JsonConvert.SerializeObject(deviceOperationToUpdate)}").AppendLine();

            if (deviceOperationId != deviceOperationToUpdate.DeviceOperationId)
            {
                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Device Operations Ids did not Match.").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<DeviceOperationResponse>()
                {
                    StatusCode = Utils.BadRequest,
                    StatusMessage = "Unable to Update Device Operation...Try Again Later",
                    Logs = logs
                };
            }

            try
            {
                var deviceOperation = await _globalRepository.Get<DeviceOperation>(deviceOperationId);
                if (deviceOperation == null)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Device Operation Not Found.").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<DeviceOperationResponse>()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = "Device Operation Not Found. Unable to Update Device Operation...Try Again Later",
                        Logs = logs
                    };
                }

                var deviceOperationToUpdateMain = _mapper.Map(deviceOperationToUpdate, deviceOperation);
                //UPDATE RECORD TO DEVICE CONTEXT. START TRACKING
                _globalRepository.Update(deviceOperationToUpdateMain);

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About to Save Device Operation to the Database").AppendLine();
                var result = await _globalRepository.SaveAll();

                if (!result.HasValue)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Saving to the Database was not Successful").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<DeviceOperationResponse>()
                    {
                        StatusCode = Utils.NotSucceeded,
                        StatusMessage = "Unable to Update Device Operation...Try Again Later",
                        Logs = logs
                    };
                }

                if (!result.Value)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Saving to the Database was not Successful. No Row Affected").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<DeviceOperationResponse>()
                    {
                        StatusCode = Utils.SaveNoRowAffected,
                        StatusMessage = "Unable to Update Device Operation...Try Again Later",
                        Logs = logs
                    };
                }

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Updating Record to the DB was Successful").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<DeviceOperationResponse>()
                {
                    StatusCode = Utils.StatusCode_Success,
                    StatusMessage = Utils.StatusMessage_Success,
                    ObjectValue = _mapper.Map<DeviceOperationResponse>(deviceOperation),
                    Logs = logs
                };
            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, "UpdateDeviceOperation Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered while Updating Device Operation to the Database").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<DeviceOperationResponse>()
                {
                    StatusCode = Utils.StatusCode_ExceptionError,
                    StatusMessage = "Unable to Update Device Operation...Try Again Later",
                    Logs = logs
                };
            }
        }
    }
}
