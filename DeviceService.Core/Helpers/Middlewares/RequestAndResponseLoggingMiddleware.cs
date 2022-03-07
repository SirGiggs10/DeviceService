using DeviceService.Core.Helpers.Common;
using DeviceService.Core.Helpers.Logging.Logger;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DeviceService.Core.Helpers.Middlewares
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;

        public RequestResponseLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
        }

        public async Task Invoke(HttpContext context)
        {
            await LogRequest(context);
            await LogResponse(context);
        }

        private async Task LogRequest(HttpContext context)
        {
            context.Request.EnableBuffering();

            using (var requestStream = _recyclableMemoryStreamManager.GetStream())
            {
                await context.Request.Body.CopyToAsync(requestStream);
                var log = $"\n\nHttp Request Information:{Environment.NewLine}" + $"Schema:{context.Request.Scheme} " + $"Host: {context.Request.Host} " + $"Path: {context.Request.Path} " + $"Method: {context.Request.Method } " + $"QueryString: {context.Request.QueryString} " + $"Request Body: {ReadStreamInChunks(requestStream)}\n";
                LogWriter.WriteLog(log, Utils.LogType.LOG_INFORMATION);
            }

            context.Request.Body.Position = 0;
        }

        private static string ReadStreamInChunks(Stream stream)
        {
            const int readChunkBufferLength = 4096;

            stream.Seek(0, SeekOrigin.Begin);
            using (var textWriter = new StringWriter())
            {
                using (var reader = new StreamReader(stream))
                {
                    var readChunk = new char[readChunkBufferLength];
                    int readChunkLength;

                    do
                    {
                        readChunkLength = reader.ReadBlock(readChunk,
                                                           0,
                                                           readChunkBufferLength);
                        textWriter.Write(readChunk, 0, readChunkLength);
                    } while (readChunkLength > 0);

                    return textWriter.ToString();
                }
            }
        }

        private async Task LogResponse(HttpContext context)
        {
            var originalBodyStream = context.Response.Body;

            using (var responseBody = _recyclableMemoryStreamManager.GetStream())
            {
                context.Response.Body = responseBody;

                await _next(context);

                context.Response.Body.Seek(0, SeekOrigin.Begin);
                var text = await new StreamReader(context.Response.Body).ReadToEndAsync();
                context.Response.Body.Seek(0, SeekOrigin.Begin);

                var log = $"\nHttp Response Information:{Environment.NewLine}" + $"Schema:{context.Request.Scheme} " + $"Host: {context.Request.Host} " + $"Path: {context.Request.Path} " + $"Method: {context.Request.Method } " + $"QueryString: {context.Request.QueryString} " + $"Response Body: {text}\n";
                LogWriter.WriteLog(log, Utils.LogType.LOG_INFORMATION);

                await responseBody.CopyToAsync(originalBodyStream);
            }
        }
    }
}
