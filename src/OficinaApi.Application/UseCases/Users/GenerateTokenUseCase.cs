using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace OficinaApi.Application.UseCases.Users
{
    public class GenerateTokenUseCase : IUseCase<GenerateTokenRequest, string>
    {
        private readonly IConfiguration _config;
        private readonly ILogger<GenerateTokenUseCase> _logger;

        public GenerateTokenUseCase(IConfiguration config, ILogger<GenerateTokenUseCase> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<UseCaseResponse<string>> ExecuteAsync(GenerateTokenRequest input)
        {
            try
            {
                var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_config["Jwt:Secret"]!));

                var creds = new SigningCredentials(
                    key, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, input.UserId),
                    new Claim(JwtRegisteredClaimNames.Email, input.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                var expira = int.Parse(
                    _config["Jwt:ExpiresInMinutes"] ?? "60");

                var token = new JwtSecurityToken(
                    issuer:             _config["Jwt:Issuer"],
                    audience:           _config["Jwt:Audience"],
                    claims:             claims,
                    expires:            DateTime.UtcNow.AddMinutes(expira),
                    signingCredentials: creds);

                var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
                _logger.LogInformation("Token generated successfully for user {UserId} ({Email}).", input.UserId, input.Email);
                return UseCaseResponse<string>.Success(tokenStr);
            }
            catch (DomainException ex)
            {
                _logger.LogError(ex, "Error generating token for user {UserId} ({Email}).", input.UserId, input.Email);
                return UseCaseResponse<string>.Failure(ex.Message);
            }
        }
    }
}
