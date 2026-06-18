using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace OficinaApi.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Requires JWT
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [SwaggerOperation(Summary = "Lista todos os usuários", 
                      Description = "Retorna uma lista com todos os usuários cadastrados no sistema. (Requer Autenticação)")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [SwaggerOperation(Summary = "Busca usuário por ID", 
                      Description = "Retorna os dados de um usuário específico a partir do seu identificador único. (Requer Autenticação)")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [SwaggerOperation(Summary = "Cria um novo usuário", 
                      Description = "Cadastra um novo usuário no sistema com os dados informados (Nome, Email, Senha e Role). " +
                                    "Esta rota permite acesso sem token temporariamente para facilitar a criação do primeiro administrador.")]
    [HttpPost]
    [AllowAnonymous] // Permitir criação do primeiro usuário
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        try
        {
            var result = await _userService.CreateUserAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [SwaggerOperation(Summary = "Atualiza os dados de um usuário", 
                      Description = "Atualiza as informações (Nome e Role) de um usuário existente a partir do seu identificador único. (Requer Autenticação)")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
    {
        try
        {
            await _userService.UpdateUserAsync(id, dto);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [SwaggerOperation(Summary = "Remove um usuário", 
                      Description = "Exclui permanentemente o cadastro de um usuário do sistema a partir do seu identificador único. (Requer Autenticação)")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _userService.DeleteUserAsync(id);
        return NoContent();
    }
}
