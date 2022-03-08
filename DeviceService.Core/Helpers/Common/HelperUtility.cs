using DeviceService.Core.Dtos.Global;
using DeviceService.Core.Helpers.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DeviceService.Core.Helpers.Common
{
    public static class HelperUtility
    {
        public static string GetRemoteIPAddress()
        {
            return MyHttpContextAccessor.GetHttpContextAccessor().HttpContext?.Features?.Get<IHttpConnectionFeature>()?.RemoteIpAddress?.ToString();
        }

        public static ControllerReturnResponse<T> HandleStatusCodesForControllerReturn<T>(APIResponse apiResponse) where T : class
        {
            //ASSIGN THE NECESSARY VALUES TO THE ENDPOINT RETURN RESPONSE
            //CHECK THE RETURN STATUS CODES AND ASSIGN THE APPROPRIATE ENDPOINT RETURN STATUS CODE AND MESSAGE
            var controllerReturnResponse = new ControllerReturnResponse<T>();

            switch (apiResponse.StatusCode)
            {
                case Utils.StatusCode_Success:

                    controllerReturnResponse.ResponseCode = (HttpStatusCode)Utils.HttpStatusCode_Ok;
                    //controllerReturnResponse.ResponseDescription = Utils.StatusMessage_CBNENairaLoginEndpointSuccess;
                    controllerReturnResponse.StatusCode = Utils.StatusCode_CBNENairaEndpointSuccess;
                    controllerReturnResponse.StatusMessage = apiResponse.ResponseDescription;

                    break;
                case Utils.StatusCode_UnknownError:

                    controllerReturnResponse.ResponseCode = (HttpStatusCode)Utils.HttpStatusCode_InternalServer;
                    controllerReturnResponse.StatusMessage = apiResponse.ResponseDescription;

                    break;
                case Utils.StatusCode_ExceptionError:

                    controllerReturnResponse.ResponseCode = (HttpStatusCode)Utils.HttpStatusCode_InternalServer;
                    controllerReturnResponse.StatusMessage = apiResponse.ResponseDescription;

                    break;
                case Utils.StatusCode_DatabaseConnectionError:

                    controllerReturnResponse.ResponseCode = (HttpStatusCode)Utils.HttpStatusCode_BadRequest;
                    controllerReturnResponse.StatusMessage = apiResponse.ResponseDescription;

                    break;
                case Utils.StatusCode_BadRequest:

                    controllerReturnResponse.ResponseCode = (HttpStatusCode)Utils.HttpStatusCode_BadRequest;
                    controllerReturnResponse.StatusMessage = apiResponse.ResponseDescription;

                    break;
                default:

                    controllerReturnResponse.ResponseCode = (HttpStatusCode)Utils.HttpStatusCode_BadRequest;
                    controllerReturnResponse.StatusMessage = apiResponse.ResponseDescription;

                    break;
            }

            return controllerReturnResponse;
        }

        public static string PadAmount(string amount)
        {
            var amt = amount + "";
            for (int i = amt.Length; i < 10; i++)
            {
                amt = "0" + amt;
            }
            return amt;
        }

        private static string ConvertTens(string MyTens)
        {
            //Try
            string Result = string.Empty;

            // Is value between 10 and 19?
            if (MyTens.Substring(0, 1).Equals("1"))
            {
                switch ((MyTens))
                {
                    case "10":
                        Result = "Ten";
                        break;
                    case "11":
                        Result = "Eleven";
                        break;
                    case "12":
                        Result = "Twelve";
                        break;
                    case "13":
                        Result = "Thirteen";
                        break;
                    case "14":
                        Result = "Fourteen";
                        break;
                    case "15":
                        Result = "Fifteen";
                        break;
                    case "16":
                        Result = "Sixteen";
                        break;
                    case "17":
                        Result = "Seventeen";
                        break;
                    case "18":
                        Result = "Eighteen";
                        break;
                    case "19":
                        Result = "Nineteen";
                        break;
                    default:
                        break;
                }
            }
            else
            {
                // .. otherwise it's between 20 and 99.
                switch (MyTens.Substring(0, 1))
                {
                    case "2":
                        Result = "Twenty ";
                        break;
                    case "3":
                        Result = "Thirty ";
                        break;
                    case "4":
                        Result = "Forty ";
                        break;
                    case "5":
                        Result = "Fifty ";
                        break;
                    case "6":
                        Result = "Sixty ";
                        break;
                    case "7":
                        Result = "Seventy ";
                        break;
                    case "8":
                        Result = "Eighty ";
                        break;
                    case "9":
                        Result = "Ninety ";
                        break;
                    default:
                        break;
                }

                // Convert ones place digit.
                Result = Result + ConvertDigit(MyTens.Substring(1, 1));
            }
            return Result;
            //Catch conv As Exception
            //    MsgBox(conv.Message)
            //End Try
        }

        private static string ConvertDigit(string MyDigit)
        {
            string functionReturnValue = null;
            //Try
            switch (MyDigit)
            {
                case "1":
                    functionReturnValue = "One";
                    break;
                case "2":
                    functionReturnValue = "Two";
                    break;
                case "3":
                    functionReturnValue = "Three";
                    break;
                case "4":
                    functionReturnValue = "Four";
                    break;
                case "5":
                    functionReturnValue = "Five";
                    break;
                case "6":
                    functionReturnValue = "Six";
                    break;
                case "7":
                    functionReturnValue = "Seven";
                    break;
                case "8":
                    functionReturnValue = "Eight";
                    break;
                case "9":
                    functionReturnValue = "Nine";
                    break;
                default:
                    functionReturnValue = string.Empty;
                    break;
            }

            return functionReturnValue;
        }

        public static string StringBytesToHex(byte[] stringBytes, bool stripHyphen)
        {
            var hexStringVal = BitConverter.ToString(stringBytes);

            return stripHyphen ? StripHyphenFromHexString(hexStringVal) : hexStringVal;
        }

        private static string StripHyphenFromHexString(string hexString)
        {
            return hexString.Replace("-", "");
        }

        public static byte[] HexToStringBytes(string hexStringVal)
        {
            var stringBytes = hexStringVal.ToBytes();

            return stringBytes;
        }

        public static string StringBytesToBase64(byte[] stringBytes)
        {
            var base64StringVal = Convert.ToBase64String(stringBytes);

            return base64StringVal;
        }

        public static byte[] Base64ToStringBytes(string base64String)
        {
            var stringBytes = Convert.FromBase64String(base64String);

            return stringBytes;
        }
    }
}
