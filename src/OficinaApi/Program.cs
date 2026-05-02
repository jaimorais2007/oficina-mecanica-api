using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OficinaApi.Application.EventHandlers;
using OficinaApi.Application.Services;
using OficinaApi.Domain.Events;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var jwtSecret   = builder.Configuration["Jwt:Secret"]!;
var jwtIssuer   = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

var connectionString = builder.Configuration.GetConnectionString("Default");

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
builder.Services.AddScoped<TokenService>();

// Configure In-Memory Database for testing purposes locally
//builder.Services.AddDbContext<OficinaApi.Infrastructure.Data.OficinaDbContext>(options =>
//    options.UseInMemoryDatabase("OficinaDbLocal"));

builder.Services.AddDbContext<OficinaApi.Infrastructure.Data.OficinaDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register Repositories
builder.Services.AddScoped<OficinaApi.Domain.Interfaces.IPartRepository, OficinaApi.Infrastructure.Repositories.PartRepository>();
builder.Services.AddScoped<OficinaApi.Domain.Interfaces.ICustomerRepository, OficinaApi.Infrastructure.Repositories.CustomerRepository>();
builder.Services.AddScoped<OficinaApi.Domain.Interfaces.IServiceOrderRepository, OficinaApi.Infrastructure.Repositories.ServiceOrderRepository>();
builder.Services.AddScoped<OficinaApi.Domain.Interfaces.IVehicleRepository, OficinaApi.Infrastructure.Repositories.VehicleRepository>();
builder.Services.AddScoped<OficinaApi.Domain.Interfaces.IServiceRepository, OficinaApi.Infrastructure.Repositories.ServiceRepository>();

// Register Application Services
builder.Services.AddScoped<OficinaApi.Application.Interfaces.IPartService, PartService>();
builder.Services.AddScoped<OficinaApi.Application.Interfaces.ICustomerService, CustomerService>();
builder.Services.AddScoped<OficinaApi.Application.Interfaces.IVehicleService, VehicleService>();
builder.Services.AddScoped<OficinaApi.Application.Interfaces.IServiceOrderService, ServiceOrderService>();
builder.Services.AddScoped<OficinaApi.Application.Interfaces.IServiceManagementService, ServiceManagementService>();

// Register Domain Event Handlers
builder.Services.AddScoped<OficinaApi.Application.Interfaces.IDomainEventHandler<ServiceOrderApprovedEvent>, ServiceOrderApprovedEventHandler>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.EnableAnnotations();
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        Name        = "Authorization",
        Type        = SecuritySchemeType.ApiKey,
        Scheme      = "Bearer",
        BearerFormat = "JWT",
        In          = ParameterLocation.Header,
        Description = "Informe o token no formato: Bearer {seu_token}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();