using Microsoft.AspNetCore.Mvc;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Interfaces;
using OficinaApi.Infrastructure.Repositories;

namespace OficinaApi.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
        {
            var customers = await _customerRepository.GetAllAsync();
            return customers.Select(x => new CustomerDto(x));
        }

        public async Task<CustomerDto?> GetCustomerByIdAsync(Guid id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null) return null;

            return new CustomerDto(customer);
        }

        public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto)
        {
            var customer = new Customer(
                dto.Name,
                dto.PersonType,
                dto.Document,
                dto.DateOfBirth,
                dto.Email
            );

            await _customerRepository.AddAsync(customer);

            return new CustomerDto(customer);
        }

        public async Task DeleteCustomerAsync(Guid id)
        {
            await _customerRepository.DeleteAsync(id);
        }

        public async Task<CustomerDto> UpdateCustomerAsync(Guid id, UpdateCustomerDto dto)
        {
            var customer = await _customerRepository.GetByIdAsync(id);

            if (customer == null)
                throw new ArgumentException("Cliente não encontrado.");

            customer.Update(
                dto.Name,
                dto.PersonType,
                dto.Document,
                dto.DateOfBirth,
                dto.Email
            );

            await _customerRepository.UpdateAsync(customer);

            return new CustomerDto(customer);
        }
    
    }
}
