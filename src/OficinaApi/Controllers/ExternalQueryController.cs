using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaApi.Application.Interfaces;

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

    // Acompanhamento do progresso da OS
    [HttpGet("orders/{id}/progress")]
    public async Task<IActionResult> GetOrderProgress(Guid id)
    {
        var progress = await _queryService.GetOrderProgressAsync(id);
        if (progress == null) return NotFound(new { Message = "Ordem de Serviço não encontrada." });

        return Ok(progress);
    }

    // Monitoramento do tempo médio de execução dos serviços
    [HttpGet("metrics/average-execution-time")]
    public async Task<IActionResult> GetAverageExecutionTime()
    {
        var metric = await _queryService.GetAverageExecutionTimeAsync();
        return Ok(metric);
    }
}
