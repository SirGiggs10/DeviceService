using DeviceService.Core.Helpers.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DeviceService.Core.Entities
{
    public class Device
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DeviceId { get; set; }
        public string DeviceName { get; set; }
        public int UserId { get; set; }
        public int DeviceTypeId { get; set; }
        public string Status { get; set; } = Utils.DeviceStatus.Offline.ToString();
        public double Temperature { get; set; } = 0;
        public double TotalUsageTimeInHours { get; set; } = 0;
        public string DeviceIconPublicId { get; set; }
        public string DeviceIconUrl { get; set; }
        public string DeviceIconFileName { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

        public virtual DeviceType DeviceType { get; set; }
        public virtual User User { get; set; }
    }
}
