using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceService.Core.Dtos.Global
{
    public class ReturnResponse<T> : APIResponse where T : class
    {
        public string ServiceRequest { get; set; }
        public string ServiceResponseDescription { get; set; }
        public int ServiceResponseCode { get; set; }
        public T ObjectValue { get; set; }
        public List<Log> Logs { get; set; }
    }
}
