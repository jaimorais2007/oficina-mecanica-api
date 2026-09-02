using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Exceptions;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.ServiceOrders
{
    public class GetServiceOrderPendingStocksUseCase : IUseCase<Guid, IEnumerable<ServiceOrderPeddingStockDto>>
    {
        private readonly IServiceOrderRepository _serviceOrderRepository;
        private readonly ILogger<GetServiceOrderPendingStocksUseCase> _logger;

        public GetServiceOrderPendingStocksUseCase(
            IServiceOrderRepository serviceOrderRepository,
            ILogger<GetServiceOrderPendingStocksUseCase> logger)
        {
            _serviceOrderRepository = serviceOrderRepository;
            _logger = logger;
        }

        public async Task<UseCaseResponse<IEnumerable<ServiceOrderPeddingStockDto>>> ExecuteAsync(Guid input)
        {
            try
            {                
                var serviceOrder = await _serviceOrderRepository.GetServiceOrderByIdToGetPeddingStocksAsync(input);
                if (serviceOrder == null)
                {
                    throw new DomainException("Ordem de serviço não encontrada.");
                }

                var dtos = serviceOrder.GetPendingStocks().Select(a => new ServiceOrderPeddingStockDto(a));
                return UseCaseResponse<IEnumerable<ServiceOrderPeddingStockDto>>.Success(dtos);
            }
            catch (DomainException ex)
            {
                _logger.LogError(ex, "Service Order not found to get pending stocks. Id: {Id}", input);
                return UseCaseResponse<IEnumerable<ServiceOrderPeddingStockDto>>.Failure(ex.Message);
            }
        }
    }
}
