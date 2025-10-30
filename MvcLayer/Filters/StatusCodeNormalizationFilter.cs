using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

namespace MvcLayer.Filters
{
    // Ensures 401/403 with a body are converted to empty status codes so StatusCodePages can re-execute
    public class StatusCodeNormalizationFilter : IAsyncResultFilter
    {
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            bool isAjaxOrJson = string.Equals(context.HttpContext.Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase)
                                || context.HttpContext.Request.Headers["Accept"].ToString().Contains("application/json", StringComparison.OrdinalIgnoreCase);

            if (!isAjaxOrJson)
            {
                // Handle ObjectResult (BadRequestObjectResult, NotFoundObjectResult, UnauthorizedObjectResult, ProblemDetails, etc.)
                if (context.Result is ObjectResult obj && obj.StatusCode.HasValue)
                {
                    int status = obj.StatusCode.Value;
                    if (status >= 400)
                    {
                        var message = ExtractMessage(obj.Value);
                        if (!string.IsNullOrWhiteSpace(message))
                        {
                            context.HttpContext.Items["StatusMessage"] = message;
                        }
                        context.Result = new StatusCodeResult(status);
                    }
                }
                // ContentResult with explicit 4xx/5xx
                else if (context.Result is ContentResult content && content.StatusCode.HasValue && content.StatusCode.Value >= 400)
                {
                    var message = content.Content;
                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        context.HttpContext.Items["StatusMessage"] = message;
                    }
                    context.Result = new StatusCodeResult(content.StatusCode.Value);
                }
                // JsonResult rarely used with 4xx explicitly; support if present
                else if (context.Result is JsonResult json)
                {
                    var statusCode = context.HttpContext.Response?.StatusCode ?? 200;
                    if (statusCode >= 400)
                    {
                        var message = ExtractMessage(json.Value);
                        if (!string.IsNullOrWhiteSpace(message))
                        {
                            context.HttpContext.Items["StatusMessage"] = message;
                        }
                        context.Result = new StatusCodeResult(statusCode);
                    }
                }
                // Forbid (403)
                else if (context.Result is ForbidResult)
                {
                    context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
                }
            }

            await next();
        }

        private static string? ExtractMessage(object? value)
        {
            if (value is null) return null;
            if (value is string s) return s;
            if (value is ProblemDetails pd)
            {
                if (!string.IsNullOrWhiteSpace(pd.Detail)) return pd.Detail;
                if (!string.IsNullOrWhiteSpace(pd.Title)) return pd.Title;
            }
            try
            {
                return JsonSerializer.Serialize(value);
            }
            catch
            {
                return value.ToString();
            }
        }
    }
}


