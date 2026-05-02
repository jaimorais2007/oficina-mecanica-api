using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.Services;

public class PartService : IPartService
{
    private readonly IPartRepository _partRepository;

    public PartService(IPartRepository partRepository)
    {
        _partRepository = partRepository;
    }

    public async Task AddStockAsync(Guid id, int quantity)
    {
        var part = await _partRepository.GetByIdAsync(id);
        if (part == null) throw new KeyNotFoundException($"Peça com ID '{id}' não encontrada.");

        part.AddStock(quantity);
        await _partRepository.UpdateAsync(part);
    }

    public async Task<PartDto> CreatePartAsync(CreatePartDto dto)
    {
        var part = new Part(dto.Name, dto.Code, dto.InitialQuantity, dto.Price);
        await _partRepository.AddAsync(part);

        return new PartDto
        {
            Id = part.Id,
            Name = part.Name,
            Code = part.Code,
            QuantityInStock = part.QuantityInStock,
            Price = part.Price
        };
    }

    public async Task DeletePartAsync(Guid id)
    {
        await _partRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<PartDto>> GetAllPartsAsync()
    {
        var parts = await _partRepository.GetAllAsync();
        return parts.Select(p => new PartDto
        {
            Id = p.Id,
            Name = p.Name,
            Code = p.Code,
            QuantityInStock = p.QuantityInStock,
            Price = p.Price
        });
    }

    public async Task<PartDto?> GetPartByIdAsync(Guid id)
    {
        var part = await _partRepository.GetByIdAsync(id);
        if (part == null) return null;

        return new PartDto
        {
            Id = part.Id,
            Name = part.Name,
            Code = part.Code,
            QuantityInStock = part.QuantityInStock,
            Price = part.Price
        };
    }

    public async Task RemoveStockAsync(Guid id, int quantity)
    {
        var part = await _partRepository.GetByIdAsync(id);
        if (part == null) throw new Exception("Peça não encontrada.");

        part.RemoveStock(quantity);
        await _partRepository.UpdateAsync(part);
    }
}
