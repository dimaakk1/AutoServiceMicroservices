using System.Net;
using System.Text.Json;

namespace AutoServiceCatalog.API.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var status = HttpStatusCode.InternalServerError; 
            var title = "Internal Server Error";
            if (exception is KeyNotFoundException)
            {
                status = HttpStatusCode.NotFound;
                title = "Resource Not Found";
            }
            else if (exception is ArgumentException)
            {
                status = HttpStatusCode.BadRequest;
                title = "Bad Request";
            }

            var problemDetails = new
            {
                type = "https://example.com/probs/error",
                title,
                status = (int)status,
                detail = exception.Message,
                instance = context.Request.Path
            };

            var result = JsonSerializer.Serialize(problemDetails);
            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)status;

            return context.Response.WriteAsync(result);
        }
    }

}
