using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceService.Core.Dtos.RoleFunctionality
{
    public class ProjectModuleResponse
    {
        public int ProjectModuleId { get; set; }
        public string ProjectModuleName { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public List<FunctionalityResponse> Functionalities { get; set; }
    }
}
