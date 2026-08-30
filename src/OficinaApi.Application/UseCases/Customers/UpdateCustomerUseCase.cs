using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Exceptions;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Customers
{
    public class UpdateCustomerUseCase : IUseCase<UpdateCustomerRequest, CustomerDto>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<UpdateCustomerUseCase> _logger;

        public UpdateCustomerUseCase(ICustomerRepository customerRepository, ILogger<UpdateCustomerUseCase> logger)
        {
            _customerRepository = customerRepository;
            _logger = logger;
        }

        public async Task<UseCaseResponse<CustomerDto>> ExecuteAsync(UpdateCustomerRequest input)
        {
            try
            {
                var customer = await _customerRepository.GetByIdAsync(input.Id);

                if (customer == null)
                {
                    _logger.LogInformation("Update failed: Customer with ID {Id} was not found.", input.Id);
                    return UseCaseResponse<CustomerDto>.Failure("Cliente não encontrado.");
                }

                customer.Update(
                    input.Dto.Name,
                    input.Dto.PersonType,
                    input.Dto.Document,
                    input.Dto.DateOfBirth,
                    input.Dto.Email
                );

                await _customerRepository.UpdateAsync(customer);

                return UseCaseResponse<CustomerDto>.Success(new CustomerDto(customer));
            }
            catch (DomainException ex)
            {
                _logger.LogError(ex, "An error occurred while updating customer with ID {Id}", input.Id);
                return UseCaseResponse<CustomerDto>.Failure(ex.Message);
            }
        }
    }
}
