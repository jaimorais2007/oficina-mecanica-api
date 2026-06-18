using System;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Customers
{
    public class DeleteCustomerUseCase : IUseCase<Guid, bool>
    {
        private readonly ICustomerRepository _customerRepository;

        public DeleteCustomerUseCase(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<UseCaseResponse<bool>> ExecuteAsync(Guid input)
        {
            try
            {
                await _customerRepository.DeleteAsync(input);
                return UseCaseResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return UseCaseResponse<bool>.Failure(ex.Message);
            }
        }
    }
}
