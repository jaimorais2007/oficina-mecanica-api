using System;

namespace OficinaApi.Application.DTOs
{
    public record AddServiceToServiceOrderRequest(Guid Id, AddServiceDto Dto);
}
