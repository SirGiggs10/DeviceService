using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DeviceService.Core.Dtos.Global
{
    public class APIResponse
    {
        public HttpStatusCode ResponseCode { get; set; }
        public string ResponseDescription { get; set; }
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
    }
}
