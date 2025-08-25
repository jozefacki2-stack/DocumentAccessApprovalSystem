using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DocumentAccessApprovalSystem.API.Extensions
{
    public static class ExceptionHandling
    {
        public static void UseValidationProblemDetails(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(appErr =>
            {
                appErr.Run(async context =>
                {
                    var feat = context.Features.Get<IExceptionHandlerFeature>();
                    if (feat?.Error is ValidationException vex)
                    {
                        var details = new ValidationProblemDetails(
                            vex.Errors.GroupBy(e => e.PropertyName)
                                      .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()))
                        {
                            Status = StatusCodes.Status400BadRequest,
                            Title = "Validation failed"
                        };
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsJsonAsync(details);
                    }
                });
            });
        }
    }
}
