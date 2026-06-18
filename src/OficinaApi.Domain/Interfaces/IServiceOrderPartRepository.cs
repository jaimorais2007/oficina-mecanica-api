using System.Collections.Generic;
using System.Threading.Tasks;
using OficinaApi.Domain.Entities;

namespace OficinaApi.Domain.Interfaces;

public interface IServiceOrderPartRepository
{
    Task UpdateRangeAsync(IEnumerable<ServiceOrderPart> serviceOrderParts);
}
