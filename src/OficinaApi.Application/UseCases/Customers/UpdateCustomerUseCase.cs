using System;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Customers
{
    public class UpdateCustomerUseCase : IUseCase<UpdateCustomerRequest, CustomerDto>
    {
        private readonly ICustomerRepository _customerRepository;

        public UpdateCustomerUseCase(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<UseCaseResponse<CustomerDto>> ExecuteAsync(UpdateCustomerRequest input)
        {
            try
            {
                var customer = await _customerRepository.GetByIdAsync(input.Id);

                if (customer == null)
                    return UseCaseResponse<CustomerDto>.Failure("Cliente não encontrado.");

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
            catch (Exception ex)
            {
                return UseCaseResponse<CustomerDto>.Failure(ex.Message);
            }
        }
    }
}
