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
                return GTBEncryptLib.DecryptText(_DeviceDbConnectionString);
            }
            set
            {
                _DeviceDbConnectionString = value;
            }
        }
    }
}
