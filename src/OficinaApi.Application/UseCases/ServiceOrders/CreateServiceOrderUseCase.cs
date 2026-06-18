using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
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

        public CreateServiceOrderUseCase(
            IServiceOrderRepository serviceOrderRepository,
            IVehicleRepository vehicleRepository,
            IServiceRepository serviceRepository,
            ICustomerRepository customerRepository)
        {
            _serviceOrderRepository = serviceOrderRepository;
            _vehicleRepository = vehicleRepository;
            _serviceRepository = serviceRepository;
            _customerRepository = customerRepository;
        }

        public async Task<UseCaseResponse<ServiceOrderDto>> ExecuteAsync(CreateServiceOrderDto input)
        {
            try
            {
                if (input.VehicleId == Guid.Empty) throw new ArgumentException("Veículo da Ordem de Serviço não informado.");
                if (input.ServicesUsed.IsNullOrEmpty()) throw new ArgumentException("Serviços que serão feitos não foram informados.");

                var existingVehicle = await _vehicleRepository.GetByIdAsync(input.VehicleId);
                if (existingVehicle is null)
                    throw new ArgumentException("Veículo não encontrado.");

                var existingCustomer = await _customerRepository.GetByIdAsync(input.CustomerId);
                if (existingCustomer is null)
                    throw new ArgumentException("Cliente não encontrado.");

                var servicesFounds = await _serviceRepository.GetByIdListAsync(input.ServicesUsed);
                if (servicesFounds.Count() != input.ServicesUsed.Count())
                    throw new ArgumentException("Algum dos serviços informados não foi encontrado.");

                var serviceOrder = new ServiceOrder(existingCustomer, existingVehicle, servicesFounds);
                await _serviceOrderRepository.AddAsync(serviceOrder);

                return UseCaseResponse<ServiceOrderDto>.Success(new ServiceOrderDto(serviceOrder));
            }
            catch (Exception ex)
            {
                return UseCaseResponse<ServiceOrderDto>.Failure(ex.Message);
            }
        }
    }
}
