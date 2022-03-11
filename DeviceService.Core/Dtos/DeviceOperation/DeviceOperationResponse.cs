using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceService.Core.Dtos.DeviceOperation
{
    public class DeviceOperationResponse
    {
        public int DeviceOperationId { get; set; }
        public string DeviceOperationName { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
