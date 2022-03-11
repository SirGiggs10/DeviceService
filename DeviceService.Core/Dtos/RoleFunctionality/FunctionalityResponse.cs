using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceService.Core.Dtos.RoleFunctionality
{
    public class FunctionalityResponse
    {
        public int FunctionalityId { get; set; }
        public string FunctionalityName { get; set; }
        public string FunctionalityDescription { get; set; }
        public int ProjectModuleId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
