using System;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Customers
{
    public class GetCustomerByIdUseCase : IUseCase<Guid, CustomerDto?>
    {
        private readonly ICustomerRepository _customerRepository;

        public GetCustomerByIdUseCase(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<UseCaseResponse<CustomerDto?>> ExecuteAsync(Guid input)
        {
            var customer = await _customerRepository.GetByIdAsync(input);
            if (customer == null)
            {
                return UseCaseResponse<CustomerDto?>.Success(null);
            }
            return UseCaseResponse<CustomerDto?>.Success(new CustomerDto(customer));
        }
    }
}
