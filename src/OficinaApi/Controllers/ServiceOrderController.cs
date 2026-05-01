using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace OficinaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Requires JWT
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

    [SwaggerOperation(Summary = "Cria uma nova ordem de serviço",
                      Description = "Abre uma nova OS no sistema. É possível informar o veículo, os serviços e as peças a utilizar. O estoque das peças é debitado automaticamente na criação.")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateServiceOrderDto dto)
    {
        var result = await _serviceOrderService.CreateServiceOrderAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
