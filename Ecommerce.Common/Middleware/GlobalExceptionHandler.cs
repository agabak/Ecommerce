using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Common.Middleware
{

    public sealed class GlobalExceptionHandler(
        RequestDelegate next,ILogger<GlobalExceptionHandler> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                // small change on the NuGet package version to avoid the error
                logger.LogError(ex, "An unhandled exception occurred while processing the request.");
                
                context.Response.StatusCode = ex switch
                { 
                    ApplicationException => StatusCodes.Status400BadRequest,
                     _=> StatusCodes.Status500InternalServerError
                };

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new ProblemDetails
                {
                    Type = ex.GetType().Name,
                    Title = "An error occurred while processing your request.",
                    Detail = ex.Message
                }));
            }
        }
    }
}
