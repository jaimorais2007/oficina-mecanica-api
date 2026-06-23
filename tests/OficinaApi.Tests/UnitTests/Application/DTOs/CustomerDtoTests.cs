using OficinaApi.Application.DTOs;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Enums;

namespace OficinaApi.Tests.UnitTests.Application.DTOs;

public static class CustomerDtoTests
{
    public static CustomerDto CreateValid()
    {
        var customer = new Customer(
            "Teste",
            PersonType.Individual,
            "37596555055",
            DateTime.Now,
            "teste@gmail.com"
        );

        return new CustomerDto(customer);
    }

    public static CreateCustomerDto CreateDto()
    {
        return new CreateCustomerDto
        {
            Name = "Teste",
            PersonType = PersonType.Individual,
            Document = "37596555055",
            DateOfBirth = new DateTime(1990, 2, 17),
            Email = "teste@gmail.com"
        };
    }

    public static UpdateCustomerDto UpdateDto()
    {
        return new UpdateCustomerDto
        {
            Name = "Atualizado",
            PersonType = PersonType.Company,
            Document = "55235252000191",
            DateOfBirth = new DateTime(1995, 2, 17),
            Email = "teste@gmail.com"
        };
    }
}