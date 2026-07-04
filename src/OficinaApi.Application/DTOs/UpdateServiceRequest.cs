using System;

namespace OficinaApi.Application.DTOs
{
    public record UpdateServiceRequest(Guid Id, UpdateServiceDto Dto);
}
