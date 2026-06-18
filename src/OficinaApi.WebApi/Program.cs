using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OficinaApi.Application.EventHandlers;
using OficinaApi.Application.Interfaces;
using OficinaApi.Application.Services;
using OficinaApi.Application.UseCases.Customers;
using OficinaApi.Application.UseCases.Parts;
using OficinaApi.Application.UseCases.Services;
using OficinaApi.Application.UseCases.ServiceOrders;
using OficinaApi.Application.UseCases.Users;
using OficinaApi.Application.UseCases.Vehicles;
using OficinaApi.Application.DTOs;
using OficinaApi.Domain.Events;
using OficinaApi.Domain.Interfaces;
using OficinaApi.Infrastructure.Data;
using OficinaApi.Infrastructure.Repositories;
using OficinaApi.WebApi.ExceptionFilters;
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
// TokenService is now implemented by GenerateTokenUseCase

// Configure PostgreSQL Database
builder.Services.AddDbContext<OficinaDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register Repositories
builder.Services.AddScoped<IPartRepository, PartRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IServiceOrderRepository, ServiceOrderRepository>();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IServiceOrderPartRepository, ServiceOrderPartRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Register UseCases
builder.Services.AddScoped<IUseCase<NoInput, IEnumerable<CustomerDto>>, GetAllCustomersUseCase>();
builder.Services.AddScoped<IUseCase<Guid, CustomerDto?>, GetCustomerByIdUseCase>();
builder.Services.AddScoped<IUseCase<CreateCustomerDto, CustomerDto>, CreateCustomerUseCase>();
builder.Services.AddScoped<IUseCase<UpdateCustomerRequest, CustomerDto>, UpdateCustomerUseCase>();
builder.Services.AddScoped<IUseCase<Guid, bool>, DeleteCustomerUseCase>();

builder.Services.AddScoped<IUseCase<NoInput, IEnumerable<PartDto>>, GetAllPartsUseCase>();
builder.Services.AddScoped<IUseCase<Guid, PartDto?>, GetPartByIdUseCase>();
builder.Services.AddScoped<IUseCase<CreatePartDto, PartDto>, CreatePartUseCase>();
builder.Services.AddScoped<IUseCase<AddStockRequest, bool>, AddStockUseCase>();
builder.Services.AddScoped<IUseCase<RemoveStockRequest, bool>, RemoveStockUseCase>();
builder.Services.AddScoped<IUseCase<Guid, bool>, DeletePartUseCase>();

builder.Services.AddScoped<IUseCase<NoInput, IEnumerable<ServiceDto>>, GetAllServicesUseCase>();
builder.Services.AddScoped<IUseCase<Guid, ServiceDto?>, GetServiceByIdUseCase>();
builder.Services.AddScoped<IUseCase<CreateServiceDto, ServiceDto>, CreateServiceUseCase>();
builder.Services.AddScoped<IUseCase<UpdateServiceRequest, ServiceDto>, UpdateServiceUseCase>();
builder.Services.AddScoped<IUseCase<Guid, bool>, DeleteServiceUseCase>();

