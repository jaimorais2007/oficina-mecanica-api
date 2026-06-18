using System;

namespace OficinaApi.Application.DTOs
{
    public record UpdateStockRequest(Guid Id, int Quantity);
}
