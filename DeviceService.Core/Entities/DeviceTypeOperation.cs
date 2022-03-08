using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceService.Core.Entities
{
    public class DeviceTypeOperation
    {
        public int DeviceTypeId { get; set; }
        public int DeviceOperationId { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

        public virtual DeviceType DeviceType { get; set; }
        public virtual DeviceOperation DeviceOperation { get; set; }
    }
}
