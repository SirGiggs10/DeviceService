using AutoMapper;
using CBN_eNaira.Core.Dtos.Global;
using CBN_eNaira.Core.Dtos.Transaction;
using CBN_eNaira.Core.Dtos.Transaction.Other;
using CBN_eNaira.Core.Dtos.TransferService;
using CBN_eNaira.Core.Dtos.User.Other;
using CBN_eNaira.Core.Helpers.Common;
using CBN_eNaira.Core.Helpers.Common.JWT;
using CBN_eNaira.Core.Helpers.ConfigurationSettings.ConfigManager;
using CBN_eNaira.Core.Helpers.Extensions;
using CBN_eNaira.Core.Helpers.Logging.Logger;
using CBN_eNaira.Core.Interfaces.Repositories;
using CBN_eNaira.Core.Interfaces.Services;
using CBN_eNaira.Core.Models;
using CBN_eNaira.Core.Services.Other;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static CBN_eNaira.Core.Helpers.Common.Utils;

namespace CBN_eNaira.Core.Services.Controller
{
    public class TransactionService : ITransactionService
    {
        private string className = string.Empty;

        private readonly ILogTransactionRepository _logTransactionRepository;
        private readonly IFIActionService _fIActionService;
        private readonly IUserTypeService _userTypeService;
        private readonly ITransferServiceWrapper _transferServiceWrapper;
        private readonly IMapper _mapper;

        public TransactionService(ILogTransactionRepository logTransactionRepository, IFIActionService fIActionService, IUserTypeService userTypeService, ITransferServiceWrapper transferServiceWrapper, IMapper mapper)
        {
            className = GetType().Name;

            _logTransactionRepository = logTransactionRepository;
            _fIActionService = fIActionService;
            _userTypeService = userTypeService;
            _transferServiceWrapper = transferServiceWrapper;
            _mapper = mapper;
        }

        public async Task<ControllerReturnResponse<SubmitTransactionResponse>> SubmitTransaction(SubmitTransactionRequest submitTransactionRequest, RequestHeader requestHeader)
        {
            string methodName = "SubmitTransaction"/*MethodBase.GetCurrentMethod().Name*/, classAndMethodName = $"{className}.{methodName}"; //MethodBase.GetCurrentMethod().Name returns MoveNext for async methods because async methods are actually called inside a wrapper method MoveNext

            var logs = new List<Log>();
            ReturnResponse<ReturnResponse<TransactionLogUpdate_Response>> updateTransactionLogResponse = null;

            //TRANSACTION LOG UPDATE REQUEST
            var transactionUpdateRequest = new TransactionUpdateRequest()
            {
                Id = 0
            };

            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Endpoint Request for Submitting Transaction. Payload: {JsonConvert.SerializeObject(submitTransactionRequest)}, RequestHeader: {JsonConvert.SerializeObject(requestHeader)}").AppendLine();

            try
            {
                //GENERATE UNIQUE REFERENCE FOR THE ENAIRA TRANSACTION
                var gTBENairaTransactionReferenceId = RandomNumberGenerator.GenerateUniqueReference();

                #region Log Transaction to the Database
                var logTransactionRequest = new Transaction()
                {
                    GTBENairaTransactionReferenceId = gTBENairaTransactionReferenceId,
                    Amount = submitTransactionRequest.Amount,
                    AmountType = (submitTransactionRequest.TransType == Utils.TransactionType.Deposit) ? Utils.AmountType.Naira : ((submitTransactionRequest.TransType == Utils.TransactionType.Withdrawal) ? Utils.AmountType.ENaira : throw new Exception()),
                    CreatedAt = DateTimeOffset.Now,
                    Posted = false,
                    TransactionSubmitted = false,
                    TransactionType = submitTransactionRequest.TransType,
                    UserAuthorized = false,
                    UserENairaWalletId = submitTransactionRequest.WalletId,
                    UserType = submitTransactionRequest.UserType,
                    UserId = submitTransactionRequest.UserId,
                    NubanAccountNumber = submitTransactionRequest.AccountNumber,
                    Channel = requestHeader.Channel
                };

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About to Call LogTransaction Repository InsertTransactionLog Method. Object: {JsonConvert.SerializeObject(logTransactionRequest)}").AppendLine();
                //LOG TRANSACTION TO THE DB
                var insertTransactionResponse = await _logTransactionRepository.InsertTransactionLog(logTransactionRequest);

                //RECONCILE THE LOGS
                logBuilder.ToString().AddToLogs(ref logs);
                logBuilder.Clear();
                insertTransactionResponse.Logs.AddToLogs(ref logs);

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Back from LogTransaction Repository InsertTransactionLog Method Call. Response: {JsonConvert.SerializeObject(insertTransactionResponse)}").AppendLine();
                #endregion

                //UPDATE

                if (insertTransactionResponse.StatusCode != Utils.StatusCode_Success)
                {
                    //ERROR OCCURED DURING TRANSACTION LOGGING TO DB
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} LogTransaction Repository InsertTransactionLog Method Call was not Successful").AppendLine();

                    var controlleReturnResponse = HelperUtility.HandleStatusCodesForControllerReturn<SubmitTransactionResponse>(insertTransactionResponse);
                    controlleReturnResponse.StatusCode = Utils.StatusCode_CBNENairaEndpoint_LogTransactionFailed;
                    controlleReturnResponse.ResponseDescription = Utils.StatusMessage_CBNENairaEndpoint_LogTransactionFailed;

                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    //TODO: THIS MAKES THE LOGGING TO BE SCATTERED. SOMETIMES RESPONSE IS LOGGED BEFORE THIS LOGGING COMPLETES. CHECK THE BETTER WAY TO FIRE AND FORGET OR MAKE THE LOGGING TO BE SYNCHRONOUS INSTEAD OF FIRE AND FORGET
                    //FINALLY WRITE THE LIST OF LOGS TO SINK(FILE)...FIRE THIS METHOD IN A SEPARATE THREAD AND FORGET IT. BECAUSE OF SPEED...
                    //Task.Run(() => LogWriter.WriteLog(logs));
                    LogWriter.WriteLog(logs);

                    return controlleReturnResponse;
                }

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} LogTransaction Repository InsertTransactionLog Method Call was Successful").AppendLine();

                transactionUpdateRequest.Id = Convert.ToInt32(insertTransactionResponse.ObjectValue?._Code ?? "0");

