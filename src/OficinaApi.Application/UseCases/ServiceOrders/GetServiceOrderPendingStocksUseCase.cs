using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.ServiceOrders
{
    public class GetServiceOrderPendingStocksUseCase : IUseCase<Guid, IEnumerable<ServiceOrderPeddingStockDto>>
    {
        private readonly IServiceOrderRepository _serviceOrderRepository;

        public GetServiceOrderPendingStocksUseCase(IServiceOrderRepository serviceOrderRepository)
        {
            _serviceOrderRepository = serviceOrderRepository;
        }

        public async Task<UseCaseResponse<IEnumerable<ServiceOrderPeddingStockDto>>> ExecuteAsync(Guid input)
        {
            try
            {
                var serviceOrder = await _serviceOrderRepository.GetServiceOrderByIdToGetPeddingStocksAsync(input);
                if (serviceOrder == null) throw new ArgumentException("Ordem de serviço não encontrada.");

                var dtos = serviceOrder.GetPendingStocks().Select(a => new ServiceOrderPeddingStockDto(a));
                return UseCaseResponse<IEnumerable<ServiceOrderPeddingStockDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                return UseCaseResponse<IEnumerable<ServiceOrderPeddingStockDto>>.Failure(ex.Message);
            }
        }
    }
}
