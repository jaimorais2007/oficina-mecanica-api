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
public class PartsController : ControllerBase
{
    private readonly IPartService _partService;

    public PartsController(IPartService partService)
    {
        _partService = partService;
    }

    [SwaggerOperation(Summary = "Lista todas as peças cadastradas",
                      Description = "Retorna uma lista com todas as peças e insumos registrados no estoque.")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var parts = await _partService.GetAllPartsAsync();
        return Ok(parts);
    }

    [SwaggerOperation(Summary = "Busca peça por ID",
                      Description = "Retorna os dados de uma peça específica a partir do seu identificador único.")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var part = await _partService.GetPartByIdAsync(id);
        if (part == null) return NotFound();
        return Ok(part);
    }

    [SwaggerOperation(Summary = "Cria uma nova peça",
                      Description = "Cadastra uma nova peça ou insumo no estoque com os dados informados no corpo da requisição.")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePartDto dto)
    {
        var result = await _partService.CreatePartAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [SwaggerOperation(Summary = "Adiciona quantidade ao estoque de uma peça",
                      Description = "Incrementa a quantidade disponível no estoque de uma peça específica pelo seu identificador único.")]
    [HttpPost("{id}/add-stock")]
    public async Task<IActionResult> AddStock(Guid id, [FromBody] UpdateStockDto dto)
    {
        try
        {
            await _partService.AddStockAsync(id, dto.Quantity);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [SwaggerOperation(Summary = "Remove quantidade do estoque de uma peça",
                      Description = "Decrementa a quantidade disponível no estoque de uma peça. Retorna erro se a quantidade a remover for maior do que o estoque disponível.")]
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

    [SwaggerOperation(Summary = "Remove uma peça",
                      Description = "Exclui permanentemente o cadastro de uma peça do estoque a partir do seu identificador único.")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _partService.DeletePartAsync(id);
        return NoContent();
    }
}
