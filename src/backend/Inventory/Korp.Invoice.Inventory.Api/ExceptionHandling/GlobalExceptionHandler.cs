using FluentValidation;
using Korp.Invoice.Inventory.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
namespace Korp.Invoice.Inventory.Api.ExceptionHandling;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problemDetails = CreateProblemDetails(httpContext, exception);
        if (problemDetails.Status >= StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Erro não tratado durante a requisição {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
    private static ProblemDetails CreateProblemDetails(HttpContext httpContext, Exception exception)
    {
        return exception
        switch
        {
            ValidationException validationException => CreateValidationProblemDetails(httpContext, validationException),
            NotFoundException => CreateProblemDetails(httpContext, StatusCodes.Status404NotFound, "Recurso não encontrado", exception.Message),
            ConflictException or InsufficientStockException => CreateProblemDetails(httpContext, StatusCodes.Status409Conflict, "Conflito de negócio", exception.Message),
            ArgumentException => CreateProblemDetails(httpContext, StatusCodes.Status400BadRequest, "Requisição inválida", exception.Message),
            StockConcurrencyException => CreateProblemDetails(httpContext, StatusCodes.Status409Conflict, "Conflito de estoque", exception.Message),
            ProductInUseException productInUseException => CreateProblemDetails(httpContext,StatusCodes.Status409Conflict,"Produto em uso", productInUseException.Message),
            _ => CreateProblemDetails(httpContext, StatusCodes.Status500InternalServerError, "Erro interno", "Ocorreu um erro inesperado ao processar a requisição.")
        };
    }
    private static ProblemDetails CreateProblemDetails(HttpContext httpContext, int status, string title, string detail)
    {
        return new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };
    }
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