                if (submitTransactionRequest.TransType == Utils.TransactionType.Withdrawal)
                {
                    #region User Authorize Withdrawal
                    var userAuthorizeWithdrawalRequest = new AuthorizeUserTypeWithdrawalRequest()
                    {
                        AuthorizeWithdrawal = true,
                        UserAuthToken = submitTransactionRequest.TokenCode,
                        UserType = submitTransactionRequest.UserType
                    };

                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About to Call UserTypeService AuthorizeUserTypeWithdrawal Method. Object: {JsonConvert.SerializeObject(userAuthorizeWithdrawalRequest)}").AppendLine();
                    //AUTHORIZE USER WITHDRAWAL
                    var userAuthorizeWithdrawalResponse = await _userTypeService.AuthorizeUserTypeWithdrawal(userAuthorizeWithdrawalRequest);

                    //RECONCILE THE LOGS
                    logBuilder.ToString().AddToLogs(ref logs);
                    logBuilder.Clear();
                    userAuthorizeWithdrawalResponse.Logs.AddToLogs(ref logs);

                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Back from UserTypeService AuthorizeUserTypeWithdrawal Method Call. Response: {JsonConvert.SerializeObject(userAuthorizeWithdrawalResponse)}").AppendLine();
                    #endregion

                    transactionUpdateRequest.UserAuthorizedAt = DateTimeOffset.Now;
                    transactionUpdateRequest.UserAuthorized = (userAuthorizeWithdrawalResponse.StatusCode == Utils.StatusCode_Success) ? true : false;    //TODO: CHECK THE STATE PROPERTY OF THE CBNENAIRA AUTHORIZE ENDPOINT RESPONSE
                    transactionUpdateRequest.UserAuthorizationStatusCode = $"{userAuthorizeWithdrawalResponse.ResponseCode.ToString()} | {userAuthorizeWithdrawalResponse.StatusCode.ToString()}";
                    transactionUpdateRequest.ModifiedAt = DateTimeOffset.Now;

                    if (userAuthorizeWithdrawalResponse.StatusCode != Utils.StatusCode_Success)
                    {
                        //ERROR OCCURED DURING USER WITHDRAWAL AUTHORIZATION
                        logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} UserTypeService AuthorizeUserTypeWithdrawal Method Call was not Successful").AppendLine();

                        var controlleReturnResponse = HelperUtility.HandleStatusCodesForControllerReturn<SubmitTransactionResponse>(userAuthorizeWithdrawalResponse);
                        controlleReturnResponse.StatusCode = Utils.StatusCode_CBNENairaEndpoint_UserAuthorizeWithdrawalFailed;
                        controlleReturnResponse.ResponseDescription = Utils.StatusMessage_CBNENairaEndpoint_UserAuthorizeWithdrawalFailed;

                        transactionUpdateRequest.TransactionAPIResponse = JsonConvert.SerializeObject(controlleReturnResponse, new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });

                        #region Update Transaction Log on the DB
                        //UPDATE TRANSACTION LOG ON THE DB
                        updateTransactionLogResponse = await UpdateTransactionLogOnDb(transactionUpdateRequest, methodName);

                        //RECONCILE THE LOG
                        logBuilder.ToString().AddToLogs(ref logs);
                        logBuilder.Clear();
                        updateTransactionLogResponse.Logs.AddToLogs(ref logs);

                        if (updateTransactionLogResponse.StatusCode != Utils.StatusCode_Success)
                        {
                            //UPDATE TRANSACTION LOG NOT SUCCESSFUL...ERROR OCCURED. RETURN
                            //Task.Run(() => LogWriter.WriteLog(logs));
                            LogWriter.WriteLog(logs);

                            return new ControllerReturnResponse<SubmitTransactionResponse>()
                            {
                                ResponseCode = updateTransactionLogResponse.ResponseCode,
                                ResponseDescription = updateTransactionLogResponse.ResponseDescription,
                                StatusCode = updateTransactionLogResponse.StatusCode,
                                StatusMessage = updateTransactionLogResponse.StatusMessage
                            };
                        }
                        #endregion

                        logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                        logBuilder.ToString().AddToLogs(ref logs);

                        //Task.Run(() => LogWriter.WriteLog(logs));
                        LogWriter.WriteLog(logs);

