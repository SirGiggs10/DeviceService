using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceService.Core.Dtos.RoleFunctionality
{
    public class FunctionalityRequest
    {
        public string FunctionalityName { get; set; }
        public string FunctionalityDescription { get; set; }
        public int ProjectModuleId { get; set; }
    }
}
