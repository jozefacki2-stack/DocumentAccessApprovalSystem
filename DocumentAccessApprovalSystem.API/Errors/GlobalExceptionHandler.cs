using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace DocumentAccessApprovalSystem.API.Errors
{
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly IProblemDetailsService _problemDetails;

        public GlobalExceptionHandler(IProblemDetailsService problemDetails)
            => _problemDetails = problemDetails;

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception ex, CancellationToken ct)
        {
            var (status, title, extensions) = MapException(ex);

            var problem = new ProblemDetails
            {
                Status = (int)status,
                Title = title,
                Detail = ex is ValidationException ? null : ex.Message, // nie spamuj szczegółem przy walidacji
                Instance = httpContext.Request.Path
            };

            if (extensions is not null)
            {
                foreach (var kv in extensions)
                    problem.Extensions[kv.Key] = kv.Value!;
            }

            httpContext.Response.StatusCode = problem.Status!.Value;
            return await _problemDetails.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problem
            });
        }

        private static (HttpStatusCode status, string title, IDictionary<string, object?>? extensions) MapException(Exception ex)
        {
            return ex switch
            {
                ValidationException vex => (
                    HttpStatusCode.BadRequest,
                    "Validation failed",
                    new Dictionary<string, object?>
                    {
                        ["errors"] = vex.Errors
                            .GroupBy(e => e.PropertyName)
                            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
                    }
                ),
                KeyNotFoundException => (HttpStatusCode.NotFound, "Resource not found", null),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized", null),
                InvalidOperationException => (HttpStatusCode.BadRequest, "Invalid operation", null),
                Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException => (HttpStatusCode.Conflict, "Concurrency conflict", null),
                _ => (HttpStatusCode.InternalServerError, "Unexpected error", null)
            };
        }
    }
}
