using System;
using System.Collections.Generic;
using System.Net;
using Azure;
using Bunnings.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Bunnings.Api.Filters
{
    public class ExceptionFilter : IActionFilter, IOrderedFilter
    {
        private readonly ILogger<ExceptionFilter> _logger;

        public ExceptionFilter(ILogger<ExceptionFilter> logger)
        {
            _logger = logger;
        }

        public int Order { get; set; } = int.MaxValue - 10;

        public void OnActionExecuting(ActionExecutingContext context)
        {
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            var headersToReturn = new Dictionary<string, string>();
            Error errorResponse = null;
            if (context.Exception != null)
            {
                errorResponse = new Error
                {
                    Type = "ApiError",
                    Status = HttpStatusCode.InternalServerError,
                    Errors = new List<string> { context.Exception.Message }
                };

                _logger.LogError(context.Exception, "Failed to process the request");
            }

            if (context.Exception is RequestFailedException serviceBusException)
            {
                if (serviceBusException.Status == (int)HttpStatusCode.TooManyRequests)
                {
                    var message = "Too Many Requests, retry after a while (Please make use of the RetryAfter header [in seconds])";
                    errorResponse = new Error
                    {
                        Type = "BlobError",
                        Status = HttpStatusCode.TooManyRequests,
                        Errors = new List<string> { message }
                    };

                    var retryAfter = TimeSpan.FromSeconds(10);
                    headersToReturn.Add("RetryAfter", ((int)retryAfter.TotalSeconds).ToString());
                }
            }

            if (errorResponse != null)
            {
                context.Result = new ObjectResult(errorResponse) { StatusCode = (int)errorResponse.Status };

                context.ExceptionHandled = true;

                if (headersToReturn.Count > 0)
                {
                    foreach (var header in headersToReturn)
                    {
                        context.HttpContext.Response.Headers.Add(header.Key, header.Value);
                    }
                }
            }

        }
    }
}