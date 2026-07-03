namespace CrawlScope.Api.Common.Http
{
    public static class ProblemDetailsFactory
    {
        public static ApiErrorResponse Create(
            HttpContext context,
            int statusCode,
            string? message = null,
            object? errors = null,
            Exception? exception = null,
            IHostEnvironment? environment = null)
        {
            var response = new ApiErrorResponse
            {
                StatusCode = statusCode,
                Message = message ?? GetDefaultMessage(statusCode),
                Errors = errors,
                TraceId = context.TraceIdentifier
            };

            if (exception is not null
                && environment?.IsDevelopment() == true
                && statusCode >= StatusCodes.Status500InternalServerError)
            {
                response.Errors = new Dictionary<string, string[]>
                {
                    ["Exception"] = [exception.GetType().Name],
                    ["Message"] = [exception.Message],
                    ["Source"] = [exception.Source ?? string.Empty],
                    ["Path"] = [context.Request.Path.ToString()]
                };
            }

            return response;
        }

        private static string GetDefaultMessage(int statusCode)
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
