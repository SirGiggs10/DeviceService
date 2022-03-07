using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DeviceService.Core.Dtos.Global
{
    public class ControllerReturnResponse<T> : APIResponse where T : class
    {
        public T ObjectValue { get; set; }
    }
}
