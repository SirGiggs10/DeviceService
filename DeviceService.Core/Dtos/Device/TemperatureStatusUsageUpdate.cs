using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using static DeviceService.Core.Helpers.Common.Utils;

namespace DeviceService.Core.Dtos.Device
{
    public class TemperatureUpdate
    {
        [Required]
        public int DeviceId { get; set; }
        [Required]
        public double Temperature { get; set; }
    }

    public class StatusUpdate
    {
        [Required]
        public int DeviceId { get; set; }
        [Required]
        public DeviceStatus Status { get; set; }
    }

    public class UsageUpdate
    {
        [Required]
        public int DeviceId { get; set; }
        [Required]
        public double TotalUsageTimeInHours { get; set; }
    }
}
