using System;

namespace OficinaApi.Application.DTOs
{
    public record UpdateVehicleRequest(Guid Id, UpdateVehicleDto Dto);
}
