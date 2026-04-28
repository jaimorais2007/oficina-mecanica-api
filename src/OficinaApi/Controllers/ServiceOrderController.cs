using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;

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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var serviceOrder = await _serviceOrderService.GetServiceOrderByIdAsync(id);
        if (serviceOrder == null) return NotFound();
        return Ok(serviceOrder);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateServiceOrderDto dto)
    {
        var result = await _serviceOrderService.CreateServiceOrderAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
