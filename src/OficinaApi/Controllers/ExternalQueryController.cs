using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaApi.Application.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace OficinaApi.Controllers;

[ApiController]
[Route("api/external")]
[AllowAnonymous] // Public route for clients
public class ExternalQueryController : ControllerBase
{
    private readonly IExternalQueryService _queryService;

    public ExternalQueryController(IExternalQueryService queryService)
    {
        _queryService = queryService;
    }

    [SwaggerOperation(Summary = "Consulta o progresso de uma ordem de serviço",
                      Description = "Rota pública (sem autenticação) que permite ao cliente acompanhar o andamento da sua OS a partir do identificador único.")]
    [HttpGet("orders/{id}/progress")]
    public async Task<IActionResult> GetOrderProgress(Guid id)
    {
        var progress = await _queryService.GetOrderProgressAsync(id);
        if (progress == null) return NotFound(new { Message = "Ordem de Serviço não encontrada." });

        return Ok(progress);
    }

    [SwaggerOperation(Summary = "Retorna o tempo médio de execução dos serviços",
                      Description = "Rota pública (sem autenticação) que retorna métricas com o tempo médio de execução dos serviços realizados pela oficina.")]
    [HttpGet("metrics/average-execution-time")]
    public async Task<IActionResult> GetAverageExecutionTime()
    {
        var metric = await _queryService.GetAverageExecutionTimeAsync();
        return Ok(metric);
    }
}
