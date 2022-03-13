using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceService.Core.Dtos.Device
{
    public class DevicePartialResponse
    {
        public int DeviceId { get; set; }
        public string DeviceName { get; set; }
        public int UserId { get; set; }
        public int DeviceTypeId { get; set; }
        public string Status { get; set; }
        public double Temperature { get; set; }
        public double TotalUsageTimeInHours { get; set; }
        public string DeviceIconPublicId { get; set; }
        public string DeviceIconUrl { get; set; }
        public string DeviceIconFileName { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
