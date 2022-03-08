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
        public int Status { get; set; }
        public double Temperature { get; set; }
        public int TotalUpTimeInHours { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

        public virtual DeviceType DeviceType { get; set; }
    }
}
