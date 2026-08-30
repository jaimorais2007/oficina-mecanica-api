using System;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Exceptions;
using OficinaApi.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace OficinaApi.Application.UseCases.Users
{
    public class UpdateUserUseCase : IUseCase<UpdateUserRequest, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UpdateUserUseCase> _logger;

        public UpdateUserUseCase(IUserRepository userRepository, ILogger<UpdateUserUseCase> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<UseCaseResponse<bool>> ExecuteAsync(UpdateUserRequest input)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(input.Id);
                if (user == null)
                {
                    _logger.LogInformation("Update failed: User with ID {Id} not found.", input.Id);
                    throw new DomainException("Usuário não encontrado.");
                }

                user.UpdateName(input.Dto.Name);
                user.UpdateRole(input.Dto.Role);

                await _userRepository.UpdateAsync(user);

                _logger.LogInformation("User {Id} updated successfully.", input.Id);
                return UseCaseResponse<bool>.Success(true);
            }
            catch (DomainException ex)
            {
                _logger.LogError(ex, "Error updating user {Id}.", input.Id);
                return UseCaseResponse<bool>.Failure(ex.Message);
            }
        }
    }
}
