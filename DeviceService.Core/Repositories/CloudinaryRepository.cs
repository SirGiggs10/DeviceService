using Ayuda_Help_Desk.API.Helpers;
using Ayuda_Help_Desk.Dtos.Cloudinary;
using Ayuda_Help_Desk.Dtos.General;
using Ayuda_Help_Desk.Helpers;
using Ayuda_Help_Desk.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Ayuda_Help_Desk.Repositories
{
    public class CloudinaryRepository : ICloudinaryRepository
    {
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private Cloudinary _cloudinary;

        public CloudinaryRepository(IOptions<CloudinarySettings> cloudinaryConfig)
        {
            _cloudinaryConfig = cloudinaryConfig;

            var acc = new Account(_cloudinaryConfig.Value.CloudName, _cloudinaryConfig.Value.ApiKey, _cloudinaryConfig.Value.ApiSecret);
            _cloudinary = new Cloudinary(acc);
        }

        public ReturnResponse UploadFilesToCloudinary(IFormFileCollection formFiles)
        {
            try
            {
                if(formFiles == null || !formFiles.Any())
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.ObjectNull,
                        StatusMessage = Utils.StatusMessageObjectNull
                    };
                }
            }
            catch(ArgumentNullException)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            var uploadedFiles = new List<RawUploadResultWithFileName>();
            for(int x = 0; x < formFiles.Count; x++)
            {
                try
                {
                    var file = formFiles[x];
                    var uploadResult = new RawUploadResult();
                    if (file.Length > 0 && file.Length < (100 * 1024))  //MAXIMUM FILE SIZE - 100MB
                    {
                        using (var stream = file.OpenReadStream())
                        {
                            uploadResult = _cloudinary.Upload("auto", null, new FileDescription(file.Name, stream));
                        }

                        uploadedFiles.Add(new RawUploadResultWithFileName()
                        {
                            RawUploadResult = uploadResult,
                            fileNameWithExtention = file.FileName,
                            FileType = file.ContentType
                        });
                    }
                    else
                    {
                        var deleteResult = DeleteOtherUploadedFiles(uploadedFiles, x);
                        if(deleteResult.StatusCode != Utils.Success)
                        {
                            return new ReturnResponse()
                            {
                                StatusCode = Utils.CloudinaryDeleteError,
                                StatusMessage = Utils.StatusMessageCloudinaryDeleteError
                            };
                        }

                        return new ReturnResponse()
                        {
                            StatusCode = Utils.InvalidFileSize,
                            StatusMessage = Utils.StatusMessageInvalidFileSize
                        };
                    }
                }
                catch(Exception)
                {
                    var deleteResult = DeleteOtherUploadedFiles(uploadedFiles, x);
                    if (deleteResult.StatusCode != Utils.Success)
                    {
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.CloudinaryDeleteError,
                            StatusMessage = Utils.StatusMessageCloudinaryDeleteError
                        };
                    }

                    return new ReturnResponse()
                    {
                        StatusCode = Utils.CloudinaryFileUploadError,
                        StatusMessage = Utils.StatusMessageCloudinaryFileUploadError
                    };
                }      
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                StatusMessage = Utils.StatusMessageSuccess,
                ObjectValue = uploadedFiles
            };
        }

        public ReturnResponse UploadFilesToCloudinary(IFormFile formFile)
        {
            if (formFile == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            var file = formFile;
            var uploadResult = new RawUploadResult();
            if (file.Length > 0 && file.Length < (100 * 1024 * 1024))  //MAXIMUM FILE SIZE - 100MB
            {
                using (var stream = file.OpenReadStream())
                {
                    uploadResult = _cloudinary.Upload("auto", null, new FileDescription(file.Name, stream));
                }

                return new ReturnResponse()
                {
                    StatusCode = Utils.Success,
                    ObjectValue = uploadResult,
                    StatusMessage = Utils.StatusMessageSuccess
                };
            }
            else
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.InvalidFileSize,
                    StatusMessage = Utils.StatusMessageInvalidFileSize
                };
            }
        }


        public ReturnResponse UploadExcelFileToCloudinary(FileInfo fileInfo)
        {
            if (fileInfo == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            var file = fileInfo;
            var uploadResult = new RawUploadResult();
            if (file.Length > 0 && file.Length < (100 * 1024 * 1024))  //MAXIMUM FILE SIZE - 100MB
            {
                using (var stream = file.OpenRead())
                {
                    uploadResult = _cloudinary.Upload("auto", null, new FileDescription(file.Name, stream));
                }

                return new ReturnResponse()
                {
                    StatusCode = Utils.Success,
                    ObjectValue = uploadResult,
                    StatusMessage = Utils.StatusMessageSuccess
                };
            }
            else
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.InvalidFileSize,
                    StatusMessage = Utils.StatusMessageInvalidFileSize
                };
            }
        }

        public ReturnResponse DeleteFilesFromCloudinary(List<string> attachedFilesPublicIds)
        {
            try
            {
                if (attachedFilesPublicIds == null || !attachedFilesPublicIds.Any())
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.ObjectNull,
                        StatusMessage = Utils.StatusMessageObjectNull
                    };
                }
            }
            catch (ArgumentNullException)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            var deleteResult = _cloudinary.DeleteResources(new DelResParams()
            {
                PublicIds = attachedFilesPublicIds
            });

            if (deleteResult.StatusCode != HttpStatusCode.OK)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.CloudinaryFileDeleteError,
                    StatusMessage = Utils.StatusMessageCloudinaryFileDeleteError
                };
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = deleteResult,
                StatusMessage = Utils.StatusMessageSuccess
            };
        }

        private ReturnResponse DeleteOtherUploadedFiles(List<RawUploadResultWithFileName> uploadedFormFiles, int fileIndex)
        {
            if((uploadedFormFiles == null) || (!uploadedFormFiles.Any()))
            {
                return new ReturnResponse()
                {
                    StatusMessage = Utils.StatusMessageObjectNull,
                    StatusCode = Utils.ObjectNull
                };
            }

            if(uploadedFormFiles.Count != fileIndex)
            {
                return new ReturnResponse()
                {
                    StatusMessage = Utils.StatusMessageBadRequest,
                    StatusCode = Utils.BadRequest
                };
            }

            var filesToDeletePublicUrls = uploadedFormFiles.Select(a => a.RawUploadResult.PublicId).ToList();
            var deletionResult = DeleteFilesFromCloudinary(filesToDeletePublicUrls);
            if(deletionResult.StatusCode != Utils.Success)
            {
                return new ReturnResponse()
                {
                    StatusMessage = Utils.StatusMessageNotSucceeded,
                    StatusCode = Utils.NotSucceeded
                };
            }

            return new ReturnResponse()
            {
                StatusMessage = Utils.StatusMessageSuccess,
                StatusCode = Utils.Success
            };
        }
    }

}
