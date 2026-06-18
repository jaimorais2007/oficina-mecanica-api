using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;

namespace OficinaApi.Application.UseCases.Users
{
    public class GenerateTokenUseCase : IUseCase<GenerateTokenRequest, string>
    {
        private readonly IConfiguration _config;

        public GenerateTokenUseCase(IConfiguration config)
        {
            _config = config;
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
                return UseCaseResponse<string>.Success(tokenStr);
            }
            catch (Exception ex)
            {
                return UseCaseResponse<string>.Failure(ex.Message);
            }
        }
    }
}
