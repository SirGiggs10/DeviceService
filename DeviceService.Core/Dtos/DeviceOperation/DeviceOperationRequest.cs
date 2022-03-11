using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DeviceService.Core.Dtos.DeviceOperation
{
    public class DeviceOperationRequest
    {
        [Required]
        public string DeviceOperationName { get; set; }
    }
}
