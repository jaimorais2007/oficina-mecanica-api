using System;

namespace OficinaApi.Application.DTOs
{
    public record AddStockRequest(Guid Id, int Quantity);
}
