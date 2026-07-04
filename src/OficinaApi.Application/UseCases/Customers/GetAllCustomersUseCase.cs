using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Customers
{
    public class GetAllCustomersUseCase : IUseCase<NoInput, IEnumerable<CustomerDto>>
    {
        private readonly ICustomerRepository _customerRepository;

        public GetAllCustomersUseCase(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<UseCaseResponse<IEnumerable<CustomerDto>>> ExecuteAsync(NoInput input)
        {
            var customers = await _customerRepository.GetAllAsync();
            var dtos = customers.Select(x => new CustomerDto(x));
            return UseCaseResponse<IEnumerable<CustomerDto>>.Success(dtos);
        }
    }
}
