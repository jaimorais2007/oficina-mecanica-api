using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Customers
{
    public class CreateCustomerUseCase : IUseCase<CreateCustomerDto, CustomerDto>
    {
        private readonly ICustomerRepository _customerRepository;

        public CreateCustomerUseCase(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<UseCaseResponse<CustomerDto>> ExecuteAsync(CreateCustomerDto input)
        {
            var customer = new Customer(
                input.Name,
                input.PersonType,
                input.Document,
                input.DateOfBirth,
                input.Email
            );

            await _customerRepository.AddAsync(customer);

            return UseCaseResponse<CustomerDto>.Success(new CustomerDto(customer));
        }
    }
}
