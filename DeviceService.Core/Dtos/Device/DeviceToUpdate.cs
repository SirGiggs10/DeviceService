using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DeviceService.Core.Dtos.Device
{
    public class DeviceToUpdate
    {
        [Required]
        public int DeviceId { get; set; }
        [Required]
        public string DeviceName { get; set; }
    }
}
