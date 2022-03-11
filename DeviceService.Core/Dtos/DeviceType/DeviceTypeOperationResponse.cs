using DeviceService.Core.Dtos.DeviceOperation;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceService.Core.Dtos.DeviceType
{
    public class DeviceTypeOperationResponse
    {
        public int DeviceTypeId { get; set; }
        public int DeviceOperationId { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

        public DeviceOperationResponse DeviceOperation { get; set; }
    }
}
