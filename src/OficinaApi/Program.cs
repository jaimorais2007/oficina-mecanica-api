using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OficinaApi.Application.EventHandlers;
using OficinaApi.Application.Interfaces;
using OficinaApi.Application.Services;
using OficinaApi.Domain.Events;
using OficinaApi.Domain.Interfaces;
using OficinaApi.EventHandlers;
using OficinaApi.Infrastructure.Repositories;
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
builder.Services.AddScoped<IPartRepository, PartRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IServiceOrderRepository, ServiceOrderRepository>();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IServiceOrderPartRepository, ServiceOrderPartRepository>();

// Register Application Services
builder.Services.AddScoped<IPartService, PartService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IServiceOrderService, ServiceOrderService>();
builder.Services.AddScoped<IServiceManagementService, ServiceManagementService>();

// Register Domain Event Dispatcher
builder.Services.AddScoped<IDomainEventDispatcher, OficinaApi.Infrastructure.Data.DomainEventDispatcher>();

// Register Domain Event Handlers
builder.Services.AddScoped<IDomainEventHandler<ServiceOrderApprovedEvent>, ServiceOrderApprovedEventHandler>();
builder.Services.AddScoped<IDomainEventHandler<PartStockAddedEvent>, PartStockAddedEventHandler>();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<OficinaApi.ExceptionFilters.GlobalExceptionFilter>();
});
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