using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceService.Core.Dtos.Global
{
    public class Log
    {
        public string MessageLog { get; set; }
        public int LogType { get; set; }
        public Exception ExceptionLog { get; set; } = null;
    }
}
