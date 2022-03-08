using CloudinaryDotNet.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceService.Core.Dtos.Cloudinary
{
    public class RawUploadResultWithFileName
    {
        public RawUploadResult RawUploadResult { get; set; }
        public string fileNameWithExtention { get; set; }
        public string FileType { get; set; }
    }
}
