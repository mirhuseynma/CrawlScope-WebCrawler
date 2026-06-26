using Microsoft.AspNetCore.Mvc;

namespace CrawlScope.Api.Common.Http
{
    public static class ProblemDetailsFactory
    {
        public static ProblemDetails Create(
            HttpContext context,
            int statusCode,
            string? detail = null,
            Exception? exception = null,
            IHostEnvironment? environment = null)
        {
            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = GetTitle(statusCode),
                Detail = detail ?? GetDefaultDetail(statusCode)
            };

            problemDetails.Extensions["traceId"] = context.TraceIdentifier;

            if (exception is not null && environment?.IsDevelopment() == true)
            {
                problemDetails.Detail = exception.Message;
                problemDetails.Extensions["exceptionType"] = exception.GetType().Name;
                problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            }

            return problemDetails;
        }

        private static string GetTitle(int statusCode)
        {
            return statusCode switch
            {
                StatusCodes.Status400BadRequest => "Bad request",
                StatusCodes.Status401Unauthorized => "Unauthorized",
                StatusCodes.Status403Forbidden => "Forbidden",
                StatusCodes.Status404NotFound => "Not found",
                StatusCodes.Status405MethodNotAllowed => "Method not allowed",
                StatusCodes.Status409Conflict => "Conflict",
                StatusCodes.Status415UnsupportedMediaType => "Unsupported media type",
                StatusCodes.Status422UnprocessableEntity => "Unprocessable entity",
                StatusCodes.Status429TooManyRequests => "Too many requests",
                StatusCodes.Status500InternalServerError => "Unexpected server error",
                StatusCodes.Status502BadGateway => "Bad gateway",
                StatusCodes.Status503ServiceUnavailable => "Service unavailable",
                StatusCodes.Status504GatewayTimeout => "Gateway timeout",
                _ when statusCode >= 400 && statusCode < 500 => "Client error",
                _ when statusCode >= 500 => "Server error",
                _ => "Request status"
            };
        }

        private static string GetDefaultDetail(int statusCode)
        {
            return statusCode switch
            {
                StatusCodes.Status400BadRequest => "The request could not be processed. Check the submitted data.",
                StatusCodes.Status401Unauthorized => "Authentication is required to access this resource.",
                StatusCodes.Status403Forbidden => "You do not have permission to access this resource.",
                StatusCodes.Status404NotFound => "The requested resource was not found.",
                StatusCodes.Status405MethodNotAllowed => "The HTTP method is not allowed for this endpoint.",
                StatusCodes.Status409Conflict => "The request conflicts with the current resource state.",
                StatusCodes.Status415UnsupportedMediaType => "The request content type is not supported.",
                StatusCodes.Status422UnprocessableEntity => "The request was understood, but it could not be processed.",
                StatusCodes.Status429TooManyRequests => "Too many requests were sent in a short period of time.",
                StatusCodes.Status500InternalServerError => "An unexpected error occurred while processing the request.",
                StatusCodes.Status502BadGateway => "The server received an invalid response from an upstream service.",
                StatusCodes.Status503ServiceUnavailable => "The service is temporarily unavailable.",
                StatusCodes.Status504GatewayTimeout => "The server timed out while waiting for an upstream service.",
                _ when statusCode >= 400 && statusCode < 500 => "The request could not be completed because of a client error.",
                _ when statusCode >= 500 => "The request could not be completed because of a server error.",
                _ => "The request completed with this status."
            };
        }
    }
}
