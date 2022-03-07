using DeviceService.Core.Dtos.Global;
using DeviceService.Core.Helpers.Common;
using DeviceService.Core.Helpers.Logging.Logger;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DeviceService.Core.Helpers.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                LogWriter.WriteLog($"Something went wrong: {ex}", Utils.LogType.LOG_DEBUG, ex);

                if (!httpContext.Response.HasStarted)
                {
                    await HandleExceptionAsync(httpContext, ex);
                }               
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsync(JsonConvert.SerializeObject((new ControllerReturnResponse<string>()
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                ResponseDescription = Utils.StatusMessage_UnknownError,
                StatusCode = Utils.StatusCode_UnknownError,
                StatusMessage = Utils.StatusMessage_UnknownError,
                ObjectValue = ""
            })));
        }
    }
}
