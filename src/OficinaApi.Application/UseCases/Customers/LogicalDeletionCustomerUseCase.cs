using Microsoft.Extensions.Logging;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Application.UseCases.Vehicles;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Customers
{
    public class LogicalDeletionCustomerUseCase : IUseCase<Guid, NoInput>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<LogicalDeletionVehicleUseCase> _logger;

        public LogicalDeletionCustomerUseCase(ICustomerRepository customerRepository, ILogger<LogicalDeletionVehicleUseCase> logger)
        {
            _customerRepository = customerRepository;
            _logger = logger;
        }

        public async Task<UseCaseResponse<NoInput>> ExecuteAsync(Guid input)
        {
            var customer = await _customerRepository.GetByIdAsync(input);

            if (customer == null)
            {
                _logger.LogInformation("Customer with ID {Id} was not found.", input);
                return UseCaseResponse<NoInput>.Success(new NoInput());
            }

            customer.SetInactive(!customer.Inactive);

            await _customerRepository.UpdateAsync(customer);
            return UseCaseResponse<NoInput>.Success(new NoInput());
        }
    }
}
