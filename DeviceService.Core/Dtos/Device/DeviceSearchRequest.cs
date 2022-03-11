using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DeviceService.Core.Dtos.Device
{
    public class DeviceSearchRequest
    {
        [Required]
        public string SearchString { get; set; }
    }
}
