using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;

namespace OficinaApi.Application.Interfaces;

public interface IServiceOrderService
{
    Task<ServiceOrderDto> CreateServiceOrderAsync(CreateServiceOrderDto dto);
    Task<ServiceOrderDto?> GetServiceOrderByIdAsync(Guid id);
}
