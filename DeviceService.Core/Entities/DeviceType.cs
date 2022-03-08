using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DeviceService.Core.Entities
{
    public class DeviceType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DeviceTypeId { get; set; }
        public string DeviceTypeName { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

        public virtual List<Device> Devices { get; set; }
        public virtual List<DeviceTypeOperation> DeviceTypeOperations { get; set; }
    }
}
