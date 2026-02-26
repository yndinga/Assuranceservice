using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AssuranceService.Api.ExceptionHandling;

public class ValidationExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ValidationExceptionHandler> _logger;

    public ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException)
            return false;

        _logger.LogWarning(validationException, "Erreur de validation des entrées : {Message}", validationException.Message);

        var errors = validationException.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        var problemDetails = new ProblemDetails
        {
            Status = (int)HttpStatusCode.BadRequest,
            Title = "Erreur de validation",
            Detail = "Une ou plusieurs erreurs de validation se sont produites.",
            Extensions = { ["errors"] = errors }
        };

        httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
