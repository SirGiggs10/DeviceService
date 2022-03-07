using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceService.Core.Dtos.Global
{
    public class Error_General<T> where T : class
    {
        public int ApiResponseCode { get; set; }
        public T ErrorObject { get; set; }
    }
}
