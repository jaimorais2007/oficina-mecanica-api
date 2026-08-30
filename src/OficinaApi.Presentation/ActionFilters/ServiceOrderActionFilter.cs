using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace OficinaApi.Presentation.ActionFilters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ServiceOrderActionFilterAttribute : TypeFilterAttribute
{
    public ServiceOrderActionFilterAttribute() : base(typeof(ServiceOrderActionFilter))
    {
    }
}

public class ServiceOrderActionFilter : IActionFilter
{
    private readonly ILogger<ServiceOrderActionFilter> _logger;

    public ServiceOrderActionFilter(ILogger<ServiceOrderActionFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        var httpMethod = context.HttpContext.Request.Method;
        var controller = context.HttpContext.Request.RouteValues["controller"] ?? string.Empty;
        var path = context.HttpContext.Request.PathBase.Value ?? string.Empty;

        _logger.LogError(
            context.Exception,
            "Erro ao processar requisição no endpoint. Controller: {Controller}, Método: {HttpMethod}, Rota: {Path}, Mensagem: {ErrorMessage}",
            controller,
            httpMethod,
            path
        );
    }

    private static string ExtractErrorMessage(object? value)
    {
        if (value is null)
        {
            return "Erro desconhecido";
        }

        if (value is string str)
        {
            return str;
        }

        var prop = value.GetType().GetProperty("message", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                   ?? value.GetType().GetProperty("Message", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        if (prop != null)
        {
            var val = prop.GetValue(value);
            if (val != null)
            {
                return val.ToString() ?? string.Empty;
            }
        }

        return value.ToString() ?? string.Empty;
    }
}
