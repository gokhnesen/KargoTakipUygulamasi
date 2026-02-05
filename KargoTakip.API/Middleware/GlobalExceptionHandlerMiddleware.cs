using KargoTakip.Application.Exceptions;
using Microsoft.AspNetCore.Hosting;
using System.Net;
using System.Text.Json;

namespace KargoTakip.API.Middleware
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bir hata oluştu. Path: {Path}, Method: {Method}",
                    context.Request.Path, context.Request.Method);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var response = new ErrorResponse();

            switch (exception)
            {
                case ValidationException validationException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Doğrulama hatası oluştu.";
                    response.Errors = validationException.Errors;
                    _logger.LogWarning("Validation hatası: {Errors}", string.Join(", ", validationException.Errors));
                    break;

                case AuthenticationException authenticationException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response.Message = authenticationException.Message;
                    _logger.LogWarning("Authentication hatasi: {Message}", authenticationException.Message);
                    break;

                case NotFoundException notFoundException:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Message = notFoundException.Message;
                    _logger.LogWarning("NotFound: {Message}", notFoundException.Message);
                    break;

                case BusinessException businessException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = businessException.Message;
                    _logger.LogWarning("Business exception: {Message}", businessException.Message);
                    break;

                case ExternalServiceException externalServiceException:
                    context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                    response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                    response.Message = externalServiceException.Message;
                    _logger.LogWarning(externalServiceException, "Harici servis hatasi: {Message}", externalServiceException.Message);
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    var env = context.RequestServices.GetService<IWebHostEnvironment>();
                    response.Message = env?.IsDevelopment() == true
                        ? $"{exception.Message} ({exception.GetType().Name})"
                        : "Bir hata oluştu. Lütfen daha sonra tekrar deneyin.";
                    _logger.LogError(exception, "Beklenmeyen hata");
                    break;
            }

            var jsonResponse = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResponse);
        }
    }

    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}