using DeviceService.Core.Dtos.Global;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceService.Core.Interfaces.Repositories
{
    public interface ICloudinaryRepository
    {
        public ReturnResponse UploadFilesToCloudinary(IFormFileCollection formFiles);
        public ReturnResponse UploadFilesToCloudinary(IFormFile formFile);
        public ReturnResponse UploadExcelFileToCloudinary(FileInfo fileInfo);
        public ReturnResponse DeleteFilesFromCloudinary(List<string> attachedFilesPublicIds);
    }
}
