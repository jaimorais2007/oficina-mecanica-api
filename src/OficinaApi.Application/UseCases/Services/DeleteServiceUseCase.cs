using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Exceptions;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Services
{
    public class DeleteServiceUseCase : IUseCase<Guid, bool>
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly ILogger<DeleteServiceUseCase> _logger;

        public DeleteServiceUseCase(IServiceRepository serviceRepository, ILogger<DeleteServiceUseCase> logger)
        {
            _serviceRepository = serviceRepository;
            _logger = logger;
        }

        public async Task<UseCaseResponse<bool>> ExecuteAsync(Guid input)
        {
            try
            {
                await _serviceRepository.DeleteAsync(input);
                return UseCaseResponse<bool>.Success(true);
            }
            catch (DomainException ex)
            {
                _logger.LogError(ex, "Error deleting service with ID {ServiceId}", input);
                return UseCaseResponse<bool>.Failure(ex.Message);
            }
        }
    }
}
