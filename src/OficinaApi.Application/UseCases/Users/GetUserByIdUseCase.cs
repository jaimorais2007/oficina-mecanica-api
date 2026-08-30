using System;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Exceptions;
using OficinaApi.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace OficinaApi.Application.UseCases.Users
{
    public class GetUserByIdUseCase : IUseCase<Guid, UserDto?>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<GetUserByIdUseCase> _logger;

        public GetUserByIdUseCase(IUserRepository userRepository, ILogger<GetUserByIdUseCase> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<UseCaseResponse<UserDto?>> ExecuteAsync(Guid input)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(input);
                if (user == null)
                {
                    _logger.LogInformation("User with ID {Id} not found.", input);
                    return UseCaseResponse<UserDto?>.Success(null);
                }

                var dto = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt
                };
                
                _logger.LogInformation("User {Id} retrieved successfully.", input);
                return UseCaseResponse<UserDto?>.Success(dto);
            }
            catch (DomainException ex)
            {
                _logger.LogError(ex, "Error retrieving user {Id}.", input);
                return UseCaseResponse<UserDto?>.Failure(ex.Message);
            }
        }
    }
}
