using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OficinaApi.Application.DTOs;
using OpenTelemetry.Trace;

namespace OficinaApi.Presentation.ExceptionFilters;

public class GlobalExceptionFilter(
    ILogger<GlobalExceptionFilter> logger
) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        context.Result = new ObjectResult(UseCaseResponse<object>.Failure("Um erro inesperado ocorreu."))
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };
        context.ExceptionHandled = false;

        var activity = System.Diagnostics.Activity.Current;
        if (activity != null)
        {
            activity.SetStatus(System.Diagnostics.ActivityStatusCode.Error, context.Exception.Message);
            activity.AddException(context.Exception);
        }

        var httpMethod = context.HttpContext.Request.Method;
        var controller = context.HttpContext.Request.RouteValues["controller"] ?? string.Empty;
        var path = context.HttpContext.Request.Path.Value ?? string.Empty;

        logger.LogError(
            context.Exception,
            "Erro ao processar requisição no endpoint. Controller: {Controller}, Método: {HttpMethod}, Rota: {Path}, Mensagem: {ErrorMessage}",
            controller,
            httpMethod,
            path,
            context.Exception.Message
        );
    }
}
