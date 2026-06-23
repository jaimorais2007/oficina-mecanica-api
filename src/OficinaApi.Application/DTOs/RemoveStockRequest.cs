using System;

namespace OficinaApi.Application.DTOs
{
    public record RemoveStockRequest(Guid Id, int Quantity);
}
