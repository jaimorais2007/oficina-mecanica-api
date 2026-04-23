using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var jwtSecret   = builder.Configuration["Jwt:Secret"]!;
var jwtIssuer   = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtIssuer,
            ValidAudience            = jwtAudience,
            IssuerSigningKey         = new SymmetricSecurityKey(
                                         Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddScoped<OficinaApi.Services.TokenService>();

// Configure In-Memory Database for testing purposes locally
builder.Services.AddDbContext<OficinaApi.Infrastructure.Data.OficinaDbContext>(options =>
    options.UseInMemoryDatabase("OficinaDbLocal"));

// Register Repositories
builder.Services.AddScoped<OficinaApi.Domain.Interfaces.IPartRepository, OficinaApi.Infrastructure.Repositories.PartRepository>();
builder.Services.AddScoped<OficinaApi.Domain.Interfaces.IServiceOrderRepository, OficinaApi.Infrastructure.Repositories.ServiceOrderRepository>();

// Register Application Services
builder.Services.AddScoped<OficinaApi.Application.Interfaces.IPartService, OficinaApi.Application.Services.PartService>();
builder.Services.AddScoped<OficinaApi.Application.Interfaces.IExternalQueryService, OficinaApi.Application.Services.ExternalQueryService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();