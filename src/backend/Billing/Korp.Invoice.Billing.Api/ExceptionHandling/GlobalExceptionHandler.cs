using Korp.Invoice.Billing.Application.Exceptions;
using Korp.Invoice.Billing.Domain.Exceptions;
using Korp.Invoice.Inventory.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
namespace Korp.Invoice.Billing.Api.ExceptionHandling;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problemDetails = exception
        switch
        {
            ValidationException validationException => CreateValidationProblemDetails(httpContext, validationException),
            NotFoundException notFoundException => CreateProblemDetails(httpContext, StatusCodes.Status404NotFound, "Recurso não encontrado", notFoundException.Message),
            InventoryBusinessException inventoryBusinessException => CreateProblemDetails(httpContext, StatusCodes.Status409Conflict, "Conflito de estoque", inventoryBusinessException.Message),
            InventoryUnavailableException inventoryUnavailableException => CreateProblemDetails(httpContext, StatusCodes.Status503ServiceUnavailable, "Serviço indisponível", inventoryUnavailableException.Message),
            _ => CreateProblemDetails(httpContext, StatusCodes.Status500InternalServerError, "Erro interno", "Ocorreu um erro inesperado ao processar a requisição.")
        };

        if (problemDetails.Status >= StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Erro durante a requisição {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static ProblemDetails CreateProblemDetails(HttpContext httpContext, int status, string title, string detail)
        => new() { Status = status, Title = title, Detail = detail, Instance = httpContext.Request.Path };

    private static ValidationProblemDetails CreateValidationProblemDetails(HttpContext httpContext, ValidationException exception)
    {
        var errors = exception.Errors.GroupBy(error => error.PropertyName).ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());

        return new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Erro de validação",
            Detail = "Um ou mais campos possuem valores inválidos.",
            Instance = httpContext.Request.Path
        };
    }
}
