using AutoMapper;
using CBN_eNaira.Core.Dtos.Global;
using CBN_eNaira.Core.Dtos.Transaction.Other;
using CBN_eNaira.Core.Dtos.User;
using CBN_eNaira.Core.Dtos.User.Other;
using CBN_eNaira.Core.Helpers.Common;
using CBN_eNaira.Core.Helpers.Common.JWT;
using CBN_eNaira.Core.Helpers.ConfigurationSettings.ConfigManager;
using CBN_eNaira.Core.Helpers.Extensions;
using CBN_eNaira.Core.Helpers.Logging.Logger;
using CBN_eNaira.Core.Interfaces.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static CBN_eNaira.Core.Helpers.Common.Utils;

namespace CBN_eNaira.Core.Services.Controller
{
    public class UserService : IUserService
    {
        private string className = string.Empty;

        private readonly IUserTypeService _userTypeService;
        private readonly IFIActionService _fIActionService;
        private readonly IMapper _mapper;

        public UserService(IUserTypeService userTypeService, IFIActionService fIActionService, IMapper mapper)
        {
            className = GetType().Name;

            _userTypeService = userTypeService;
            _fIActionService = fIActionService;
            _mapper = mapper;
        }

        public async Task<ControllerReturnResponse<UserLoginResponse>> LoginUser(UserLoginRequest userLoginRequest, RequestHeader requestHeader)
        {
            string methodName = "LoginUser"/*MethodBase.GetCurrentMethod().Name*/, classAndMethodName = $"{className}.{methodName}"; //MethodBase.GetCurrentMethod().Name returns MoveNext for async methods because async methods are actually called inside a wrapper method MoveNext

            var logs = new List<Log>();

            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Endpoint Request for User Login. Username: {userLoginRequest.UserName}, RequestHeader: {JsonConvert.SerializeObject(requestHeader)}").AppendLine();

            try
            {
                #region User (Customer/Merchant) Login
                var userTypeLoginRequest = _mapper.Map<UserTypeLoginRequest>(userLoginRequest);
                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About to Call UserType Service User Login Method. Username: {userTypeLoginRequest.UserName}").AppendLine();
                //USER ENAIRA WALLET LOGIN
                var userTypeLoginResponse = await _userTypeService.LoginUserType(userTypeLoginRequest);

                //RECONCILE THE LOGS
                logBuilder.ToString().AddToLogs(ref logs);
                logBuilder.Clear();
                userTypeLoginResponse.Logs.AddToLogs(ref logs);

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Back from UserType Service User Login Method Call. Response: {JsonConvert.SerializeObject(userTypeLoginResponse)}").AppendLine();
                #endregion

                if(userTypeLoginResponse.StatusCode != Utils.StatusCode_Success)
                {
                    //ERROR OCCURED DURING USER ENAIRA WALLET LOGIN
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} UserTypeService LoginUserType Method Call was not Successful").AppendLine();

                    var controlleReturnResponse = HelperUtility.HandleStatusCodesForControllerReturn<UserLoginResponse>(userTypeLoginResponse);
                    controlleReturnResponse.StatusCode = Utils.StatusCode_CBNENairaEndpoint_UserLoginFailed;
                    controlleReturnResponse.ResponseDescription = Utils.StatusMessage_CBNENairaEndpoint_UserLoginFailed;

                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    //TODO: THIS MAKES THE LOGGING TO BE SCATTERED. SOMETIMES RESPONSE IS LOGGED BEFORE THIS LOGGING COMPLETES. CHECK THE BETTER WAY TO FIRE AND FORGET OR MAKE THE LOGGING TO BE SYNCHRONOUS INSTEAD OF FIRE AND FORGET
                    //FINALLY WRITE THE LIST OF LOGS TO SINK(FILE)...FIRE THIS METHOD IN A SEPARATE THREAD AND FORGET IT. BECAUSE OF SPEED...
                    //Task.Run(() => LogWriter.WriteLog(logs));
                    LogWriter.WriteLog(logs);

                    return controlleReturnResponse;
                }

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} UserTypeService LoginUserType Method Call was Successful").AppendLine();

