using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DeviceService.Core.Dtos.DeviceType
{
    public class DeviceTypeToUpdate
    {
        [Required]
        public int DeviceTypeId { get; set; }
        [Required]
        public string DeviceTypeName { get; set; }
    }
}
