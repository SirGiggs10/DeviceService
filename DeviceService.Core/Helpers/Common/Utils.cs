using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace DeviceService.Core.Helpers.Common
{
    public class Utils
    {
        //STATUS CODES
        public const int StatusCode_Success = 0;
        public const int StatusCode_UnknownError = 1;
        public const int StatusCode_TokenNullValue = 2;
        public const int StatusCode_BadRequest = 3;
        public const int StatusCode_Unauthorized = 4;
        public const int StatusCode_PartialContent = 5;
        public const int StatusCode_Failure = 9;
        public const int StatusCode_DatabaseConnectionTimeout = 10;
        public const int StatusCode_StoredProcedureError = 12;
        public const int StatusCode_ExceptionError = 13;
        public const int StatusCode_DatabaseConnectionError = 14;
        public const int StatusCode_InvalidUserType = 15;
        public const int StatusCode_InvalidTransactionType = 16;

        //CBNENaira ENDPOINT STATUS CODES
        public const int StatusCode_CBNENairaEndpointSuccess = 0;
        public const int StatusCode_CBNENairaEndpointDatabaseConnectionTimeout = 97;  //NOT USED
        public const int StatusCode_CBNENairaEndpointUnknownError = 1;
        public const int StatusCode_CBNENairaEndpointBadRequest = 2;
        public const int StatusCode_CBNENairaEndpointDatabaseConnectionError = 96;
        public const int StatusCode_CBNENairaEndpoint_FILoginFailed = 11;
        public const int StatusCode_CBNENairaEndpoint_UserLoginFailed = 12;
        public const int StatusCode_CBNENairaEndpoint_UserDetailFetchFailed = 13;
        public const int StatusCode_CBNENairaEndpoint_TokenDecryptionError = 14;
        public const int StatusCode_CBNENairaEndpoint_ENairaWalletAccountNumberMissing = 15;
        public const int StatusCode_CBNENairaEndpoint_LogTransactionFailed = 16;
        public const int StatusCode_CBNENairaEndpoint_UserAuthorizeWithdrawalFailed = 17;
        public const int StatusCode_CBNENairaEndpoint_EmployeeDetailFetchFailed = 18;
        public const int StatusCode_CBNENairaEndpoint_BranchDetailFetchFailed = 19;
        public const int StatusCode_CBNENairaEndpoint_FISubmitTransactionTypeFailed = 20;
        public const int StatusCode_CBNENairaEndpoint_PostTransferError = 21;
        public const int StatusCode_CBNENairaEndpoint_UpdateTransactionLogFailed = 22;
        public const int StatusCode_CBNENairaEndpoint_SubmitTransactionException = 23;

        //STATUS MESSAGES
        public const string StatusMessage_Success = "Request Successful.";
        public const string StatusMessage_UnknownError = "Unknown Error Occured while Performing this Action.";
        public const string StatusMessage_TokenNullValue = "Authorization Token Value is Null";
        public const string StatusMessage_BadRequest = "Required request parameter is Invalid / Missing";
        public const string StatusMessage_Unauthorized = "Authentication Token is Unauthorized";
        public const string StatusMessage_PartialContent = "Invalid Response from POS Terminal Monitoring System";
        public const string StatusMessage_Failure = "Request Failed";
        public const string StatusMessage_DatabaseConnectionTimeout = "Database Connection Timeout";
        public const string StatusMessage_StoredProcedureError = "Stored Procedured Execution Failed";
        public const string StatusMessage_ExceptionError = "An Exception Occured";
        public const string StatusMessage_DatabaseConnectionError = "Database Connection Error";
        public const string StatusMessage_InvalidUserType = "Invalid User Type";
        public const string StatusMessage_InvalidTransactionType = "Invalid Transaction Type";

        //CBNENaira ENDPOINT STATUS MESSAGES
        public const string StatusMessage_CBNENairaLoginEndpointSuccess = "Login Successful. User Account Details Fetched";
        public const string StatusMessage_CBNENairaSubmitTransactionEndpointSuccess = "Transaction Submitted Successfully";
        public const string StatusMessage_CBNENairaEndpointDatabaseConnectionTimeout = "Database Connection Timeout Occured";
        public const string StatusMessage_CBNENairaEndpointLoginUnknownError = "Unable to Login User";
        public const string StatusMessage_CBNENairaEndpointDatabaseConnectionError = "System Malfunction";
        public const string StatusMessage_CBNENairaEndpoint_FILoginFailed = "FI Login Failed";
        public const string StatusMessage_CBNENairaEndpoint_UserLoginFailed = "User Login Failed";
        public const string StatusMessage_CBNENairaEndpoint_UserDetailFetchFailed = "Unable to Fetch User Details";
        public const string StatusMessage_CBNENairaEndpoint_FITokenDecryptionError = "Error Occured Decrypting FI Auth Token";
        public const string StatusMessage_CBNENairaEndpoint_UserTokenDecryptionError = "Error Occured Decrypting User Auth Token";
        public const string StatusMessage_CBNENairaEndpoint_ENairaWalletAccountNumberMissing = "User ENairaWalletAccountNumber Not Existing in BankAccounts";
        public const string StatusMessage_CBNENairaLoginEndpoint_UserException = "Login Failed. User Account Details Not Fetched";
        public const string StatusMessage_CBNENairaLoginEndpoint_TransactionException = "Submit Transaction Request Failed";
        public const string StatusMessage_CBNENairaEndpoint_LogTransactionFailed = "Unable to Log this Transaction";
        public const string StatusMessage_CBNENairaEndpoint_UserAuthorizeWithdrawalFailed = "Unable to Authorize User Withdrawal";
        public const string StatusMessage_CBNENairaEndpoint_EmployeeDetailFetchFailed = "Unable to Fetch FI Employee Details";
        public const string StatusMessage_CBNENairaEndpoint_BranchDetailFetchFailed = "Unable to Fetch FI Branch Details";
        public const string StatusMessage_CBNENairaEndpoint_FISubmitTransactionTypeFailed = "Unable to Process Transaction. Try Again.";
        public const string StatusMessage_CBNENairaEndpoint_PostTransferError = "Transfer Posting Failed";
        public const string StatusMessage_CBNENairaEndpoint_UpdateTransactionLogFailed = "Unable to Update Transaction Log";
        public const string StatusMessage_CBNENairaEndpoint_PostTransferNubanNotValidError = "Unable to Convert Nuban Account Number To Regular";
        public const string StatusMessage_CBNENairaEndpoint_SubmitTransactionInsufficientBalance = "Insufficient ENaira Balance.";

        //LOG TYPE
        public enum LogType
        {
            /// <summary>
            /// Log Message in Debug Level
            /// </summary>
            [Description("Log Message in Debug Level")]
            LOG_DEBUG = 1,
            /// <summary>
            /// Log Message in Information Level
            /// </summary>
            [Description("Log Message in Information Level")]
            LOG_INFORMATION = 2,
            /// <summary>
            /// Log Message in Error Level
            /// </summary>
            [Description("Log Message in Error Level")]
            LOG_ERROR = 3
        }

        //GRAPHQL COMMAND TYPES
        public enum GraphQLCommandType
        {
            /// <summary>
            /// MUTATION Command Type
            /// </summary>
            [Description("MUTATION Command Type")]
            MUTATION = 1,
            /// <summary>
            /// QUERY Command Type
            /// </summary>
            [Description("QUERY Command Type")]
            QUERY = 2,
        }

        //APPLICATION HTTP STATUS CODES
        public const int HttpStatusCode_Ok = StatusCodes.Status200OK;
        public const int HttpStatusCode_BadRequest = StatusCodes.Status400BadRequest;
        public const int HttpStatusCode_InternalServer = StatusCodes.Status500InternalServerError;

        //CBNENaira RESPONSE CODES
        public const string ResponseCode_Success = "00";

        //GET CBNENaira RESPONSE DESCRIPTIONS
        public static string GetResponseDescription(string responseCode)
        {
            switch (responseCode)
            {
                case ResponseCode_Success:
                    return "Approved or completed successfully";
                
                default:
                    //UNREACHABLE PART OF CODE
                    return "Unknown Response Code was Gotten from CBNENaira...Try Again Later.";
            }
        }

        //CBNENaira API HTTP STATUS CODES
        public const int HttpStatusCode_200 = 200;
        public const int HttpStatusCode_400 = 400;
        public const int HttpStatusCode_401 = 401;
        public const int HttpStatusCode_403 = 403;
        public const int HttpStatusCode_500 = 500;

        //GET CBNENaira HTTP STATUS CODES DESCRIPTION
        public static string GetHttpStatusDescription(int httpStatusCode)
        {
            switch (httpStatusCode)
            {
                case HttpStatusCode_200:
                    return "Request Successful";
                case HttpStatusCode_400:
                    return "Required request parameter or path variable is missing";
                case HttpStatusCode_401:
                    return "Missing Authorization request header or access token is invalid";
                case HttpStatusCode_403:
                    return "Missing/Invalid Authorization Token";
                case HttpStatusCode_500:
                    return "Request processing failed";
                default:
                    //UNREACHABLE PART OF CODE
                    return "Unknown Status Code was Gotten from CBNENaira...Try Again Later.";
            }
        }

        //CBNENaira USERTYPES
        public enum UserType
        {
            Customer = 1,
            Merchant = 2
        }

        //USER LOGIN TOKEN CLAIM TYPES
        public const string ClaimType_UserId = "userid";

        //CBNENaira TRANSACTION TYPES
        public enum TransactionType
        {
            Deposit = 1,
            Withdrawal = 2
        }

        //CBNENaira AMOUNT TYPES
        public enum AmountType
        {
            ENaira = 1,
            Naira = 2
        }

        //CBNENaira Transaction States
        public enum ENairaTransactionCurrentState
        {
            Settling = 1
        }

        //CBNENaira Destination Fund Type
        public enum DestinationFundType
        {
            BANK_ACCOUNT = 1,
            CASH = 2
        }

        //BASIS RESPONSE CODES AND DESCRIPTION
        public static string GetBasisResponseDescription(string responseCode)
        {
            var respDesc = string.Empty;

            switch(responseCode)
            {
                case "@ERR1@":

                    respDesc = "Invalid Source Account";

                    break;
                case "@ERR2@":

                    respDesc = "Source Account has restrictions";

                    break;
                case "@ERR3@":

                    respDesc = "Source Account has restrictions";

                    break;
                case "@ERR4@":

                    respDesc = "Target Account has restrictions";

                    break;
                case "@ERR5@":

                    respDesc = "Invalid Amount";

                    break;
                case "@ERR6@":

                    respDesc = "Unknown Error: Transaction Unsuccessful!";

                    break;
                case "@ERR7@":

                    respDesc = "Operation Successful!";

                    break;
                case "@ERR8@":

                    respDesc = "Unknown Error: Transaction Unsuccessful!";

                    break;
                case "@ERR9@":

                    respDesc = "Transfer cannot be Executed";

                    break;
                case "@ERR10@":

                    respDesc = "Invalid Account";

                    break;
                case "@ERR11@":

                    respDesc = "Invalid Password";

                    break;
                case "@ERR12@":

                    respDesc = "Invalid Branch";

                    break;
                case "@ERR13@":

                    respDesc = "Invalid Check Number!";

                    break;
                case "@ERR14@":

                    respDesc = "Cheque Cashed";

                    break;
                case "@ERR15@":

                    respDesc = "Invalid Sub Account";

                    break;
                case "@ERR16@":

                    respDesc = "Invalid Customer number";

                    break;
                case "@ERR17@":

                    respDesc = "Customer has no PBS Account";

                    break;
                case "@ERR18@":

                    respDesc = "Request Already Exists";

                    break;
                case "@ERR19@":

                    respDesc = "Request accepted for further processing";

                    break;
                case "@ERR20@":

                    respDesc = "Accounts involved do not belong to the same branch";

                    break;
                case "@ERR21@":

                    respDesc = "Accounts involved do not belong to the same customer";

                    break;
                case "@ERR22@":

                    respDesc = "Accounts involved are not of the same currency";

                    break;
                case "@ERR23@":

                    respDesc = "Transfer amount exceeds accounts allowed transfer limit";

                    break;
                case "@ERR24@":

                    respDesc = "Transfer Amount exceeds Accounts available balance";

                    break;
                case "@ERR25@":

                    respDesc = "One of the Branches Involved is not on the network";

                    break;
                case "@ERR26@":

                    respDesc = "No Data Retrieved";

                    break;
                case "@ERR27@":

                    respDesc = "Not a checking Account";

                    break;
                case "@ERR28@":

                    respDesc = "Source Account is Dormant";

                    break;
                case "@ERR29@":

                    respDesc = "Target Account is Dormant";

                    break;
                case "@ERR30@":

                    respDesc = "Source Account is Closed";

                    break;
                case "@ERR31@":

                    respDesc = "Target Account is closed";

                    break;
                case "@ERR32@":

                    respDesc = "External Transactions are not allowed on Source Account";

                    break;
                case "@ERR33@":

                    respDesc = "External Transactions are not allowed on Target Account";

                    break;
                case "@ERR-58@":

                    respDesc = "Either the Source or the Target Account Has Restriction";

                    break;
                case "@ERR5000@":

                    respDesc = "@ERR5000@";

                    break;
                case "@ERR844@":

                    respDesc = "Cheque is not within customer series";

                    break;
                case "@ERR845@":

                    respDesc = "Cheque Has Been Processed Before";

                    break;
                default:

                    respDesc = "Error occurred: Transaction Unsuccessful!";

                    break;
            }

            return respDesc;
        }
    }
}
