using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DeviceService.Core.Dtos.DeviceType
{
    public class DeviceTypeRequest
    {
        [Required]
        public string DeviceTypeName { get; set; }
        [Required]
        public List<DeviceTypeOperationRequest> DeviceTypeOperations { get; set; }
    }
}
