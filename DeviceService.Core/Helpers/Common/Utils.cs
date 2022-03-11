using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography;
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
        public const int StatusCode_ForbiddenError = 6;
        public const int StatusCode_Failure = 9;
        public const int StatusCode_DatabaseConnectionTimeout = 10;
        public const int StatusCode_StoredProcedureError = 12;
        public const int StatusCode_ExceptionError = 13;
        public const int StatusCode_DatabaseConnectionError = 14;   
        
        public const int Success = 20;
        public const int NotFound = 22;
        public const int ObjectNull = 23;
        public const int SaveError = 24;
        public const int SaveNoRowAffected = 25;
        public const int NotSucceeded = 26;
        public const int ObjectExists = 27;
        public const int BadRequest = 28;
        public const int SignInError = 29;
        public const int EmailAlreadyConfirmed = 30;
        public const int PreviousPasswordStorageError = 31;
        public const int NewPasswordError = 32;
        public const int InvalidUserType = 33;
        public const int RoleAssignmentError = 34;
        public const int CustomerCreationError = 35;
        public const int InvalidFileSize = 36;
        public const int CloudinaryFileUploadError = 38;
        public const int CloudinaryFileDeleteError = 39;
        public const int CloudinaryDeleteError = 40;
        public const int UserNotAllowed = 41;
        public const int AuditReportError = 42;

        //STATUS MESSAGES
        public const string StatusMessage_Success = "Request Successful.";
        public const string StatusMessage_UnknownError = "Unknown Error Occured while Performing this Action.";
        public const string StatusMessage_TokenNullValue = "Authorization Token Value is Null";
        public const string StatusMessage_BadRequest = "Required request parameter is Invalid / Missing";
        public const string StatusMessage_Unauthorized = "Authentication Token is Unauthorized";
        public const string StatusMessage_PartialContent = "Invalid Response from BVN Service";
        public const string StatusMessage_ForbiddenError = "User Not Authorized";
        public const string StatusMessage_Failure = "Request Failed";
        public const string StatusMessage_DatabaseConnectionTimeout = "Database Connection Timeout";
        public const string StatusMessage_StoredProcedureError = "Stored Procedured Execution Failed";
        public const string StatusMessage_ExceptionError = "An Exception Occured";
        public const string StatusMessage_DatabaseConnectionError = "Database Connection Error";

        public const string StatusMessageSuccess = "Request Successful";
        public const string StatusMessageMailFailure = "Object could not send Mail";
        public const string StatusMessageNotFound = "Object was not Found";
        public const string StatusMessageObjectNull = "Object is Empty";
        public const string StatusMessageSaveError = "Object was unable to Save";
        public const string StatusMessageSaveNoRowAffected = "Object Save Action was not executed";
        public const string StatusMessageNotSucceeded = "Action on this Object did not Succeed";
        public const string StatusMessageObjectExists = "Object already Exists";
        public const string StatusMessageBadRequest = "Bad Request";
        public const string StatusMessageSignInError = "Incorrect Email Address or Password!!!";
        public const string StatusMessageEmailAlreadyConfirmed = "The Email Address has already been Confirmed by Ayuda";
        public const string StatusMessagePreviousPasswordStorageError = "There was Error Storing Previous Password";
        public const string StatusMessageNewPasswordError = "New Password Error";
        public const string StatusMessageInvalidUserType = "The User Type is Invalid";
        public const string StatusMessageRoleAssignmentError = "Unable to Assign Role";
        public const string StatusMessageCustomerCreationError = "Customer Creation Failed";
        public const string StatusMessageInvalidFileSize = "The Size of the Uploaded File is Invalid";
        public const string StatusMessageCloudinaryFileUploadError = "An Error Occured While Uploading the File";
        public const string StatusMessageCloudinaryFileDeleteError = "An Error Occured While Deleting the File";
        public const string StatusMessageCloudinaryDeleteError = "An Error Occured while Deleting files from Cloudinary";
        public const string StatusMessageAuditReportError = "An Error Occured during User Activity Audit";

        //USER TYPES
        public const int UserType_User = 1;

        //USER ROLES
        public const string Role_Administrator = "Administrator";
        public const string Role_NormalUser = "NormalUser";

        //CUSTOM APPLICATION USER CLAIM TYPES
        public const string ClaimType_UserType = "UserType";
        public const string ClaimType_UserEmail = "UserEmail";

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

        //APPLICATION HTTP STATUS CODES
        public const int HttpStatusCode_Ok = StatusCodes.Status200OK;
        public const int HttpStatusCode_BadRequest = StatusCodes.Status400BadRequest;
        public const int HttpStatusCode_Unauthorized = StatusCodes.Status401Unauthorized;
        public const int HttpStatusCode_Forbidden = StatusCodes.Status403Forbidden;
        public const int HttpStatusCode_NotFound = StatusCodes.Status404NotFound;
        public const int HttpStatusCode_InternalServer = StatusCodes.Status500InternalServerError;

        //API HTTP STATUS CODES
        public const int HttpStatusCode_200 = 200;
        public const int HttpStatusCode_400 = 400;
        public const int HttpStatusCode_401 = 401;
        public const int HttpStatusCode_403 = 403;
        public const int HttpStatusCode_404 = 404;
        public const int HttpStatusCode_500 = 500;

        //GET HTTP STATUS CODES DESCRIPTION
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
                case HttpStatusCode_404:
                    return "Resource Object Not Found";
                case HttpStatusCode_500:
                    return "Request processing failed";
                default:
                    //UNREACHABLE PART OF CODE
                    return "Unknown Status Code was Gotten from CBNENaira...Try Again Later.";
            }
        }

        //AES ENCRYPTION
        #region AES ENCRYPTION

        //AES ENCRYPTION MODES
        public const int AES_Mode_CBC = 1;
        public const int AES_Mode_ECB = 2;
        public const int AES_Mode_CTS = 3;

        //AES ENCRYPTION KEYSIZES
        public const int AES_KeySize_128 = 1;
        public const int AES_KeySize_256 = 2;

        //AES ENCRYPTION RETURN TYPE
        public const int AES_ReturnType_Base64 = 1;
        public const int AES_ReturnType_Hex = 2;

        //AES ENCRYPTION DEFAULT VALUES
        public const int AES_Mode_Default = 1;
        public const int AES_KeySize_Default = 1;
        public const int AES_ReturnType_Default = 1;

        public static CipherMode GetCipherMode(int cipherModeCode)
        {
            CipherMode cipherMode;

            switch (cipherModeCode)
            {
                case AES_Mode_CBC:

                    cipherMode = CipherMode.CBC;

                    break;
                case AES_Mode_ECB:

                    cipherMode = CipherMode.ECB;

                    break;

                case AES_Mode_CTS:

                    cipherMode = CipherMode.CTS;

                    break;

                default:

                    cipherMode = CipherMode.CBC;

                    break;
            }

            return cipherMode;
        }

        public static int GetKeySize(int keySizeCode)
        {
            int keySize;

            switch (keySizeCode)
            {
                case AES_KeySize_128:

                    keySize = 128;

                    break;

                case AES_KeySize_256:

                    keySize = 256;

                    break;

                default:

                    keySize = 128;

                    break;
            }

            return keySize;
        }
        #endregion

        //SHA ENCRYPTION ALGORITHMS
        #region SHA Encryption Algorithms
        public const string SHA_Algorithm_256 = "SHA256";
        public const string SHA_Algorithm_512 = "SHA512";
        #endregion

        //DEVICE STATUS
        public enum DeviceStatus
        {
            Available = 1,
            Offline = 2
        }
    }
}
