using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceService.Core.Helpers.ConfigurationSettings.AppSettings
{
    public class ConnectionStrings
    {
        private string _DecryptionServiceConnectionString { get; set; }
        public string DecryptionServiceConnectionString
        {
            get
            {
                return GTBEncryptLib.DecryptText(_DecryptionServiceConnectionString);
            }
            set
            {
                _DecryptionServiceConnectionString = value;
            }
        }
    }
}
