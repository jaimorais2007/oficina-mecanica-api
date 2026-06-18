using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;

namespace OficinaApi.Application.Interfaces;

public interface IPartService
{
    Task<PartDto> CreatePartAsync(CreatePartDto dto);
    Task<PartDto?> GetPartByIdAsync(Guid id);
    Task<IEnumerable<PartDto>> GetAllPartsAsync();
    Task AddStockAsync(Guid id, int quantity);
    Task RemoveStockAsync(Guid id, int quantity);
    Task DeletePartAsync(Guid id);
}
