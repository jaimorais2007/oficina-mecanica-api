using OficinaApi.Application.DTOs;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Enums;

namespace OficinaApi.Tests.UnitTests.Application.DTOs;

public static class VehicleDtoTests
{
    public static VehicleDto CreateValid()
    {
        var customer = new Customer(
            "João",
            PersonType.Company,
            "55235252000191",
            new DateTime(1990, 1, 1),
            "teste@gmail.com"
        );

        var vehicle = new Vehicle(
            customer,
            "ABC1234",
            "Toyota",
            "Corolla",
            2020
        );

        return new VehicleDto(vehicle);
    }

    public static CreateVehicleDto CreateDto()
    {
        return new CreateVehicleDto
        {
            CustomerId = Guid.NewGuid(),
            Plate = "ABC1234",
            Brand = "Toyota",
            Model = "Corolla",
            Year = 2020
        };
    }

    public static UpdateVehicleDto UpdateDto()
    {
        return new UpdateVehicleDto
        {
            Plate = "XYZ9999",
            Brand = "Honda",
            Model = "Civic",
            Year = 2022
        };
    }
}