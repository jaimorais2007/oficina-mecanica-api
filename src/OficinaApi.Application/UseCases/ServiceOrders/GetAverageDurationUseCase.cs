using System;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.ServiceOrders
{
    public class GetAverageDurationUseCase : IUseCase<NoInput, double>
    {
        private readonly IServiceOrderRepository _serviceOrderRepository;

        public GetAverageDurationUseCase(IServiceOrderRepository serviceOrderRepository)
        {
            _serviceOrderRepository = serviceOrderRepository;
        }

        public async Task<UseCaseResponse<double>> ExecuteAsync(NoInput input)
        {
            try
            {
                var average = await _serviceOrderRepository.GetAverageDurationInDaysAsync();
                return UseCaseResponse<double>.Success(average);
            }
            catch (Exception ex)
            {
                return UseCaseResponse<double>.Failure(ex.Message);
            }
        }
    }
}
