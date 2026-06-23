using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Customers
{
    public class GetCustomerByIdUseCase : IUseCase<Guid, CustomerDto?>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<GetCustomerByIdUseCase> _logger;

        public GetCustomerByIdUseCase(ICustomerRepository customerRepository, ILogger<GetCustomerByIdUseCase> logger)
        {
            _customerRepository = customerRepository;
            _logger = logger;
        }

        public async Task<UseCaseResponse<CustomerDto?>> ExecuteAsync(Guid input)
        {
            var customer = await _customerRepository.GetByIdAsync(input);
            if (customer == null)
            {
                _logger.LogInformation("Customer with ID {Id} was not found.", input);
                return UseCaseResponse<CustomerDto?>.Success(null);
            }
            return UseCaseResponse<CustomerDto?>.Success(new CustomerDto(customer));
        }
    }
}
