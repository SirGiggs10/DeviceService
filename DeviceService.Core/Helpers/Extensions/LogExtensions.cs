using DeviceService.Core.Dtos.Global;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceService.Core.Helpers.Extensions
{
    public static class LogExtensions
    {
        public static void AddToLogs(this List<Log> logs, ref List<Log> mainLogs)
        {
            mainLogs.AddRange(logs);
        }
    }
}
