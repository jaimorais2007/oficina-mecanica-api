using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Exceptions;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Vehicles
{
    public class CreateVehicleUseCase : IUseCase<CreateVehicleDto, VehicleDto>
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<CreateVehicleUseCase> _logger;

        public CreateVehicleUseCase(IVehicleRepository vehicleRepository, ICustomerRepository customerRepository, ILogger<CreateVehicleUseCase> logger)
        {
            _vehicleRepository = vehicleRepository;
            _customerRepository = customerRepository;
            _logger = logger;
        }

        public async Task<UseCaseResponse<VehicleDto>> ExecuteAsync(CreateVehicleDto input)
        {
            try
            {
                var customer = await _customerRepository.GetByIdAsync(input.CustomerId);
                if (customer == null)
                {
                    _logger.LogInformation("Failed to create vehicle: Customer {CustomerId} not found.", input.CustomerId);
                    throw new DomainException("Cliente não encontrado.");
                }

                var vehicle = new Vehicle(customer, input.Plate, input.Brand, input.Model, input.Year);

                await _vehicleRepository.AddAsync(vehicle);

                return UseCaseResponse<VehicleDto>.Success(new VehicleDto(vehicle));
            }
            catch (DomainException ex)
            {
                _logger.LogError(ex, "An error occurred while creating a new vehicle for customer {CustomerId}.", input.CustomerId);
                return UseCaseResponse<VehicleDto>.Failure(ex.Message);
            }
        }
    }
}
