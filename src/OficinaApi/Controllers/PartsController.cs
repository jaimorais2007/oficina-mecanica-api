using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;

namespace OficinaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize] // Temporariamente desativado para o Kawan conseguir testar o MVP local via Swagger, sem depender do TokenService ainda.
public class PartsController : ControllerBase
{
    private readonly IPartService _partService;

    public PartsController(IPartService partService)
    {
        _partService = partService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var parts = await _partService.GetAllPartsAsync();
        return Ok(parts);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var part = await _partService.GetPartByIdAsync(id);
        if (part == null) return NotFound();
        return Ok(part);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePartDto dto)
    {
        var result = await _partService.CreatePartAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("{id}/add-stock")]
    public async Task<IActionResult> AddStock(Guid id, [FromBody] UpdateStockDto dto)
    {
        try
        {
            await _partService.AddStockAsync(id, dto.Quantity);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("{id}/remove-stock")]
    public async Task<IActionResult> RemoveStock(Guid id, [FromBody] UpdateStockDto dto)
    {
        try
        {
            await _partService.RemoveStockAsync(id, dto.Quantity);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _partService.DeletePartAsync(id);
        return NoContent();
    }
}
