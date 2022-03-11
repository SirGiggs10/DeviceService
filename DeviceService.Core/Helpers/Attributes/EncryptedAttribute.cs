using DeviceService.Core.Helpers.Common;
using DeviceService.Core.Helpers.ConfigurationSettings.ConfigManager;
using DeviceService.Core.Helpers.Encryption.EncryptionUtility;
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
                string value1 = value is object ? value.ToString() : "";

                if (!string.IsNullOrWhiteSpace(value1))
                {
                    var aesCredentials = ConfigSettings.AES_Encryption_Credentials;
                    decryptedVal = AES.AES_DecryptText(value1, aesCredentials.AES_Key, aesCredentials.AES_IV, Utils.AES_Mode_CBC, Utils.AES_KeySize_128, Utils.AES_ReturnType_Hex);

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
