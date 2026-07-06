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
public class UsersController : ControllerBase
{
    private readonly IUseCase<NoInput, IEnumerable<UserDto>> _getAllUsersUseCase;
    private readonly IUseCase<Guid, UserDto?> _getUserByIdUseCase;
    private readonly IUseCase<CreateUserDto, UserDto> _createUserUseCase;
    private readonly IUseCase<UpdateUserRequest, bool> _updateUserUseCase;
    private readonly IUseCase<Guid, bool> _deleteUserUseCase;
    private readonly IUseCase<Guid, NoInput> _logicalDeletionUserUseCase;


    public UsersController(
        IUseCase<NoInput, IEnumerable<UserDto>> getAllUsersUseCase,
        IUseCase<Guid, UserDto?> getUserByIdUseCase,
        IUseCase<CreateUserDto, UserDto> createUserUseCase,
        IUseCase<UpdateUserRequest, bool> updateUserUseCase,
        IUseCase<Guid, bool> deleteUserUseCase,
        IUseCase<Guid, NoInput> logicalDeletionUserUseCase)
    {
        _getAllUsersUseCase = getAllUsersUseCase;
        _getUserByIdUseCase = getUserByIdUseCase;
        _createUserUseCase = createUserUseCase;
        _updateUserUseCase = updateUserUseCase;
        _deleteUserUseCase = deleteUserUseCase;
        _logicalDeletionUserUseCase = logicalDeletionUserUseCase;
    }

    [SwaggerOperation(Summary = "Lista todos os usuários", 
                      Description = "Retorna uma lista com todos os usuários cadastrados no sistema. (Requer Autenticação)")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _getAllUsersUseCase.ExecuteAsync(new NoInput());
        if (!result.IsSuccess) return BadRequest(new { Message = string.Join(", ", result.Messages) });
        return Ok(result.Response);
    }

    [SwaggerOperation(Summary = "Busca usuário por ID", 
                      Description = "Retorna os dados de um usuário específico a partir do seu identificador único. (Requer Autenticação)")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _getUserByIdUseCase.ExecuteAsync(id);
        if (!result.IsSuccess) return BadRequest(new { Message = string.Join(", ", result.Messages) });
        if (result.Response == null) return NotFound();
        return Ok(result.Response);
    }

    [SwaggerOperation(Summary = "Cria um novo usuário", 
                      Description = "Cadastra um novo usuário no sistema com os dados informados (Nome, Email, Senha e Role). " +
                                    "Esta rota permite acesso sem token temporariamente para facilitar a criação do primeiro administrador.")]
    [HttpPost]
    [AllowAnonymous] // Permitir criação do primeiro usuário
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        var result = await _createUserUseCase.ExecuteAsync(dto);
        if (!result.IsSuccess) return BadRequest(new { Message = string.Join(", ", result.Messages) });
        return CreatedAtAction(nameof(GetById), new { id = result.Response.Id }, result.Response);
    }

    [SwaggerOperation(Summary = "Atualiza os dados de um usuário", 
                      Description = "Atualiza as informações (Nome e Role) de um usuário existente a partir do seu identificador único. (Requer Autenticação)")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
    {
        var result = await _updateUserUseCase.ExecuteAsync(new UpdateUserRequest(id, dto));
        if (!result.IsSuccess) return BadRequest(new { Message = string.Join(", ", result.Messages) });
        return NoContent();
    }

    [SwaggerOperation(Summary = "Inativa/Ativa o usuário",
          Description = "Inativa ou ativa um usário existente a partir do identificador único.")]
    [HttpPut("{id}/LogicalDeletion")]
    public async Task<IActionResult> LogicalDeletion([FromRoute] Guid id)
    {
        var result = await _logicalDeletionUserUseCase.ExecuteAsync(id);

        return Ok(result.Response);
    }

    [SwaggerOperation(Summary = "Remove um usuário", 
                      Description = "Exclui permanentemente o cadastro de um usuário do sistema a partir do seu identificador único. (Requer Autenticação)")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _deleteUserUseCase.ExecuteAsync(id);
        if (!result.IsSuccess) return BadRequest(new { Message = string.Join(", ", result.Messages) });
        return NoContent();
    }
}
