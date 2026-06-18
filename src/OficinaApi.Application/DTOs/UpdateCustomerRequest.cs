using System;

namespace OficinaApi.Application.DTOs
{
    public record UpdateCustomerRequest(Guid Id, UpdateCustomerDto Dto);
}
