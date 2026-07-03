using CrawlScope.Api.Common.Http;
using CrawlScope.Application.Common.Exceptions;
using FluentValidation;

namespace CrawlScope.Api.Common.Middleware
{
    public class ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (ValidationException ex)
            {
                await WriteValidationProblemAsync(context, ex);
            }
            catch (ArgumentException ex)
            {
                await WriteProblemAsync(context, ex, StatusCodes.Status400BadRequest, ex.Message);
            }
            catch (NotFoundException ex)
            {
                await WriteProblemAsync(context, ex, StatusCodes.Status404NotFound, ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                await WriteProblemAsync(context, ex, StatusCodes.Status403Forbidden, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                await WriteProblemAsync(context, ex, StatusCodes.Status409Conflict, ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception occurred while processing the request.");
                await WriteProblemAsync(
                    context,
                    ex,
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred while processing the request.");
            }
        }

        private static async Task WriteValidationProblemAsync(HttpContext context, ValidationException exception)
        {
            var errors = exception.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).ToArray());

            var response = ProblemDetailsFactory.Create(
                context,
                StatusCodes.Status400BadRequest,
                "Validation failed.",
                errors);

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(response);
        }

        private async Task WriteProblemAsync(
            HttpContext context,
            Exception exception,
            int statusCode,
            string detail)
        {
            if (statusCode >= StatusCodes.Status500InternalServerError)
            {
                logger.LogError(exception, "Server error occurred while processing the request.");
            }
            else
            {
                logger.LogWarning(exception, "Request failed with status code {StatusCode}.", statusCode);
            }

            var response = ProblemDetailsFactory.Create(
                context,
                statusCode,
                detail,
                errors: null,
                exception,
                environment);

            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
