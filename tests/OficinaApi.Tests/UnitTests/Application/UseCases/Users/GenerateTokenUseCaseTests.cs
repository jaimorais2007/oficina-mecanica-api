using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.UseCases.Users;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Unit.Tests.Application.UseCases.Users
{
    public class GenerateTokenUseCaseTests
    {
        private readonly GenerateTokenUseCase _sut;

        public GenerateTokenUseCaseTests()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Secret"]           = "chave-de-teste-minimo-32-caracteres!!",
                    ["Jwt:Issuer"]           = "oficina-api",
                    ["Jwt:Audience"]         = "oficina-clientes",
                    ["Jwt:ExpiresInMinutes"] = "60"
                })
                .Build();

            _sut = new GenerateTokenUseCase(config, Mock.Of<ILogger<GenerateTokenUseCase>>());
        }

        [Fact]
        public async Task ExecuteAsync_RetornaStringNaoVazia()
        {
            var result = await _sut.ExecuteAsync(new GenerateTokenRequest(Guid.NewGuid().ToString(), "user@oficina.com"));

            result.IsSuccess.Should().BeTrue();
            result.Response.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task ExecuteAsync_TokenContemEmailNoClaim()
        {
            var email = "user@oficina.com";
            var userId = Guid.NewGuid().ToString();

            var result = await _sut.ExecuteAsync(new GenerateTokenRequest(userId, email));

            result.IsSuccess.Should().BeTrue();
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(result.Response);

            jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == email);
        }

        [Fact]
        public async Task ExecuteAsync_TokenContemSubComUserId()
        {
            var userId = Guid.NewGuid().ToString();

            var result = await _sut.ExecuteAsync(new GenerateTokenRequest(userId, "user@oficina.com"));

            result.IsSuccess.Should().BeTrue();
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(result.Response);

            jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == userId);
        }

        [Fact]
        public async Task ExecuteAsync_TokenTemExpiracaoFutura()
        {
            var result = await _sut.ExecuteAsync(new GenerateTokenRequest(Guid.NewGuid().ToString(), "user@oficina.com"));

            result.IsSuccess.Should().BeTrue();
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(result.Response);

            jwt.ValidTo.Should().BeAfter(DateTime.UtcNow);
        }

        [Fact]
        public async Task ExecuteAsync_IssuerEAudienceCorretos()
        {
            var result = await _sut.ExecuteAsync(new GenerateTokenRequest(Guid.NewGuid().ToString(), "user@oficina.com"));

            result.IsSuccess.Should().BeTrue();
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(result.Response);

            jwt.Issuer.Should().Be("oficina-api");
            jwt.Audiences.Should().Contain("oficina-clientes");
        }
    }
}
