using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using OficinaApi.Application.Services;
using Xunit;

namespace Unit.Tests;

public class TokenServiceTests
{
    private readonly TokenService _sut;

    public TokenServiceTests()
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

        _sut = new TokenService(config);
    }

    [Fact]
    public void GerarToken_RetornaStringNaoVazia()
    {
        var token = _sut.GerarToken(Guid.NewGuid().ToString(), "user@oficina.com");

        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GerarToken_TokenContemEmailNoClaim()
    {
        var email = "user@oficina.com";
        var userId = Guid.NewGuid().ToString();

        var token = _sut.GerarToken(userId, email);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == email);
    }

    [Fact]
    public void GerarToken_TokenContemSubComUserId()
    {
        var userId = Guid.NewGuid().ToString();

        var token = _sut.GerarToken(userId, "user@oficina.com");

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == userId);
    }

    [Fact]
    public void GerarToken_TokenTemExpiracaoFutura()
    {
        var token = _sut.GerarToken(Guid.NewGuid().ToString(), "user@oficina.com");

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.ValidTo.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void GerarToken_IssuerEAudienceCorretos()
    {
        var token = _sut.GerarToken(Guid.NewGuid().ToString(), "user@oficina.com");

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Issuer.Should().Be("oficina-api");
        jwt.Audiences.Should().Contain("oficina-clientes");
    }
}
