using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Exceptions;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Customers
{
    public class DeleteCustomerUseCase : IUseCase<Guid, bool>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<DeleteCustomerUseCase> _logger;

        public DeleteCustomerUseCase(ICustomerRepository customerRepository, ILogger<DeleteCustomerUseCase> logger)
        {
            _customerRepository = customerRepository;
            _logger = logger;
        }

        public async Task<UseCaseResponse<bool>> ExecuteAsync(Guid input)
        {
            try
            {
                await _customerRepository.DeleteAsync(input);
                return UseCaseResponse<bool>.Success(true);
            }
            catch (DomainException ex)
            {
                _logger.LogError(ex, "An error occurred while deleting customer with ID {Id}", input);
                return UseCaseResponse<bool>.Failure(ex.Message);
            }
        }
    }
}
