using DeviceService.Core.Dtos.Global;
using DeviceService.Core.Helpers.Common;
using DeviceService.Core.Helpers.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace DeviceService.Core.Helpers.Filters.ActionFilters
{
    public class ValidationActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = new ObjectResult(new ControllerReturnResponse<string>()
                {
                    StatusCode = Utils.StatusCode_BadRequest,
                    StatusMessage = context.ModelState.Where(x => x.Value.Errors.Count > 0).ToDictionary(kvp => kvp.Key, kvp => string.Join(", ", kvp.Value.Errors.Select(e => e.ErrorMessage))).ToDictionaryString()
                })
                {
                    StatusCode = Utils.HttpStatusCode_BadRequest
                };
            }
            //base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);
        }

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            base.OnResultExecuting(context);
        }

        public override void OnResultExecuted(ResultExecutedContext context)
        {
            base.OnResultExecuted(context);
        }
    }
}