                        return controlleReturnResponse;
                    }

                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} UserTypeService AuthorizeUserTypeWithdrawal Method Call was Successful").AppendLine();
                }

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
                    //ERROR OCCURED DURING FI LOGIN
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} FIActionService LoginBank Method Call was not Successful").AppendLine();

                    var controlleReturnResponse = HelperUtility.HandleStatusCodesForControllerReturn<SubmitTransactionResponse>(fiLoginResponse);
                    controlleReturnResponse.StatusCode = Utils.StatusCode_CBNENairaEndpoint_FILoginFailed;
                    controlleReturnResponse.ResponseDescription = Utils.StatusMessage_CBNENairaEndpoint_FILoginFailed;

                    transactionUpdateRequest.TransactionAPIResponse = JsonConvert.SerializeObject(controlleReturnResponse, new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });

                    #region Update Transaction Log on the DB
                    //UPDATE TRANSACTION LOG ON THE DB
                    updateTransactionLogResponse = await UpdateTransactionLogOnDb(transactionUpdateRequest, methodName);

                    //RECONCILE THE LOG
                    logBuilder.ToString().AddToLogs(ref logs);
                    logBuilder.Clear();
                    updateTransactionLogResponse.Logs.AddToLogs(ref logs);

                    if (updateTransactionLogResponse.StatusCode != Utils.StatusCode_Success)
                    {
                        //UPDATE TRANSACTION LOG NOT SUCCESSFUL...ERROR OCCURED. RETURN
                        //Task.Run(() => LogWriter.WriteLog(logs));
                        LogWriter.WriteLog(logs);

                        return new ControllerReturnResponse<SubmitTransactionResponse>()
                        {
                            ResponseCode = updateTransactionLogResponse.ResponseCode,
                            ResponseDescription = updateTransactionLogResponse.ResponseDescription,
                            StatusCode = updateTransactionLogResponse.StatusCode,
                            StatusMessage = updateTransactionLogResponse.StatusMessage
                        };
                    }
                    #endregion

                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    //Task.Run(() => LogWriter.WriteLog(logs));
                    LogWriter.WriteLog(logs);

                    return controlleReturnResponse;
                }

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} FIActionService LoginBank Method Call was Successful").AppendLine();

                #region FI Auth Token Decryption
                //DECRYPT FI AUTH TOKEN
                var fIUserId = JWTHelper.JwtDecoderGetClaimValue(fiLoginResponse.ObjectValue?.Token, Utils.ClaimType_UserId);
                #endregion

                if (fIUserId == null)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} FI Auth Token Decryption Failed. Token: {fiLoginResponse.ObjectValue?.Token}").AppendLine();

                    var controlleReturnResponse = new ControllerReturnResponse<SubmitTransactionResponse>()
                    {
                        ResponseCode = (HttpStatusCode)Utils.HttpStatusCode_InternalServer,
                        ResponseDescription = Utils.StatusMessage_CBNENairaEndpoint_FITokenDecryptionError,
                        StatusCode = Utils.StatusCode_CBNENairaEndpoint_TokenDecryptionError,
                        StatusMessage = Utils.StatusMessage_CBNENairaEndpoint_FITokenDecryptionError
                    };

                    transactionUpdateRequest.TransactionAPIResponse = JsonConvert.SerializeObject(controlleReturnResponse, new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });

                    #region Update Transaction Log on the DB
                    //UPDATE TRANSACTION LOG ON THE DB
                    updateTransactionLogResponse = await UpdateTransactionLogOnDb(transactionUpdateRequest, methodName);

                    //RECONCILE THE LOG
                    logBuilder.ToString().AddToLogs(ref logs);
                    logBuilder.Clear();
                    updateTransactionLogResponse.Logs.AddToLogs(ref logs);

                    if (updateTransactionLogResponse.StatusCode != Utils.StatusCode_Success)
                    {
                        //UPDATE TRANSACTION LOG NOT SUCCESSFUL...ERROR OCCURED. RETURN
                        //Task.Run(() => LogWriter.WriteLog(logs));
                        LogWriter.WriteLog(logs);

                        return new ControllerReturnResponse<SubmitTransactionResponse>()
                        {
                            ResponseCode = updateTransactionLogResponse.ResponseCode,
                            ResponseDescription = updateTransactionLogResponse.ResponseDescription,
                            StatusCode = updateTransactionLogResponse.StatusCode,
                            StatusMessage = updateTransactionLogResponse.StatusMessage
                        };
                    }
                    #endregion

                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    //Task.Run(() => LogWriter.WriteLog(logs));
                    LogWriter.WriteLog(logs);

                    return controlleReturnResponse;
                }

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} FI Token Decryption Successful. Token: {fiLoginResponse.ObjectValue?.Token} UserId: {fIUserId}").AppendLine();

                #region Financial Institution (Bank) Get Employee (Teller) Details
                var employeeDetailsRequest = new EmployeeDetailRequest()
                {
                    FIUserGuid = fIUserId,
                    FIAuthToken = fiLoginResponse.ObjectValue?.Token
                };

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About to Call FIActionService GetEmployeeDetails Method. Object: {JsonConvert.SerializeObject(employeeDetailsRequest)}").AppendLine();
                //FI GET EMPLOYEE DETAILS
                var employeeDetailsResponse = await _fIActionService.GetEmployeeDetails(employeeDetailsRequest);

                //RECONCILE THE LOGS
                logBuilder.ToString().AddToLogs(ref logs);
                logBuilder.Clear();
                employeeDetailsResponse.Logs.AddToLogs(ref logs);

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Back from FIActionService GetEmployeeDetails Method Call. Response: {JsonConvert.SerializeObject(employeeDetailsResponse)}").AppendLine();
                #endregion

                if (employeeDetailsResponse.StatusCode != Utils.StatusCode_Success)
                {
                    //ERROR OCCURED DURING FI GETEMPLOYEEDETAILS
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} FIActionService GetEmployeeDetails Method Call was not Successful").AppendLine();

                    var controlleReturnResponse = HelperUtility.HandleStatusCodesForControllerReturn<SubmitTransactionResponse>(employeeDetailsResponse);
                    controlleReturnResponse.StatusCode = Utils.StatusCode_CBNENairaEndpoint_EmployeeDetailFetchFailed;
                    controlleReturnResponse.ResponseDescription = Utils.StatusMessage_CBNENairaEndpoint_EmployeeDetailFetchFailed;

                    transactionUpdateRequest.TransactionAPIResponse = JsonConvert.SerializeObject(controlleReturnResponse, new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });

                    #region Update Transaction Log on the DB
                    //UPDATE TRANSACTION LOG ON THE DB
                    updateTransactionLogResponse = await UpdateTransactionLogOnDb(transactionUpdateRequest, methodName);

                    //RECONCILE THE LOG
                    logBuilder.ToString().AddToLogs(ref logs);
                    logBuilder.Clear();
                    updateTransactionLogResponse.Logs.AddToLogs(ref logs);

                    if (updateTransactionLogResponse.StatusCode != Utils.StatusCode_Success)
                    {
                        //UPDATE TRANSACTION LOG NOT SUCCESSFUL...ERROR OCCURED. RETURN
                        //Task.Run(() => LogWriter.WriteLog(logs));
                        LogWriter.WriteLog(logs);

                        return new ControllerReturnResponse<SubmitTransactionResponse>()
                        {
                            ResponseCode = updateTransactionLogResponse.ResponseCode,
                            ResponseDescription = updateTransactionLogResponse.ResponseDescription,
                            StatusCode = updateTransactionLogResponse.StatusCode,
                            StatusMessage = updateTransactionLogResponse.StatusMessage
                        };
                    }
                    #endregion

                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    //Task.Run(() => LogWriter.WriteLog(logs));
                    LogWriter.WriteLog(logs);

                    return controlleReturnResponse;
                }

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} FIActionService GetEmployeeDetails Method Call was Successful").AppendLine();

                #region Financial Institution (Bank) Get Branch Details
                var branchDetailsRequest = new BranchDetailRequest()
                {
                    BranchGuid = employeeDetailsResponse.ObjectValue?.BranchGuid,
                    FIAuthToken = fiLoginResponse.ObjectValue?.Token
                };

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About to Call FIActionService GetBranchDetails Method. Object: {JsonConvert.SerializeObject(branchDetailsRequest)}").AppendLine();
                //FI GET BRANCH DETAILS
                var branchDetailsResponse = await _fIActionService.GetBranchDetails(branchDetailsRequest);

                //RECONCILE THE LOGS
                logBuilder.ToString().AddToLogs(ref logs);
                logBuilder.Clear();
                branchDetailsResponse.Logs.AddToLogs(ref logs);

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Back from FIActionService GetBranchDetails Method Call. Response: {JsonConvert.SerializeObject(branchDetailsResponse)}").AppendLine();
                #endregion

                if (branchDetailsResponse.StatusCode != Utils.StatusCode_Success)
                {
                    //ERROR OCCURED DURING FI GETBRANCHDETAILS
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} FIActionService GetBranchDetails Method Call was not Successful").AppendLine();

                    var controlleReturnResponse = HelperUtility.HandleStatusCodesForControllerReturn<SubmitTransactionResponse>(branchDetailsResponse);
                    controlleReturnResponse.StatusCode = Utils.StatusCode_CBNENairaEndpoint_BranchDetailFetchFailed;
                    controlleReturnResponse.ResponseDescription = Utils.StatusMessage_CBNENairaEndpoint_BranchDetailFetchFailed;

                    transactionUpdateRequest.TransactionAPIResponse = JsonConvert.SerializeObject(controlleReturnResponse, new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });

                    #region Update Transaction Log on the DB
                    //UPDATE TRANSACTION LOG ON THE DB
                    updateTransactionLogResponse = await UpdateTransactionLogOnDb(transactionUpdateRequest, methodName);

                    //RECONCILE THE LOG
                    logBuilder.ToString().AddToLogs(ref logs);
                    logBuilder.Clear();
                    updateTransactionLogResponse.Logs.AddToLogs(ref logs);

                    if (updateTransactionLogResponse.StatusCode != Utils.StatusCode_Success)
                    {
                        //UPDATE TRANSACTION LOG NOT SUCCESSFUL...ERROR OCCURED. RETURN
                        //Task.Run(() => LogWriter.WriteLog(logs));
                        LogWriter.WriteLog(logs);

                        return new ControllerReturnResponse<SubmitTransactionResponse>()
                        {
                            ResponseCode = updateTransactionLogResponse.ResponseCode,
                            ResponseDescription = updateTransactionLogResponse.ResponseDescription,
                            StatusCode = updateTransactionLogResponse.StatusCode,
                            StatusMessage = updateTransactionLogResponse.StatusMessage
                        };
                    }
                    #endregion

                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    //Task.Run(() => LogWriter.WriteLog(logs));
                    LogWriter.WriteLog(logs);

                    return controlleReturnResponse;
                }

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} FIActionService GetBranchDetails Method Call was Successful").AppendLine();

                logBuilder.ToString().AddToLogs(ref logs);
                logBuilder.Clear();

                #region Transfer (Post to Basis) First OR Submit Transaction First?
                ReturnResponse<SubmitTransactionTypeResponse> submitTransactionTypeResp = null;
                ReturnResponse<PostTransactionResponse> postTransactionTypeResp = null;

                if (submitTransactionRequest.TransType == Utils.TransactionType.Deposit)
                {
                    //TRANSFER (POST TO BASIS) FROM USER NAIRA ACCOUNT TO BRANCH NAIRA ACCOUNT FIRST BEFORE SUBMITTING DEPOSIT REQUEST
                    //TRANSFER(POST TO BASIS) FROM USER NAIRA ACCOUNT TO BRANCH NAIRA ACCOUNT
                    var singleTransferResponse = await TransferFundsBetweenAccounts(submitTransactionRequest, gTBENairaTransactionReferenceId, requestHeader.Channel, methodName);
                    postTransactionTypeResp = singleTransferResponse.ObjectValue;
                    var postTransResp = postTransactionTypeResp?.ObjectValue;

                    //RECONCILE THE LOG
                    singleTransferResponse.Logs.AddToLogs(ref logs);

                    transactionUpdateRequest.PostedAt = DateTimeOffset.Now;
                    transactionUpdateRequest.Posted = ((postTransResp?.isSuccessful) ?? false) ? true : false;
                    transactionUpdateRequest.PostingStatusCode = $"{postTransResp?.ApiResponseCode ?? string.Empty} | {postTransResp?.TransferServiceResponse?.responseCode ?? string.Empty}";
                    transactionUpdateRequest.BasisTransactionReferenceId = postTransResp?.TransferServiceResponse?.uniqueBasisRef ?? string.Empty;
                    transactionUpdateRequest.ModifiedAt = DateTimeOffset.Now;
                    transactionUpdateRequest.RegularAccountNumber = singleTransferResponse?.ObjectValue?.ObjectValue?.TransferServiceResponse?.accountToDeBit;

                    if (singleTransferResponse.StatusCode != Utils.StatusCode_Success)
                    {
                        //TRANSFER NOT SUCCESSFUL...ERROR OCCURED. RETURN
                        var controlleReturnResponse = new ControllerReturnResponse<SubmitTransactionResponse>()
                        {
                            ResponseCode = singleTransferResponse.ResponseCode,
                            ResponseDescription = singleTransferResponse.ResponseDescription,
                            StatusCode = singleTransferResponse.StatusCode,
                            StatusMessage = singleTransferResponse.StatusMessage
                        };

                        transactionUpdateRequest.TransactionAPIResponse = JsonConvert.SerializeObject(controlleReturnResponse, new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });

                        #region Update Transaction Log on the DB
                        //UPDATE TRANSACTION LOG ON THE DB
                        updateTransactionLogResponse = await UpdateTransactionLogOnDb(transactionUpdateRequest, methodName);

                        //RECONCILE THE LOG
                        updateTransactionLogResponse.Logs.AddToLogs(ref logs);

                        if (updateTransactionLogResponse.StatusCode != Utils.StatusCode_Success)
                        {
                            //UPDATE TRANSACTION LOG NOT SUCCESSFUL...ERROR OCCURED. RETURN
                            //Task.Run(() => LogWriter.WriteLog(logs));
                            LogWriter.WriteLog(logs);

                            return new ControllerReturnResponse<SubmitTransactionResponse>()
                            {
                                ResponseCode = updateTransactionLogResponse.ResponseCode,
                                ResponseDescription = updateTransactionLogResponse.ResponseDescription,
                                StatusCode = updateTransactionLogResponse.StatusCode,
                                StatusMessage = updateTransactionLogResponse.StatusMessage
                            };
                        }
                        #endregion

                        //Task.Run(() => LogWriter.WriteLog(logs));
                        LogWriter.WriteLog(logs);

                        return controlleReturnResponse;
                    }

                    //TRANSFER SUCCESSFUL...SUBMIT TRANSACTION REQUEST
                    //SUBMIT TRANSACTION (WITHDRAWAL / DEPOSIT) REQUEST FOR USER (CUSTOMER / MERCHANT)
                    var singleSubmitTransactionResponse = await SubmitTransactionTypeForUserType(submitTransactionRequest, gTBENairaTransactionReferenceId, branchDetailsResponse.ObjectValue?.BranchWalletGuid, fiLoginResponse.ObjectValue?.Token, methodName);
                    submitTransactionTypeResp = singleSubmitTransactionResponse.ObjectValue;
                    var submitTransTypeResp = submitTransactionTypeResp?.ObjectValue;

                    //RECONCILE THE LOG
                    singleSubmitTransactionResponse.Logs.AddToLogs(ref logs);

                    transactionUpdateRequest.CurrencyCode = submitTransTypeResp?.CurrencyCode ?? string.Empty;
                    transactionUpdateRequest.ENairaTransactionCurrentState = submitTransTypeResp?.ENairaTransactionCurrentState ?? string.Empty;
                    transactionUpdateRequest.ENairaTransactionId = submitTransTypeResp?.ENairaTransactionId ?? string.Empty;
                    transactionUpdateRequest.TransactionSubmitted = (submitTransactionTypeResp?.StatusCode == Utils.StatusCode_Success) ? true : false;
                    transactionUpdateRequest.TransactionSubmissionStatusCode = $"{submitTransactionTypeResp?.ResponseCode.ToString()} | {submitTransactionTypeResp?.StatusCode.ToString()}";
                    transactionUpdateRequest.TransactionSubmittedAt = DateTimeOffset.Now;
                    transactionUpdateRequest.ModifiedAt = DateTimeOffset.Now;
                   
                    if (singleSubmitTransactionResponse.StatusCode != Utils.StatusCode_Success)
                    {
                        //TRANSACTION SUBMISSION NOT SUCCESSFUL...ERROR OCCURED. RETURN

                        var controlleReturnResponse = new ControllerReturnResponse<SubmitTransactionResponse>()
                        {
                            ResponseCode = singleSubmitTransactionResponse.ResponseCode,
                            ResponseDescription = singleSubmitTransactionResponse.ResponseDescription,
                            StatusCode = singleSubmitTransactionResponse.StatusCode,
                            StatusMessage = singleSubmitTransactionResponse.StatusMessage
                        };

                        transactionUpdateRequest.TransactionAPIResponse = JsonConvert.SerializeObject(controlleReturnResponse, new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });

                        #region Update Transaction Log on the DB
                        //UPDATE TRANSACTION LOG ON THE DB
                        updateTransactionLogResponse = await UpdateTransactionLogOnDb(transactionUpdateRequest, methodName);

                        //RECONCILE THE LOG
                        updateTransactionLogResponse.Logs.AddToLogs(ref logs);

                        if (updateTransactionLogResponse.StatusCode != Utils.StatusCode_Success)
                        {
                            //UPDATE TRANSACTION LOG NOT SUCCESSFUL...ERROR OCCURED. RETURN
                            //Task.Run(() => LogWriter.WriteLog(logs));
                            LogWriter.WriteLog(logs);

                            return new ControllerReturnResponse<SubmitTransactionResponse>()
                            {
                                ResponseCode = updateTransactionLogResponse.ResponseCode,
                                ResponseDescription = updateTransactionLogResponse.ResponseDescription,
                                StatusCode = updateTransactionLogResponse.StatusCode,
                                StatusMessage = updateTransactionLogResponse.StatusMessage
                            };
                        }
                        #endregion

                        //Task.Run(() => LogWriter.WriteLog(logs));
                        LogWriter.WriteLog(logs);

                        return controlleReturnResponse;
                    }
                }
                else if(submitTransactionRequest.TransType == Utils.TransactionType.Withdrawal)
                {
                    //SUBMIT WITHDRAWAL REQUEST FIRST BEFORE TRANSFER (POST TO BASIS) FROM BRANCH NAIRA ACCOUNT TO USER NAIRA ACCOUNT
                    //SUBMIT TRANSACTION (WITHDRAWAL / DEPOSIT) REQUEST FOR USER (CUSTOMER / MERCHANT)
                    var singleSubmitTransactionResponse = await SubmitTransactionTypeForUserType(submitTransactionRequest, gTBENairaTransactionReferenceId, branchDetailsResponse.ObjectValue?.BranchWalletGuid, fiLoginResponse.ObjectValue?.Token, methodName);
                    submitTransactionTypeResp = singleSubmitTransactionResponse.ObjectValue;
                    var submitTransTypeResp = submitTransactionTypeResp?.ObjectValue;

                    //RECONCILE THE LOG
                    singleSubmitTransactionResponse.Logs.AddToLogs(ref logs);

                    transactionUpdateRequest.CurrencyCode = submitTransTypeResp?.CurrencyCode ?? string.Empty;
                    transactionUpdateRequest.ENairaTransactionCurrentState = submitTransTypeResp?.ENairaTransactionCurrentState ?? string.Empty;
                    transactionUpdateRequest.ENairaTransactionId = submitTransTypeResp?.ENairaTransactionId ?? string.Empty;
                    transactionUpdateRequest.TransactionSubmitted = (submitTransactionTypeResp?.StatusCode == Utils.StatusCode_Success) ? true : false;
                    transactionUpdateRequest.TransactionSubmissionStatusCode = $"{submitTransactionTypeResp?.ResponseCode.ToString()} | {submitTransactionTypeResp?.StatusCode.ToString()}";
                    transactionUpdateRequest.TransactionSubmittedAt = DateTimeOffset.Now;
                    transactionUpdateRequest.ModifiedAt = DateTimeOffset.Now;

                    if (singleSubmitTransactionResponse.StatusCode != Utils.StatusCode_Success)
                    {
                        //TRANSACTION SUBMISSION NOT SUCCESSFUL...ERROR OCCURED. RETURN

                        var controlleReturnResponse = new ControllerReturnResponse<SubmitTransactionResponse>()
                        {
                            ResponseCode = singleSubmitTransactionResponse.ResponseCode,
                            ResponseDescription = singleSubmitTransactionResponse.ResponseDescription,
                            StatusCode = singleSubmitTransactionResponse.StatusCode,
                            StatusMessage = singleSubmitTransactionResponse.StatusMessage
                        };

                        transactionUpdateRequest.TransactionAPIResponse = JsonConvert.SerializeObject(controlleReturnResponse, new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });

                        #region Update Transaction Log on the DB
                        //UPDATE TRANSACTION LOG ON THE DB
                        updateTransactionLogResponse = await UpdateTransactionLogOnDb(transactionUpdateRequest, methodName);

                        //RECONCILE THE LOG
                        logBuilder.ToString().AddToLogs(ref logs);
                        logBuilder.Clear();
                        updateTransactionLogResponse.Logs.AddToLogs(ref logs);

                        if (updateTransactionLogResponse.StatusCode != Utils.StatusCode_Success)
                        {
                            //UPDATE TRANSACTION LOG NOT SUCCESSFUL...ERROR OCCURED. RETURN
                            //Task.Run(() => LogWriter.WriteLog(logs));
                            LogWriter.WriteLog(logs);

                            return new ControllerReturnResponse<SubmitTransactionResponse>()
                            {
                                ResponseCode = updateTransactionLogResponse.ResponseCode,
                                ResponseDescription = updateTransactionLogResponse.ResponseDescription,
                                StatusCode = updateTransactionLogResponse.StatusCode,
                                StatusMessage = updateTransactionLogResponse.StatusMessage
                            };
                        }
                        #endregion

                        //Task.Run(() => LogWriter.WriteLog(logs));
                        LogWriter.WriteLog(logs);
                      
                        return controlleReturnResponse;
                    }

                    //TRANSFER(POST TO BASIS) FROM USER NAIRA ACCOUNT TO BRANCH NAIRA ACCOUNT
                    var singleTransferResponse = await TransferFundsBetweenAccounts(submitTransactionRequest, gTBENairaTransactionReferenceId, requestHeader.Channel, methodName);
                    postTransactionTypeResp = singleTransferResponse.ObjectValue;
                    var postTransResp = postTransactionTypeResp?.ObjectValue;

                    //RECONCILE THE LOG
                    singleTransferResponse.Logs.AddToLogs(ref logs);

                    transactionUpdateRequest.PostedAt = DateTimeOffset.Now;
                    transactionUpdateRequest.Posted = ((postTransResp?.isSuccessful).Value == true) ? true : false;
                    transactionUpdateRequest.PostingStatusCode = $"{postTransResp?.ApiResponseCode} | {postTransResp?.TransferServiceResponse?.responseCode ?? string.Empty}";
                    transactionUpdateRequest.BasisTransactionReferenceId = postTransResp?.TransferServiceResponse?.uniqueBasisRef ?? string.Empty;
                    transactionUpdateRequest.ModifiedAt = DateTimeOffset.Now;
                    transactionUpdateRequest.RegularAccountNumber = singleTransferResponse?.ObjectValue?.ObjectValue?.TransferServiceResponse?.accountToCredit;
                    if (singleTransferResponse.StatusCode != Utils.StatusCode_Success)
                    {
                        //TRANSFER NOT SUCCESSFUL...ERROR OCCURED. RETURN

                        var controlleReturnResponse = new ControllerReturnResponse<SubmitTransactionResponse>()
                        {
                            ResponseCode = singleTransferResponse.ResponseCode,
                            ResponseDescription = singleTransferResponse.ResponseDescription,
                            StatusCode = singleTransferResponse.StatusCode,
                            StatusMessage = singleTransferResponse.StatusMessage
                        };

                        transactionUpdateRequest.TransactionAPIResponse = JsonConvert.SerializeObject(controlleReturnResponse, new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });

                        #region Update Transaction Log on the DB
                        //UPDATE TRANSACTION LOG ON THE DB
                        updateTransactionLogResponse = await UpdateTransactionLogOnDb(transactionUpdateRequest, methodName);

                        //RECONCILE THE LOG
                        updateTransactionLogResponse.Logs.AddToLogs(ref logs);

                        if (updateTransactionLogResponse.StatusCode != Utils.StatusCode_Success)
                        {
                            //UPDATE TRANSACTION LOG NOT SUCCESSFUL...ERROR OCCURED. RETURN
                            //Task.Run(() => LogWriter.WriteLog(logs));
                            LogWriter.WriteLog(logs);

                            return new ControllerReturnResponse<SubmitTransactionResponse>()
                            {
                                ResponseCode = updateTransactionLogResponse.ResponseCode,
                                ResponseDescription = updateTransactionLogResponse.ResponseDescription,
                                StatusCode = updateTransactionLogResponse.StatusCode,
                                StatusMessage = updateTransactionLogResponse.StatusMessage
                            };
                        }
                        #endregion

                        //Task.Run(() => LogWriter.WriteLog(logs));
                        LogWriter.WriteLog(logs);

                        return controlleReturnResponse;
                    }
                }
                else
                {
                    throw new Exception();
                }
                #endregion

                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                //PERFORM FINAL TRANSACTION LOG UPDATE ON THE DB
                transactionUpdateRequest.ModifiedAt = DateTimeOffset.Now;

                var submitTransactionResponse = HelperUtility.HandleStatusCodesForControllerReturn<SubmitTransactionResponse>(submitTransactionTypeResp);
                submitTransactionResponse.ResponseDescription = Utils.StatusMessage_CBNENairaSubmitTransactionEndpointSuccess;

                transactionUpdateRequest.TransactionAPIResponse = JsonConvert.SerializeObject(submitTransactionResponse, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                });

                #region Update Transaction Log on the DB
                //UPDATE TRANSACTION LOG ON THE DB
                updateTransactionLogResponse = await UpdateTransactionLogOnDb(transactionUpdateRequest, methodName);

                //RECONCILE THE LOG
                updateTransactionLogResponse.Logs.AddToLogs(ref logs);

                if (updateTransactionLogResponse.StatusCode != Utils.StatusCode_Success)
                {
                    //UPDATE TRANSACTION LOG NOT SUCCESSFUL...ERROR OCCURED. RETURN
                    //Task.Run(() => LogWriter.WriteLog(logs));
                    LogWriter.WriteLog(logs);

                    return new ControllerReturnResponse<SubmitTransactionResponse>()
                    {
                        ResponseCode = updateTransactionLogResponse.ResponseCode,
                        ResponseDescription = updateTransactionLogResponse.ResponseDescription,
                        StatusCode = updateTransactionLogResponse.StatusCode,
                        StatusMessage = updateTransactionLogResponse.StatusMessage
                    };
                }
                #endregion

                //Task.Run(() => LogWriter.WriteLog(logs));
                LogWriter.WriteLog(logs);

                //FINALLY RETURN SUCCESS
                var transferDetailsResp = postTransactionTypeResp?.ObjectValue?.TransferServiceResponse;
                var submitTransDetailsResp = submitTransactionTypeResp?.ObjectValue;
                
                submitTransactionResponse.ObjectValue = new SubmitTransactionResponse()
                {
                    Amount = transferDetailsResp?.traAmount.ToString(),
                    AmountInWords = transferDetailsResp?.amountInWords,
                    BasisTransactionReferenceId = transferDetailsResp?.uniqueBasisRef,
                    CurrencyCode = submitTransDetailsResp?.CurrencyCode,
                    GTBENairaTransactionReferenceId = gTBENairaTransactionReferenceId,
                    ENairaTransactionCurrentState = submitTransDetailsResp?.ENairaTransactionCurrentState,
                    ENairaTransactionId = submitTransDetailsResp?.ENairaTransactionId,
                    UserId = submitTransactionRequest?.UserId,
                    PurposeAndRemarks = $"{transferDetailsResp?.remarks}"    //CHECK LATER AND MERGE REMARK AND PURPOSE
                };

                return submitTransactionResponse;
            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, $"{methodName} Exception");
                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered while Submitting Transaction For User").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                var controlleReturnResponse = new ControllerReturnResponse<SubmitTransactionResponse>()
                {
                    ResponseCode = (HttpStatusCode)Utils.HttpStatusCode_InternalServer,
                    ResponseDescription = Utils.StatusMessage_CBNENairaLoginEndpoint_TransactionException,
                    StatusCode = Utils.StatusCode_CBNENairaEndpoint_SubmitTransactionException,
                    StatusMessage = Utils.StatusMessage_CBNENairaLoginEndpoint_TransactionException
                };

                transactionUpdateRequest.TransactionAPIResponse = JsonConvert.SerializeObject(controlleReturnResponse, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                });

                #region Update Transaction Log on the DB
                //UPDATE TRANSACTION LOG ON THE DB
                updateTransactionLogResponse = await UpdateTransactionLogOnDb(transactionUpdateRequest, methodName);

                //RECONCILE THE LOG
                updateTransactionLogResponse.Logs.AddToLogs(ref logs);

                if (updateTransactionLogResponse.StatusCode != Utils.StatusCode_Success)
                {
                    //UPDATE TRANSACTION LOG NOT SUCCESSFUL...ERROR OCCURED. RETURN
                    //Task.Run(() => LogWriter.WriteLog(logs));
                    LogWriter.WriteLog(logs);

                    return new ControllerReturnResponse<SubmitTransactionResponse>()
                    {
                        ResponseCode = updateTransactionLogResponse.ResponseCode,
                        ResponseDescription = updateTransactionLogResponse.ResponseDescription,
                        StatusCode = updateTransactionLogResponse.StatusCode,
                        StatusMessage = updateTransactionLogResponse.StatusMessage
                    };
                }
                #endregion

                //Task.Run(() => LogWriter.WriteLog(logs));
                LogWriter.WriteLog(logs);

                return controlleReturnResponse;
            }
        }

        private async Task<ReturnResponse<ReturnResponse<PostTransactionResponse>>> TransferFundsBetweenAccounts(SubmitTransactionRequest submitTransactionRequest, string gTBENairaTransactionReferenceId, string channel, string classMethName)
        {
            var logBuilder = new StringBuilder();
            var logs = new List<Log>();

            var returnResponse = new ReturnResponse<ReturnResponse<PostTransactionResponse>>();

            #region Intra-Bank Transfer (Post To Basis)

            var postRequest = new PostTransactionRequest()
            {
                Amount = (Convert.ToDouble(submitTransactionRequest.Amount) * ConfigSettings.AppSetting.CBNENairaToNairaValue).ToString(),
                Channel = channel,
                TransactionId = gTBENairaTransactionReferenceId,
                RequestId = $"{gTBENairaTransactionReferenceId}",
                Remark = submitTransactionRequest.Remarks ?? string.Empty
            };

            var regularAcctNumResp = await BasisService.ConvertNubanAccountToRegularAccount(submitTransactionRequest.AccountNumber);

            //RECONCILE THE LOGS
            logBuilder.ToString().AddToLogs(ref logs);
            logBuilder.Clear();
            regularAcctNumResp.Logs.AddToLogs(ref logs);

            if (regularAcctNumResp.StatusCode != Utils.StatusCode_Success)
            {
                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Nuban AccountNumber Not Valid. AccountNumber: {submitTransactionRequest.AccountNumber}").AppendLine();
                logBuilder.AppendLine($"--------------{classMethName}--------END--------").AppendLine();

                logBuilder.ToString().AddToLogs(ref logs);

                returnResponse.ResponseCode = (HttpStatusCode)Utils.HttpStatusCode_InternalServer;
                returnResponse.ResponseDescription = $"{Utils.StatusMessage_CBNENairaEndpoint_PostTransferError} | {Utils.StatusMessage_CBNENairaEndpoint_PostTransferNubanNotValidError}";
                returnResponse.StatusCode = Utils.StatusCode_CBNENairaEndpoint_PostTransferError;
                returnResponse.StatusMessage = Utils.StatusMessage_CBNENairaEndpoint_PostTransferError;
                returnResponse.Logs = logs;

                return returnResponse;
            }

            if (submitTransactionRequest.TransType == TransactionType.Deposit)
            {
                postRequest.AccountToCredit = ConfigSettings.FIBranchDetail.BranchENairaWalletAccountNumber;
                postRequest.AccountToDebit = regularAcctNumResp.ObjectValue;
                postRequest.Purpose = $"{gTBENairaTransactionReferenceId} Transfer of {submitTransactionRequest.Amount} from {submitTransactionRequest.AccountNumber} to Branch ENaira Wallet Account";
            }
            else if (submitTransactionRequest.TransType == TransactionType.Withdrawal)
            {
                postRequest.AccountToCredit = regularAcctNumResp.ObjectValue;
                postRequest.AccountToDebit = ConfigSettings.FIBranchDetail.BranchENairaWalletAccountNumber;
                postRequest.Purpose = $"{gTBENairaTransactionReferenceId} Transfer of {submitTransactionRequest.Amount} from Branch ENaira Wallet {submitTransactionRequest.WalletId} Account to {submitTransactionRequest.AccountNumber}";
                postRequest.AuthMode = ConfigSettings.FIBranchDetail.AuthMode;
                postRequest.AuthValue = ConfigSettings.FIBranchDetail.AuthPin;
                postRequest.SubChannel = ConfigSettings.FIBranchDetail.Channel;
                postRequest.UserId = ConfigSettings.FIBranchDetail.UserId;               
            }
            else
            {
                throw new Exception();
            }

            var accountSplitted = postRequest.AccountToDebit.Split('/');
            postRequest.UserId = $"{accountSplitted[0]}{accountSplitted[1]}01";

            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About to Call TransferServiceWrapper PostTransaction Method. Object: {JsonConvert.SerializeObject(postRequest)}").AppendLine();
            //TRANSFER FUNDS BETWEEN BRANCH AND USER BANK ACCOUNTS
            var transferResponse = await _transferServiceWrapper.PostTransaction(postRequest);

            //RECONCILE THE LOGS
            logBuilder.ToString().AddToLogs(ref logs);
            logBuilder.Clear();
            transferResponse.ObjectValue?.LogEntry?.AddToLogs(ref logs);

            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Back from TransferServiceWrapper PostTransaction Method Call. Response: {JsonConvert.SerializeObject(transferResponse)}").AppendLine();
            #endregion

            if ((transferResponse.ObjectValue?.isSuccessful).HasValue && !transferResponse.ObjectValue.isSuccessful)
            {
                //ERROR OCCURED DURING FUNDS TRANSFER
                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} TransferServiceWrapper PostTransaction Method Call was not Successful").AppendLine();

                returnResponse.ResponseCode = (HttpStatusCode)Utils.HttpStatusCode_InternalServer;
                //returnResponse.ResponseDescription = Utils.StatusMessage_CBNENairaEndpoint_PostTransferError;
                returnResponse.ResponseDescription = transferResponse.ObjectValue?.ApiResponseMessage ?? "Error Occured while Posting Transaction";
                returnResponse.StatusCode = Utils.StatusCode_CBNENairaEndpoint_PostTransferError;
                returnResponse.StatusMessage = Utils.StatusMessage_CBNENairaEndpoint_PostTransferError;

                logBuilder.AppendLine($"--------------{classMethName}--------END--------").AppendLine();
            }
            else
            {
                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} TransferServiceWrapper PostTransaction Method Call was Successful").AppendLine();

                returnResponse.StatusCode = Utils.StatusCode_Success;
            }

            logBuilder.ToString().AddToLogs(ref logs);

            returnResponse.ObjectValue = transferResponse;
            returnResponse.Logs = logs;

            return returnResponse;
        }

        private async Task<ReturnResponse<ReturnResponse<SubmitTransactionTypeResponse>>> SubmitTransactionTypeForUserType(SubmitTransactionRequest submitTransactionRequest, string gTBENairaTransactionReferenceId, string branchWalletGuid, string fIAuthToken, string classMethName)
        {
            var logBuilder = new StringBuilder();
            var logs = new List<Log>();

            var returnResponse = new ReturnResponse<ReturnResponse<SubmitTransactionTypeResponse>>();

            #region Financial Institution (Bank) Submit Transaction Type (Deposit / Withdrawal) Request
            var submitTransactionTypeRequest = new SubmitTransactionTypeRequest()
            {
                Amount = submitTransactionRequest.Amount,
                BranchWalletId = branchWalletGuid,
                UserWalletId = submitTransactionRequest.WalletId,
                DestinationFundType = (submitTransactionRequest.TransType == Utils.TransactionType.Deposit) ? Utils.DestinationFundType.CASH : ((submitTransactionRequest.TransType == Utils.TransactionType.Withdrawal) ? Utils.DestinationFundType.BANK_ACCOUNT : throw new Exception()),
                ReferenceId = gTBENairaTransactionReferenceId,
                FIAuthToken = fIAuthToken,
                TransactionType = submitTransactionRequest.TransType
            };

            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About to Call FIActionService SubmitTransactionType Method. Object: {JsonConvert.SerializeObject(submitTransactionTypeRequest)}").AppendLine();
            //FI SUBMIT TRANSACTION TYPE (DEPOSIT / WITHDRAWAL)
            var submitTransactionTypeResponse = await _fIActionService.SubmitTransactionType(submitTransactionTypeRequest);

            //RECONCILE THE LOGS
            logBuilder.ToString().AddToLogs(ref logs);
            logBuilder.Clear();
            submitTransactionTypeResponse.Logs.AddToLogs(ref logs);

            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Back from FIActionService SubmitTransactionType Method Call. Response: {JsonConvert.SerializeObject(submitTransactionTypeResponse)}").AppendLine();
            #endregion

            if (submitTransactionTypeResponse.StatusCode != Utils.StatusCode_Success)
            {
                //ERROR OCCURED DURING FI SUBMIT TRANSACTION
                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} FIActionService SubmitTransactionType Method Call was not Successful").AppendLine();

                var controlleReturnResponse = HelperUtility.HandleStatusCodesForControllerReturn<SubmitTransactionResponse>(submitTransactionTypeResponse);
                returnResponse.StatusCode = Utils.StatusCode_CBNENairaEndpoint_FISubmitTransactionTypeFailed;
                returnResponse.StatusMessage = controlleReturnResponse.StatusMessage;
                returnResponse.ResponseCode = controlleReturnResponse.ResponseCode;

                if(returnResponse.StatusMessage.ToLower().Contains("insufficient"))
                {
                    returnResponse.ResponseDescription = (submitTransactionRequest.TransType == TransactionType.Withdrawal) ? Utils.StatusMessage_CBNENairaEndpoint_SubmitTransactionInsufficientBalance : Utils.StatusMessage_CBNENairaEndpoint_FISubmitTransactionTypeFailed;
                }
                else
                {
                    returnResponse.ResponseDescription = returnResponse.StatusMessage;
                }               

                logBuilder.AppendLine($"--------------{classMethName}--------END--------").AppendLine();
            }
            else
            {
                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} FIActionService SubmitTransactionType Method Call was Successful").AppendLine();

                returnResponse.StatusCode = Utils.StatusCode_Success;
            }
           
            logBuilder.ToString().AddToLogs(ref logs);

            returnResponse.ObjectValue = submitTransactionTypeResponse;
            returnResponse.Logs = logs;

            return returnResponse;
        }

        private async Task<ReturnResponse<ReturnResponse<TransactionLogUpdate_Response>>> UpdateTransactionLogOnDb(TransactionUpdateRequest transactionUpdateRequest, string classMethName)
        {     
            var logBuilder = new StringBuilder();
            var logs = new List<Log>();

            var returnResponse = new ReturnResponse<ReturnResponse<TransactionLogUpdate_Response>>();

            #region Update Transaction Log on the DB
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About to Call LogTransactionRepository UpdateTransactionLog Method. Object: {JsonConvert.SerializeObject(transactionUpdateRequest)}").AppendLine();
            //UPDATE TRANSACTION LOG ON DB
            var transactionUpdateResponse = await _logTransactionRepository.UpdateTransactionLog(transactionUpdateRequest);

            //RECONCILE THE LOGS
            logBuilder.ToString().AddToLogs(ref logs);
            logBuilder.Clear();
            transactionUpdateResponse.Logs.AddToLogs(ref logs);

            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Back from LogTransactionRepository UpdateTransactionLog Method Call. Response: {JsonConvert.SerializeObject(transactionUpdateResponse)}").AppendLine();

            if (transactionUpdateResponse.StatusCode != Utils.StatusCode_Success)
            {
                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} LogTransactionRepository UpdateTransactionLog Method Call was not Successful").AppendLine();

                var controlleReturnResp = HelperUtility.HandleStatusCodesForControllerReturn<SubmitTransactionResponse>(transactionUpdateResponse);
                returnResponse.StatusCode = Utils.StatusCode_CBNENairaEndpoint_UpdateTransactionLogFailed;
                returnResponse.StatusMessage = controlleReturnResp.StatusMessage;
                returnResponse.ResponseCode = controlleReturnResp.ResponseCode;
                returnResponse.ResponseDescription = Utils.StatusMessage_CBNENairaEndpoint_UpdateTransactionLogFailed;

                logBuilder.AppendLine($"--------------{classMethName}--------END--------").AppendLine();
            }
            else
            {
                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} LogTransactionRepository UpdateTransactionLog Method Call was Successful").AppendLine();
               
                returnResponse.StatusCode = Utils.StatusCode_Success;
            }           
            #endregion

            logBuilder.ToString().AddToLogs(ref logs);

            returnResponse.ObjectValue = transactionUpdateResponse;
            returnResponse.Logs = logs;

            return returnResponse;
        }
    }
}
