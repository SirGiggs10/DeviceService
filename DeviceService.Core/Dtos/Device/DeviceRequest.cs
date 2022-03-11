using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DeviceService.Core.Dtos.Device
{
    public class DeviceRequest
    {
        [Required(ErrorMessage = "DeviceName is Required")]
        public string DeviceName { get; set; }
        [Required]
        public int DeviceTypeId { get; set; }
        [Required]
        public IFormFile DeviceIcon { get; set; }
    }
}
