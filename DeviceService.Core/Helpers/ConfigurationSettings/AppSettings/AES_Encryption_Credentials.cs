using DeviceService.Core.Helpers.Encryption.SimpleBasicEncryption;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceService.Core.Helpers.ConfigurationSettings.AppSettings
{
    public class AES_Encryption_Credentials
    {
        private string _AES_Key { get; set; }
        public string AES_Key
        {
            get
            {
                var decryptedTextObject = SimpleBasicEncryptionUtility.DecryptText(_AES_Key);

                return decryptedTextObject.Item1 ? decryptedTextObject.Item2 : string.Empty;
            }
            set
            {
                _AES_Key = value;
            }
        }

        private string _AES_IV { get; set; }
        public string AES_IV
        {
            get
            {
                var decryptedTextObject = SimpleBasicEncryptionUtility.DecryptText(_AES_IV);

                return decryptedTextObject.Item1 ? decryptedTextObject.Item2 : string.Empty;
            }
            set
            {
                _AES_IV = value;
            }
        }
    }
}
