using DeviceService.Core.Dtos.Auth;
using DeviceService.Core.Dtos.DeviceType;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceService.Core.Dtos.Device
{
    public class DeviceWithRelatedDevicesResponse
    {
        public int DeviceId { get; set; }
        public string DeviceName { get; set; }
        public int UserId { get; set; }
        public int DeviceTypeId { get; set; }
        public int Status { get; set; }
        public double Temperature { get; set; }
        public double TotalUpTimeInHours { get; set; }
        public string DeviceIconPublicId { get; set; }
        public string DeviceIconUrl { get; set; }
        public string DeviceIconFileName { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
        public DeviceTypeResponse DeviceType { get; set; }
        public UserToReturn User { get; set; }
        public List<DevicePartialResponse> RelatedDevices { get; set; }
    }
}
