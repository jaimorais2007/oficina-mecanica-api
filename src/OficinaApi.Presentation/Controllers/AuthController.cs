using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace OficinaApi.Presentation.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUseCase<AuthenticateUserRequest, UserDto?> _authenticateUserUseCase;
    private readonly IUseCase<GenerateTokenRequest, string> _generateTokenUseCase;

    public AuthController(
        IUseCase<AuthenticateUserRequest, UserDto?> authenticateUserUseCase,
        IUseCase<GenerateTokenRequest, string> generateTokenUseCase)
    {
        _authenticateUserUseCase = authenticateUserUseCase;
        _generateTokenUseCase = generateTokenUseCase;
    }

    [SwaggerOperation(Summary = "Realiza o login e retorna o Token JWT", 
                      Description = "Autentica um usuário existente a partir do e-mail e senha, retornando os dados do usuário e um Token Bearer válido para uso nas outras rotas.")]
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest(new { Message = "E-mail e senha são obrigatórios." });
        }

        var authResult = await _authenticateUserUseCase.ExecuteAsync(new AuthenticateUserRequest(dto.Email, dto.Password));

        if (!authResult.IsSuccess || authResult.Response == null)
        {
            return Unauthorized(new { Message = "E-mail ou senha incorretos." });
        }

        var user = authResult.Response;
        var tokenResult = await _generateTokenUseCase.ExecuteAsync(new GenerateTokenRequest(user.Id.ToString(), user.Email));

        if (!tokenResult.IsSuccess)
        {
            return BadRequest(new { Message = string.Join(", ", tokenResult.Messages) });
        }

        return Ok(new { token = tokenResult.Response, user });
    }
}
