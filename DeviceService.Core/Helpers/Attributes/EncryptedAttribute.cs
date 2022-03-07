using DeviceService.Core.Helpers.Common;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DeviceService.Core.Helpers.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class EncryptedAttribute : ValidationAttribute
    {
        //Logger nlogger = LogManager.GetCurrentClassLogger();
        // private string Channel;
        public bool Required = true;

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            //INCORPORATED REQUIRED CHECK INTO THE ENCRYPTED ATTRIBUTE
            if(Required)
            {
                if (string.IsNullOrWhiteSpace(value?.ToString()))
                {
                    return new ValidationResult($"The {validationContext.DisplayName} Field is Required");
                }
            }

            string decryptedVal = string.Empty;
            string _Channel = string.Empty;

            try
            {
                try
                {
                    var stringVal = new StringValues();
                    var otherProperty = MyHttpContextAccessor.GetHttpContextAccessor().HttpContext.Request.Headers.TryGetValue("Channel", out stringVal);
                    _Channel = stringVal[0].ToString();
                    /*var otherProperty = validationContext.ObjectType.GetProperty("Channel");  //WAS RETURNING NULL...CHECK WHY...MAYBE .NET 5. WORKS WELL IN .NET CORE 3.1
                    _Channel = otherProperty?.GetValue(validationContext.ObjectInstance, null)?.ToString(); //FIXED THE ERROR BY PASSING THE REQUEST HEADER CLASS AS THE FIRST PARAMETER BEFORE THE BODY OBJECT..THIS FIX DIDNT WORK..CHECK LATER*/
                    
                    if (string.IsNullOrWhiteSpace(_Channel))
                    {
                        throw new Exception();
                    }
                }
                catch (Exception ex)
                {
                    return new ValidationResult("Error Encountered. Could not get Channel.");
                }

                string value1 = value is object ? value.ToString() : "";

                if (!string.IsNullOrWhiteSpace(value1))
                {
                    var decryptionKey = 
                    decryptedVal = EncryptionUtil.RSADecrypt(value.ToString(), _Channel);

                    if (string.IsNullOrEmpty(decryptedVal) && !string.IsNullOrEmpty(value1))
                    {
                        return new ValidationResult($"The {validationContext.DisplayName} Field Must be Encrypted"/*FormatErrorMessage(validationContext.DisplayName)*/);
                    }
                }

                validationContext.ObjectType.GetProperty(validationContext.MemberName).SetValue(validationContext.ObjectInstance, decryptedVal, null);

                return ValidationResult.Success;
            }
            catch (Exception ex)
            {
                //nlogger.Error(ex, $"Could not Decrypt Field:{validationContext.DisplayName}; With Value:{value}");
                return new ValidationResult($"The {validationContext.DisplayName} Field Must be Encrypted"/*FormatErrorMessage(validationContext.DisplayName)*/);
            }          
        }
    }
}
