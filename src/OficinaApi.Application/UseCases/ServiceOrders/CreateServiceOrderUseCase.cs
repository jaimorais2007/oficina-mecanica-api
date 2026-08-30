using Microsoft.Extensions.Logging;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.ServiceOrders
{
    public class CreateServiceOrderUseCase : IUseCase<CreateServiceOrderDto, ServiceOrderDto>
    {
        private readonly IServiceOrderRepository _serviceOrderRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<CreateServiceOrderUseCase> _logger;

        public CreateServiceOrderUseCase(
            IServiceOrderRepository serviceOrderRepository,
            IVehicleRepository vehicleRepository,
            IServiceRepository serviceRepository,
            ICustomerRepository customerRepository,
            ILogger<CreateServiceOrderUseCase> logger)
        {
            _serviceOrderRepository = serviceOrderRepository;
            _vehicleRepository = vehicleRepository;
            _serviceRepository = serviceRepository;
            _customerRepository = customerRepository;
            _logger = logger;
        }

        public async Task<UseCaseResponse<ServiceOrderDto>> ExecuteAsync(CreateServiceOrderDto input)
        {
            try
            {
                if (input.VehicleId == Guid.Empty)
                {
                    _logger.LogInformation("VehicleId is empty.");
                    throw new ArgumentException("Veículo da Ordem de Serviço não informado.");
                }
                
                if (input.ServicesUsed == null || !input.ServicesUsed.Any())
                {
                    _logger.LogInformation("No services provided for the service order.");
                    throw new ArgumentException("Serviços que serão feitos não foram informados.");
                }

                var existingVehicle = await _vehicleRepository.GetByIdAsync(input.VehicleId);
                if (existingVehicle is null)
                {
                    _logger.LogInformation("Vehicle not found. VehicleId: {VehicleId}", input.VehicleId);
                    throw new ArgumentException("Veículo não encontrado.");
                }

                var existingCustomer = await _customerRepository.GetByIdAsync(input.CustomerId);
                if (existingCustomer is null)
                {
                    _logger.LogInformation("Customer not found. CustomerId: {CustomerId}", input.CustomerId);
                    throw new ArgumentException("Cliente não encontrado.");
                }

                var servicesFounds = await _serviceRepository.GetByIdListAsync(input.ServicesUsed);
                if (servicesFounds.Count() != input.ServicesUsed.Count())
                {
                    _logger.LogInformation("One or more services not found. Expected: {Expected}, Found: {Found}", input.ServicesUsed.Count(), servicesFounds.Count());
                    throw new ArgumentException("Algum dos serviços informados não foi encontrado.");
                }

                var serviceOrder = new ServiceOrder(existingCustomer, existingVehicle, servicesFounds);
                await _serviceOrderRepository.AddAsync(serviceOrder);

                _logger.LogInformation("Service Order successfully created. Id: {ServiceOrder}", serviceOrder.Id);
                return UseCaseResponse<ServiceOrderDto>.Success(new ServiceOrderDto(serviceOrder));
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                _logger.LogError(ex, "Error creating service order");
                return UseCaseResponse<ServiceOrderDto>.Failure(ex.Message);
            }
        }
    }
}
