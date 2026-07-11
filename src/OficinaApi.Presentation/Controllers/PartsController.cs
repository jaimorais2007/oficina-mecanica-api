using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Application.UseCases.Vehicles;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OficinaApi.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Requires JWT
public class PartsController : ControllerBase
{
    private readonly IUseCase<NoInput, IEnumerable<PartDto>> _getAllPartsUseCase;
    private readonly IUseCase<Guid, PartDto?> _getPartByIdUseCase;
    private readonly IUseCase<CreatePartDto, PartDto> _createPartUseCase;
    private readonly IUseCase<AddStockRequest, bool> _addStockUseCase;
    private readonly IUseCase<RemoveStockRequest, bool> _removeStockUseCase;
    private readonly IUseCase<Guid, bool> _deletePartUseCase;
    private readonly IUseCase<Guid, NoInput> _logicalDeletionPartsUseCase;


    public PartsController(
        IUseCase<NoInput, IEnumerable<PartDto>> getAllPartsUseCase,
        IUseCase<Guid, PartDto?> getPartByIdUseCase,
        IUseCase<CreatePartDto, PartDto> createPartUseCase,
        IUseCase<AddStockRequest, bool> addStockUseCase,
        IUseCase<RemoveStockRequest, bool> removeStockUseCase,
        IUseCase<Guid, bool> deletePartUseCase,
        IUseCase<Guid, NoInput> logicalDeletionPartsUseCase)
    {
        _getAllPartsUseCase = getAllPartsUseCase;
        _getPartByIdUseCase = getPartByIdUseCase;
        _createPartUseCase = createPartUseCase;
        _addStockUseCase = addStockUseCase;
        _removeStockUseCase = removeStockUseCase;
        _deletePartUseCase = deletePartUseCase;
        _logicalDeletionPartsUseCase = logicalDeletionPartsUseCase;
    }

    [SwaggerOperation(Summary = "Lista todas as peças cadastradas",
                      Description = "Retorna uma lista com todas as peças e insumos registrados no estoque.")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _getAllPartsUseCase.ExecuteAsync(new NoInput());
        if (!result.IsSuccess) return BadRequest(new { Message = string.Join(", ", result.Messages) });
        return Ok(result.Response);
    }

    [SwaggerOperation(Summary = "Busca peça por ID",
                      Description = "Retorna os dados de uma peça específica a partir do seu identificador único.")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _getPartByIdUseCase.ExecuteAsync(id);
        if (!result.IsSuccess) return BadRequest(new { Message = string.Join(", ", result.Messages) });
        if (result.Response == null) return NotFound();
        return Ok(result.Response);
    }

    [SwaggerOperation(Summary = "Cria uma nova peça",
                      Description = "Cadastra um nova peça ou insumo no estoque com os dados informados no corpo da requisição.")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePartDto dto)
    {
        var result = await _createPartUseCase.ExecuteAsync(dto);
        if (!result.IsSuccess) return BadRequest(new { Message = string.Join(", ", result.Messages) });
        return CreatedAtAction(nameof(GetById), new { id = result.Response.Id }, result.Response);
    }

    [SwaggerOperation(Summary = "Adiciona quantidade ao estoque de uma peça",
                      Description = "Incrementa a quantidade disponível no estoque de uma peça específica pelo seu identificador único.")]
    [HttpPost("{id}/add-stock")]
    public async Task<IActionResult> AddStock(Guid id, [FromBody] UpdateStockDto dto)
    {
        var result = await _addStockUseCase.ExecuteAsync(new AddStockRequest(id, dto.Quantity));
        if (!result.IsSuccess)
        {
            if (result.Messages.Any(m => m.Contains("não encontrada")))
            {
                return NotFound(new { Message = string.Join(", ", result.Messages) });
            }
            return BadRequest(new { Message = string.Join(", ", result.Messages) });
        }
        return NoContent();
    }

    [SwaggerOperation(Summary = "Remove quantidade do estoque de uma peça",
                      Description = "Decrementa a quantidade disponível no estoque de uma peça. Retorna erro se a quantidade a remover for maior do que o estoque disponível.")]
    [HttpPost("{id}/remove-stock")]
    public async Task<IActionResult> RemoveStock(Guid id, [FromBody] UpdateStockDto dto)
    {
        var result = await _removeStockUseCase.ExecuteAsync(new RemoveStockRequest(id, dto.Quantity));
        if (!result.IsSuccess)
        {
            return BadRequest(new { Message = string.Join(", ", result.Messages) });
        }
        return NoContent();
    }

    [SwaggerOperation(Summary = "Inativa/Ativa uma peça",
          Description = "Inativa ou ativa uma peça existente a partir do identificador único.")]
    [HttpPut("{id}/LogicalDeletion")]
    public async Task<IActionResult> LogicalDeletion([FromRoute] Guid id)
    {
        var result = await _logicalDeletionPartsUseCase.ExecuteAsync(id);

        return Ok(result.Response);
    }

    [SwaggerOperation(Summary = "Remove uma peça",
                      Description = "Exclui permanentemente o cadastro de uma peça do estoque a partir do seu identificador único.")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _deletePartUseCase.ExecuteAsync(id);
        if (!result.IsSuccess) return BadRequest(new { Message = string.Join(", ", result.Messages) });
        return NoContent();
    }
}
