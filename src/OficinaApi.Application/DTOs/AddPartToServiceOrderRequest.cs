using System;

namespace OficinaApi.Application.DTOs
{
    public record AddPartToServiceOrderRequest(Guid Id, AddPartDto Dto);
}