                #region Financial Institution (Bank) Login
                var fiLoginDetails = ConfigSettings.FinancialInstitutionLoginDetails;
                var fiLoginRequest = _mapper.Map<BankLoginRequest>(fiLoginDetails);
                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About to Call FIActionService LoginBank Method. Username: {fiLoginRequest.UserName}").AppendLine();
                //FI ENAIRA LOGIN
                var fiLoginResponse = await _fIActionService.LoginBank(fiLoginRequest);

                //RECONCILE THE LOGS
                logBuilder.ToString().AddToLogs(ref logs);
                logBuilder.Clear();
                fiLoginResponse.Logs.AddToLogs(ref logs);

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Back from FIActionService LoginBank Method Call. Response: {JsonConvert.SerializeObject(fiLoginResponse)}").AppendLine();
                #endregion

                if (fiLoginResponse.StatusCode != Utils.StatusCode_Success)
                {
                    //ERROR OCCURED DURING FI ENAIRA WALLET LOGIN
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} FIActionService LoginBank Method Call was not Successful").AppendLine();

                    var controlleReturnResponse = HelperUtility.HandleStatusCodesForControllerReturn<UserLoginResponse>(fiLoginResponse);
                    controlleReturnResponse.StatusCode = Utils.StatusCode_CBNENairaEndpoint_FILoginFailed;
                    controlleReturnResponse.ResponseDescription = Utils.StatusMessage_CBNENairaEndpoint_FILoginFailed;

                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    //TODO: THIS MAKES THE LOGGING TO BE SCATTERED. SOMETIMES RESPONSE IS LOGGED BEFORE THIS LOGGING COMPLETES. CHECK THE BETTER WAY TO FIRE AND FORGET OR MAKE THE LOGGING TO BE SYNCHRONOUS INSTEAD OF FIRE AND FORGET
                    //FINALLY WRITE THE LIST OF LOGS TO SINK(FILE)...FIRE THIS METHOD IN A SEPARATE THREAD AND FORGET IT. BECAUSE OF SPEED...
                    //Task.Run(() => LogWriter.WriteLog(logs));
                    LogWriter.WriteLog(logs);

