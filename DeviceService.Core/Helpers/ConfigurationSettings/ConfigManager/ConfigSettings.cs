using DeviceService.Core.Helpers.ConfigurationSettings.AppSettings;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DeviceService.Core.Helpers.ConfigurationSettings.ConfigManager
{
    public static class ConfigSettings
    {
        public static ConnectionStrings ConnectionString => ConfigurationSettingsHelper.GetConfigurationSectionObject<ConnectionStrings>("ConnectionString");
        public static AppSetting AppSetting => ConfigurationSettingsHelper.GetConfigurationSectionObject<AppSetting>("AppSetting");
        public static AES_Encryption_Credentials AES_Encryption_Credentials => ConfigurationSettingsHelper.GetConfigurationSectionObject<AES_Encryption_Credentials>("AES_Encryption_Credentials");
    }
}
