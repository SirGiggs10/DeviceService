using DeviceService.Core.Helpers.Encryption.SimpleBasicEncryption;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceService.Core.Helpers.ConfigurationSettings.AppSettings
{
    public class ConnectionStrings
    {
        private string _DeviceDbConnectionString { get; set; }
        public string DeviceDbConnectionString
        {
            get
            {
                var decryptedTextObject = SimpleBasicEncryptionUtility.DecryptText(_DeviceDbConnectionString);

                return decryptedTextObject.Item1 ? decryptedTextObject.Item2 : string.Empty;
            }
            set
            {
                _DeviceDbConnectionString = value;
            }
        }
    }
}
