using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Application.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace OficinaApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly TokenService _tokenService;
    private readonly IUserService _userService;

    public AuthController(TokenService tokenService, IUserService userService)
    {
        _tokenService = tokenService;
        _userService = userService;
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

        // Validate user in the database
        var user = await _userService.AuthenticateAsync(dto.Email, dto.Password);

        if (user == null)
        {
            return Unauthorized(new { Message = "E-mail ou senha incorretos." });
        }

        // Generate token with valid User ID and Email
        var token = _tokenService.GerarToken(user.Id.ToString(), user.Email);
        
        return Ok(new { token, user });
    }
}
