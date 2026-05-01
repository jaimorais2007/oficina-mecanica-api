using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var jwtSecret   = builder.Configuration["Jwt:Secret"]!;
var jwtIssuer   = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

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

// Configure PostgreSQL Database
builder.Services.AddDbContext<OficinaApi.Infrastructure.Data.OficinaDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register Repositories
builder.Services.AddScoped<OficinaApi.Domain.Interfaces.IPartRepository, OficinaApi.Infrastructure.Repositories.PartRepository>();
builder.Services.AddScoped<OficinaApi.Domain.Interfaces.ICustomerRepository, OficinaApi.Infrastructure.Repositories.CustomerRepository>();
builder.Services.AddScoped<OficinaApi.Domain.Interfaces.IServiceOrderRepository, OficinaApi.Infrastructure.Repositories.ServiceOrderRepository>();
builder.Services.AddScoped<OficinaApi.Domain.Interfaces.IVehicleRepository, OficinaApi.Infrastructure.Repositories.VehicleRepository>();
builder.Services.AddScoped<OficinaApi.Domain.Interfaces.IServiceRepository, OficinaApi.Infrastructure.Repositories.ServiceRepository>();
builder.Services.AddScoped<OficinaApi.Domain.Interfaces.IUserRepository, OficinaApi.Infrastructure.Repositories.UserRepository>();

// Register Application Services
builder.Services.AddScoped<OficinaApi.Application.Interfaces.IPartService, OficinaApi.Application.Services.PartService>();
builder.Services.AddScoped<OficinaApi.Application.Interfaces.ICustomerService, OficinaApi.Application.Services.CustomerService>();
builder.Services.AddScoped<OficinaApi.Application.Interfaces.IExternalQueryService, OficinaApi.Application.Services.ExternalQueryService>();
builder.Services.AddScoped<OficinaApi.Application.Interfaces.IVehicleService, OficinaApi.Application.Services.VehicleService>();
builder.Services.AddScoped<OficinaApi.Application.Interfaces.IServiceManagementService, OficinaApi.Application.Services.ServiceManagementService>();
builder.Services.AddScoped<OficinaApi.Application.Interfaces.IUserService, OficinaApi.Application.Services.UserService>();

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

using (var scope = app.Services.CreateScope())
{
    var userService = scope.ServiceProvider.GetRequiredService<OficinaApi.Application.Interfaces.IUserService>();
    var users = await userService.GetAllUsersAsync();
    
    // Verifica se não tem nenhum admin@gmail.com
    bool hasAdmin = false;
    foreach (var u in users)
    {
        if (u.Email == "admin@gmail.com") hasAdmin = true;
    }

    if (!hasAdmin)
    {
        await userService.CreateUserAsync(new OficinaApi.Application.DTOs.CreateUserDto
        {
            Name = "Admin Inicial",
            Email = "admin@gmail.com",
            Password = "123",
            Role = "Admin"
        });
    }
}

app.Run();