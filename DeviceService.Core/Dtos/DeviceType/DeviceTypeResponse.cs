using DeviceService.Core.Dtos.DeviceOperation;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceService.Core.Dtos.DeviceType
{
    public class DeviceTypeResponse
    {
        public int DeviceTypeId { get; set; }
        public string DeviceTypeName { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public List<DeviceTypeOperationResponse> DeviceTypeOperations { get; set; }
    }
}
