using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace OficinaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] 
public class ServiceOrdersController : ControllerBase
{
    private readonly IServiceOrderService _serviceOrderService;

    public ServiceOrdersController(IServiceOrderService serviceOrderService)
    {
        _serviceOrderService = serviceOrderService;
    }

    [SwaggerOperation(Summary = "Busca ordem de serviço por ID",
                      Description = "Retorna os dados completos de uma ordem de serviço, incluindo serviços realizados, peças utilizadas e veículo associado.")]
    [HttpGet("{id}")] 
    public async Task<IActionResult> GetById(Guid id)
    {
        var serviceOrder = await _serviceOrderService.GetServiceOrderByIdAsync(id);
        if (serviceOrder == null) return NotFound();
        return Ok(serviceOrder);
    } 

    [SwaggerOperation(Summary = "Cria  uma nova ordem de serviço",
                      Description = "Abre uma nova OS no sistema. É possível informar o veículo, os serviços a utilizar.")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateServiceOrderDto dto)
    {
        var result = await _serviceOrderService.CreateServiceOrderAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [SwaggerOperation(Summary = "Move uma ordem de serviço para analise",
                      Description = "Move uma ordem de serviço para o status de análise técnica.")]
    [HttpPost("{id}/startAnalysis")]
    public async Task<IActionResult> MoveToAnalysis(Guid id)
    {
        var result = await _serviceOrderService.StartDiagnosticsAsync(id);
        return Ok(result);
    }

    [SwaggerOperation(Summary = "Move uma ordem de serviço para execução",
                      Description = "Move uma ordem de serviço para o status de execução, indicando que os trabalhos começaram.")]
    [HttpPost("{id}/finishAnalysis")]
    public async Task<IActionResult> FinishAnalysis(Guid id)
    {
        var result = await _serviceOrderService.FinishAnalysisAsync(id);
        return Ok(result);
    }

    [SwaggerOperation(Summary = "Adiciona uma peça a ordem de serviço",
                      Description = "Adiciona uma peça a uma ordem de serviço existente.")]
    [HttpPost("{id}/parts")]
    public async Task<IActionResult> AddPartToServiceOrder(Guid id, [FromBody] AddPartDto dto)
    {
        var result = await _serviceOrderService.AddPartToServiceOrderAsync(id, dto);
        return Ok(result);
    }

    [SwaggerOperation(Summary = "Adiciona um serviço a ordem de serviço",
                      Description = "Adiciona um serviço a uma ordem de serviço existente.")]
    [HttpPost("{id}/services")]
    public async Task<IActionResult> AddServiceToServiceOrder(Guid id, [FromBody] AddServiceDto dto)
    {
        var result = await _serviceOrderService.AddServiceToServiceOrderAsync(id, dto);
        return Ok(result);
    }

    [SwaggerOperation(Summary = "Aprova uma ordem de serviço",
                      Description = "Move uma ordem de serviço para o status de execução, indicando que foi aprovada.")]
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApproveServiceOrder(Guid id)
    {
        var result = await _serviceOrderService.ApproveServiceOrderAsync(id);
        return Ok(result);
    }

    [SwaggerOperation(Summary = "Finaliza a execução de uma ordem de serviço",
                      Description = "Move uma ordem de serviço para o status de finalizada, indicando que a execução foi concluída.")]
    [HttpPost("{id}/finishExecution")]
    public async Task<IActionResult> FinishExecution(Guid id)
    {
        var result = await _serviceOrderService.FinishExecutionAsync(id);
        return Ok(result);
    }

    [SwaggerOperation(Summary = "Entrega uma ordem de serviço",
                      Description = "Move uma ordem de serviço para o status de entregue, indicando que foi entregue ao cliente.")]
    [HttpPost("{id}/deliver")]
    public async Task<IActionResult> DeliverServiceOrder(Guid id)
    {
        var result = await _serviceOrderService.DeliverServiceOrderAsync(id);
        return Ok(result);
    }

    [SwaggerOperation(Summary = "Lista os alertas da ordem de serviço",
                      Description = "Retorna uma lista de alertas relacionados a uma ordem de serviço, peças em falta ou problemas técnicos.")]
    [HttpGet("{id}/alerts")]
    public async Task<IActionResult> GetServiceOrderAlerts(Guid id)
    {
        var result = await _serviceOrderService.GetServiceOrderAlertsAsync(id);
        return Ok(result);
    }
}