builder.Services.AddScoped<IUseCase<NoInput, IEnumerable<ServiceOrderDto>>, GetAllServiceOrdersUseCase>();
builder.Services.AddScoped<IUseCase<Guid, ServiceOrderDto?>, GetServiceOrderByIdUseCase>();
builder.Services.AddScoped<IUseCase<CreateServiceOrderDto, ServiceOrderDto>, CreateServiceOrderUseCase>();
builder.Services.AddScoped<IUseCase<StartDiagnosticsRequest, ServiceOrderDto>, StartDiagnosticsUseCase>();
builder.Services.AddScoped<IUseCase<FinishAnalysisRequest, ServiceOrderDto>, FinishAnalysisUseCase>();
builder.Services.AddScoped<IUseCase<AddPartToServiceOrderRequest, ServiceOrderDto>, AddPartToServiceOrderUseCase>();
builder.Services.AddScoped<IUseCase<AddServiceToServiceOrderRequest, ServiceOrderDto>, AddServiceToServiceOrderUseCase>();
builder.Services.AddScoped<IUseCase<ApproveServiceOrderRequest, ServiceOrderDto>, ApproveServiceOrderUseCase>();
builder.Services.AddScoped<IUseCase<FinishExecutionRequest, ServiceOrderDto>, FinishExecutionUseCase>();
builder.Services.AddScoped<IUseCase<DeliverServiceOrderRequest, ServiceOrderDto>, DeliverServiceOrderUseCase>();
builder.Services.AddScoped<IUseCase<Guid, IEnumerable<ServiceOrderPeddingStockDto>>, GetServiceOrderPendingStocksUseCase>();
builder.Services.AddScoped<IUseCase<NoInput, double>, GetAverageDurationUseCase>();

builder.Services.AddScoped<IUseCase<NoInput, IEnumerable<UserDto>>, GetAllUsersUseCase>();
builder.Services.AddScoped<IUseCase<Guid, UserDto?>, GetUserByIdUseCase>();
builder.Services.AddScoped<IUseCase<CreateUserDto, UserDto>, CreateUserUseCase>();
builder.Services.AddScoped<IUseCase<UpdateUserRequest, bool>, UpdateUserUseCase>();
builder.Services.AddScoped<IUseCase<Guid, bool>, DeleteUserUseCase>();
builder.Services.AddScoped<IUseCase<AuthenticateUserRequest, UserDto?>, AuthenticateUserUseCase>();
builder.Services.AddScoped<IUseCase<GenerateTokenRequest, string>, GenerateTokenUseCase>();

builder.Services.AddScoped<IUseCase<NoInput, IEnumerable<VehicleDto?>>, GetAllVehiclesUseCase>();
builder.Services.AddScoped<IUseCase<Guid, VehicleDto?>, GetVehicleByIdUseCase>();
builder.Services.AddScoped<IUseCase<CreateVehicleDto, VehicleDto>, CreateVehicleUseCase>();
builder.Services.AddScoped<IUseCase<UpdateVehicleRequest, VehicleDto>, UpdateVehicleUseCase>();
builder.Services.AddScoped<IUseCase<Guid, bool>, DeleteVehicleUseCase>();

builder.Services.AddScoped<IEmailService, EmailService>();


// Register Domain Event Dispatcher
builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

// Register Domain Event Handlers
builder.Services.AddScoped<IDomainEventHandler<ServiceOrderApprovedEvent>, ServiceOrderApprovedEventHandler>();
builder.Services.AddScoped<IDomainEventHandler<PartStockAddedEvent>, PartStockAddedEventHandler>();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
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

using (var scope = app.Services.CreateScope())
{
    try
    {
        var getAllUsersUseCase = scope.ServiceProvider.GetRequiredService<IUseCase<NoInput, IEnumerable<UserDto>>>();
        var createUserUseCase = scope.ServiceProvider.GetRequiredService<IUseCase<CreateUserDto, UserDto>>();
        
        var usersResponse = await getAllUsersUseCase.ExecuteAsync(new NoInput());
        if (usersResponse.IsSuccess)
        {
            bool hasAdmin = false;
            foreach (var u in usersResponse.Response)
            {
                if (u.Email == "admin@gmail.com") hasAdmin = true;
            }

            if (!hasAdmin)
            {
                await createUserUseCase.ExecuteAsync(new CreateUserDto
                {
                    Name = "Admin Inicial",
                    Email = "admin@gmail.com",
                    Password = "123",
                    Role = "Admin"
                });
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Aviso: Não foi possível verificar/criar o usuário Admin inicial. O banco de dados pode estar indisponível ou a tabela Users ainda não foi criada. Detalhe: {ex.Message}");
    }
}

app.Run();
