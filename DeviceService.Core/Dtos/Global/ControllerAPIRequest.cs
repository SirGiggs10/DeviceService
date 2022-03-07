using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DeviceService.Core.Dtos.Global
{
    public class ControllerAPIRequest
    {
        [Required]
        public string RequestId { get; set; }
    }
}
