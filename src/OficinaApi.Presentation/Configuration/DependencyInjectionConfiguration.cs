using Microsoft.EntityFrameworkCore;
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
using OficinaApi.Infrastructure.Metrics;

namespace OficinaApi.Presentation.Configuration;

public static class DependencyInjectionConfiguration
{
    public static IServiceCollection AddDependencyInjectionConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Configure PostgreSQL Database
        services.AddDbContext<OficinaDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Register Repositories
        services.AddScoped<IPartRepository, PartRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IServiceOrderRepository, ServiceOrderRepository>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<IServiceOrderPartRepository, ServiceOrderPartRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        // Register UseCases
        services.AddScoped<IUseCase<NoInput, IEnumerable<CustomerDto>>, GetAllCustomersUseCase>();
        services.AddScoped<IUseCase<Guid, CustomerDto?>, GetCustomerByIdUseCase>();
        services.AddScoped<IUseCase<CreateCustomerDto, CustomerDto>, CreateCustomerUseCase>();
        services.AddScoped<IUseCase<UpdateCustomerRequest, CustomerDto>, UpdateCustomerUseCase>();
        services.AddScoped<IUseCase<Guid, bool>, DeleteCustomerUseCase>();
        services.AddScoped<IUseCase<Guid, NoInput>, LogicalDeletionCustomerUseCase>();


        services.AddScoped<IUseCase<NoInput, IEnumerable<PartDto>>, GetAllPartsUseCase>();
        services.AddScoped<IUseCase<Guid, PartDto?>, GetPartByIdUseCase>();
        services.AddScoped<IUseCase<CreatePartDto, PartDto>, CreatePartUseCase>();
        services.AddScoped<IUseCase<AddStockRequest, bool>, AddStockUseCase>();
        services.AddScoped<IUseCase<RemoveStockRequest, bool>, RemoveStockUseCase>();
        services.AddScoped<IUseCase<Guid, bool>, DeletePartUseCase>();
        services.AddScoped<IUseCase<PartStockAddedEvent, bool>, PartStockAddedUseCase>();
        services.AddScoped<IUseCase<Guid, NoInput>, LogicalDeletionPartsUseCase>();

        services.AddScoped<IUseCase<NoInput, IEnumerable<ServiceDto>>, GetAllServicesUseCase>();
        services.AddScoped<IUseCase<Guid, ServiceDto?>, GetServiceByIdUseCase>();
        services.AddScoped<IUseCase<CreateServiceDto, ServiceDto>, CreateServiceUseCase>();
        services.AddScoped<IUseCase<UpdateServiceRequest, ServiceDto>, UpdateServiceUseCase>();
        services.AddScoped<IUseCase<Guid, bool>, DeleteServiceUseCase>();
        services.AddScoped<IUseCase<Guid, NoInput>, LogicalDeletionServiceUseCase>();     

        services.AddScoped<IUseCase<NoInput, IEnumerable<ServiceOrderDto>>, GetAllServiceOrdersUseCase>();
        services.AddScoped<IUseCase<Guid, ServiceOrderDto?>, GetServiceOrderByIdUseCase>();
        services.AddScoped<IUseCase<Guid, ServiceOrderStatusDto?>, GetServiceOrderByStatusUseCase>();
        
        services.AddScoped<IUseCase<CreateServiceOrderDto, ServiceOrderDto>, CreateServiceOrderUseCase>();
        services.AddScoped<IUseCase<StartDiagnosticsRequest, ServiceOrderDto>, StartDiagnosticsUseCase>();
        services.AddScoped<IUseCase<FinishAnalysisRequest, ServiceOrderDto>, FinishAnalysisUseCase>();
        services.AddScoped<IUseCase<AddPartToServiceOrderRequest, ServiceOrderDto>, AddPartToServiceOrderUseCase>();
        services.AddScoped<IUseCase<AddServiceToServiceOrderRequest, ServiceOrderDto>, AddServiceToServiceOrderUseCase>();
        services.AddScoped<IUseCase<ApproveServiceOrderRequest, ServiceOrderDto>, ApproveServiceOrderUseCase>();
        services.AddScoped<IUseCase<FinishExecutionRequest, ServiceOrderDto>, FinishExecutionUseCase>();
        services.AddScoped<IUseCase<DeliverServiceOrderRequest, ServiceOrderDto>, DeliverServiceOrderUseCase>();
        services.AddScoped<IUseCase<RefuseServiceOrderRequest, ServiceOrderDto>, RefuseServiceOrderUseCase>();

        services.AddScoped<IUseCase<Guid, IEnumerable<ServiceOrderPeddingStockDto>>, GetServiceOrderPendingStocksUseCase>();
        services.AddScoped<IUseCase<NoInput, double>, GetAverageDurationUseCase>();
        services.AddScoped<IUseCase<ServiceOrderApprovedEvent, bool>, ServiceOrderApprovedUseCase>();

        services.AddScoped<IUseCase<NoInput, IEnumerable<UserDto>>, GetAllUsersUseCase>();
        services.AddScoped<IUseCase<Guid, UserDto?>, GetUserByIdUseCase>();
        services.AddScoped<IUseCase<CreateUserDto, UserDto>, CreateUserUseCase>();
        services.AddScoped<IUseCase<UpdateUserRequest, bool>, UpdateUserUseCase>();
        services.AddScoped<IUseCase<Guid, bool>, DeleteUserUseCase>();
        services.AddScoped<IUseCase<AuthenticateUserRequest, UserDto?>, AuthenticateUserUseCase>();
        services.AddScoped<IUseCase<GenerateTokenRequest, string>, GenerateTokenUseCase>();
        services.AddScoped<IUseCase<Guid, NoInput>, LogicalDeletionUserUseCase>();

        services.AddScoped<IUseCase<NoInput, IEnumerable<VehicleDto?>>, GetAllVehiclesUseCase>();
        services.AddScoped<IUseCase<Guid, VehicleDto?>, GetVehicleByIdUseCase>();
        services.AddScoped<IUseCase<CreateVehicleDto, VehicleDto>, CreateVehicleUseCase>();
        services.AddScoped<IUseCase<UpdateVehicleRequest, VehicleDto>, UpdateVehicleUseCase>();
        services.AddScoped<IUseCase<Guid, bool>, DeleteVehicleUseCase>();
        services.AddScoped<IUseCase<Guid, NoInput>, LogicalDeletionVehicleUseCase>();

        services.AddScoped<IEmailService, EmailService>();


        // Register Domain Event Dispatcher
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        // Register Domain Event Handlers
        services.AddScoped<IDomainEventHandler<ServiceOrderApprovedEvent>, ServiceOrderApprovedEventHandler>();
        services.AddScoped<IDomainEventHandler<ServiceOrderStatusChangedEvent>, ServiceOrderMailEventHandler>();
        services.AddScoped<IDomainEventHandler<PartStockAddedEvent>, PartStockAddedEventHandler>();

        services.AddScoped<IApplicationMetrics, ApplicationMetrics>();

        return services;
    }
}
