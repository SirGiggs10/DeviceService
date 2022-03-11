using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DeviceService.Core.Dtos.DeviceOperation
{
    public class DeviceOperationToUpdate
    {
        [Required]
        public int DeviceOperationId { get; set; }
        [Required]
        public string DeviceOperationName { get; set; }
    }
}
