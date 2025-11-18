using System.Net;
using System.Text.Json;

namespace StudentManagement.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[Global Exception] {ex.Message}");

                if (IsApiRequest(context))
                {
                    await WriteJsonError(context, ex);
                }
                else
                {
                    await RedirectToErrorPage(context, ex);
                }
            }
        }

        private bool IsApiRequest(HttpContext context)
        {
            return context.Request.Path.Value!.StartsWith("/api")
                || context.Request.Headers["Accept"].ToString().Contains("application/json");
        }

        private Task WriteJsonError(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var error = new
            {
                statusCode = context.Response.StatusCode,
                message = "An unexpected error occurred.",
                detail = _env.IsDevelopment() ? ex.Message : null
            };

            var json = JsonSerializer.Serialize(error);
            return context.Response.WriteAsync(json);
        }

        private Task RedirectToErrorPage(HttpContext context, Exception ex)
        {
            var traceId = Guid.NewGuid().ToString();

            _logger.LogError(ex, $"Error Trace ID: {traceId}");

            context.Response.Redirect($"/Home/Error?traceId={traceId}");
            return Task.CompletedTask;
        }
    }
}
