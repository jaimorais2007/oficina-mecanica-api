using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace OficinaApi.Presentation.ExceptionFilters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ServiceOrderExceptionFilterAttribute : TypeFilterAttribute
{
    public ServiceOrderExceptionFilterAttribute() : base(typeof(ServiceOrderExceptionFilter))
    {
    }
}

public class ServiceOrderExceptionFilter : IExceptionFilter
{
    private readonly ILogger<ServiceOrderExceptionFilter> _logger;

    public ServiceOrderExceptionFilter(ILogger<ServiceOrderExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        var httpMethod = context.HttpContext.Request.Method;
        var path = context.HttpContext.Request.Path.Value ?? string.Empty;
        var actionName = context.ActionDescriptor.DisplayName 
                         ?? context.ActionDescriptor.RouteValues["action"] 
                         ?? "Unknown";

        _logger.LogError(
            context.Exception,
            "[ServiceOrder] Erro ao processar requisição no endpoint. Método: {HttpMethod}, Rota: {Path}, Action: {Action}, Mensagem: {ErrorMessage}",
            httpMethod,
            path,
            actionName,
            context.Exception.Message
        );

        context.Result = new BadRequestObjectResult(new { message = context.Exception.Message });
        context.ExceptionHandled = true;
    }
}
