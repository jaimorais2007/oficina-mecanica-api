using System;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Exceptions;
using OficinaApi.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace OficinaApi.Application.UseCases.Users
{
    public class DeleteUserUseCase : IUseCase<Guid, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<DeleteUserUseCase> _logger;

        public DeleteUserUseCase(IUserRepository userRepository, ILogger<DeleteUserUseCase> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<UseCaseResponse<bool>> ExecuteAsync(Guid input)
        {
            try
            {
                await _userRepository.DeleteAsync(input);
                _logger.LogInformation("User {UserId} deleted successfully.", input);
                return UseCaseResponse<bool>.Success(true);
            }
            catch (DomainException ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}.", input);
                return UseCaseResponse<bool>.Failure(ex.Message);
            }
        }
    }
}