                    return controlleReturnResponse;
                }

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} FIActionService LoginBank Method Call was Successful").AppendLine();

                #region User Auth Token Decryption
                //DECRYPT USER AUTH TOKEN
                var userId = JWTHelper.JwtDecoderGetClaimValue(userTypeLoginResponse.ObjectValue?.Token, Utils.ClaimType_UserId);
                #endregion

                if (userId == null)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} User Token Decryption Failed. Token: {userTypeLoginResponse.ObjectValue?.Token}").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    //Task.Run(() => LogWriter.WriteLog(logs));
                    LogWriter.WriteLog(logs);

                    return new ControllerReturnResponse<UserLoginResponse>()
                    {
                        ResponseCode = (HttpStatusCode)Utils.HttpStatusCode_InternalServer,
                        ResponseDescription = Utils.StatusMessage_CBNENairaEndpoint_UserTokenDecryptionError,
                        StatusCode = Utils.StatusCode_CBNENairaEndpoint_TokenDecryptionError,
                        StatusMessage = Utils.StatusMessage_CBNENairaEndpoint_UserTokenDecryptionError
                    };
                }

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} User Token Decryption Successful. Token: {userTypeLoginResponse.ObjectValue?.Token} UserId: {userId}").AppendLine();

                #region Financial Institution (Bank) Get User (Customer/Merchant) Details
                var userDetailsRequest = new UserTypeDetailRequest()
                {
                    UserWalletGuid = userId,
                    UserType = userLoginRequest.UserType,
                    FIAuthToken = fiLoginResponse.ObjectValue.Token
                };

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About to Call FIActionService GetUserTypeDetails Method. Object: {JsonConvert.SerializeObject(userDetailsRequest)}").AppendLine();
                //USER GET ENAIRA WALLET DETAILS
                var userDetailsResponse = await _fIActionService.GetUserTypeDetails(userDetailsRequest);

                //RECONCILE THE LOGS
                logBuilder.ToString().AddToLogs(ref logs);
                logBuilder.Clear();
                userDetailsResponse.Logs.AddToLogs(ref logs);

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Back from FIActionService GetUserTypeDetails Method Call. Response: {JsonConvert.SerializeObject(userDetailsResponse)}").AppendLine();
                #endregion

                if (userDetailsResponse.StatusCode != Utils.StatusCode_Success)
                {
                    //ERROR OCCURED DURING FI GETUSERDETAILS
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} FIActionService GetUserTypeDetails Method Call was not Successful").AppendLine();

                    var controlleReturnResponse = HelperUtility.HandleStatusCodesForControllerReturn<UserLoginResponse>(userDetailsResponse);
                    controlleReturnResponse.StatusCode = Utils.StatusCode_CBNENairaEndpoint_UserDetailFetchFailed;
                    controlleReturnResponse.ResponseDescription = Utils.StatusMessage_CBNENairaEndpoint_UserDetailFetchFailed;

                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    //Task.Run(() => LogWriter.WriteLog(logs));
                    LogWriter.WriteLog(logs);

                    return controlleReturnResponse;
                }

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} FIActionService GetUserTypeDetails Method Call was Successful").AppendLine();

                #region Match ENaira Wallet Account Number to Bank Account Number of Logged In User on Bank Platform
                //CHECK IF ACCOUNT NUMBER TIED TO ENAIRA WALLET EXISTS IN LIST OF ACCOUNTS OF LOGGED IN USER ON THE BANK PLATFORM
                var userENairaWalletAccountNumber = userDetailsResponse.ObjectValue?.AccountNumber?.Trim() ?? "";

                var allUserBankAccounts = JsonConvert.DeserializeObject<List<UserLoginRequest.Account>>(userLoginRequest.AccountNumbers ?? "[]", new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    Error = delegate (object sender, ErrorEventArgs args)
                    {
                        //errors.Add(args.ErrorContext.Error.Message);
                        args.ErrorContext.Handled = true;
                    }
                }) ?? new List<UserLoginRequest.Account>();

                if(!allUserBankAccounts.Any(a => a.Nuban.Equals(userENairaWalletAccountNumber,StringComparison.OrdinalIgnoreCase)))
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} User Bank Accounts does not Contain User ENaira Wallet Account Number. ENairaWalletAccountNumber: {userENairaWalletAccountNumber}").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    //Task.Run(() => LogWriter.WriteLog(logs));
                    LogWriter.WriteLog(logs);

                    return new ControllerReturnResponse<UserLoginResponse>()
                    {
                        ResponseCode = (HttpStatusCode)Utils.HttpStatusCode_BadRequest,
                        ResponseDescription = Utils.StatusMessage_CBNENairaEndpoint_ENairaWalletAccountNumberMissing,
                        StatusCode = Utils.StatusCode_CBNENairaEndpoint_ENairaWalletAccountNumberMissing,
                        StatusMessage = Utils.StatusMessage_CBNENairaEndpoint_ENairaWalletAccountNumberMissing
                    };
                }
                #endregion

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} User Bank Accounts Contain User ENaira Wallet Account Number. ENairaWalletAccountNumber: {userENairaWalletAccountNumber}").AppendLine();
                
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                //Task.Run(() => LogWriter.WriteLog(logs));
                LogWriter.WriteLog(logs);

                //FINALLY RETURN SUCCESS
                var userAccountDetailsResponse = HelperUtility.HandleStatusCodesForControllerReturn<UserLoginResponse>(userTypeLoginResponse);
                userAccountDetailsResponse.ResponseDescription = Utils.StatusMessage_CBNENairaLoginEndpointSuccess;
                userAccountDetailsResponse.ObjectValue = new UserLoginResponse()
                {
                    AccountNumber = userENairaWalletAccountNumber,
                    UserAuthToken = userTypeLoginResponse.ObjectValue?.Token,
                    UserWalletId = userId,
                    UserWalletFullName = userDetailsResponse.ObjectValue?.FullName
                };

                return userAccountDetailsResponse;
            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, "LoginUser Exception");
                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered while Logging User In").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ControllerReturnResponse<UserLoginResponse>()
                {
                    ResponseCode = (HttpStatusCode)Utils.HttpStatusCode_InternalServer,
                    ResponseDescription = Utils.StatusMessage_CBNENairaLoginEndpoint_UserException,
                    StatusCode = Utils.StatusCode_ExceptionError,
                    StatusMessage = Utils.StatusMessage_CBNENairaLoginEndpoint_UserException
                };
            }
        }
    }
}
