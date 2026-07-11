using Microsoft.Extensions.Logging;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Application.UseCases.Vehicles;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Users
{
    public class LogicalDeletionUserUseCase : IUseCase<Guid, NoInput>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<LogicalDeletionVehicleUseCase> _logger;

        public LogicalDeletionUserUseCase(IUserRepository userRepository, ILogger<LogicalDeletionVehicleUseCase> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<UseCaseResponse<NoInput>> ExecuteAsync(Guid input)
        {
            var user = await _userRepository.GetByIdAsync(input);

            if (user == null)
            {
                _logger.LogInformation("User with ID {Id} not found.", input);
                return UseCaseResponse<NoInput>.Success(new NoInput());
            }

            user.SetInactive(!user.Inactive);

            await _userRepository.UpdateAsync(user);
            return UseCaseResponse<NoInput>.Success(new NoInput());
        }
    }
}
