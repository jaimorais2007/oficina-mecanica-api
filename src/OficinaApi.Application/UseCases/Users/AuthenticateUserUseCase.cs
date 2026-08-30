using System;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Exceptions;
using OficinaApi.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace OficinaApi.Application.UseCases.Users
{
    public class AuthenticateUserUseCase : IUseCase<AuthenticateUserRequest, UserDto?>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AuthenticateUserUseCase> _logger;

        public AuthenticateUserUseCase(IUserRepository userRepository, ILogger<AuthenticateUserUseCase> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<UseCaseResponse<UserDto?>> ExecuteAsync(AuthenticateUserRequest input)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(input.Email);

                if (user == null)
                {
                    _logger.LogInformation("Authentication failed: User with email {Email} not found.", input.Email);
                    return UseCaseResponse<UserDto?>.Success(null);
                }

                bool isValidPassword = BCrypt.Net.BCrypt.Verify(input.Password, user.PasswordHash);

                if (!isValidPassword)
                {
                    _logger.LogInformation("Authentication failed: Invalid password for user {Email}.", input.Email);
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

                _logger.LogInformation("User {Email} authenticated successfully.", input.Email);
                return UseCaseResponse<UserDto?>.Success(dto);
            }
            catch (DomainException ex)
            {
                _logger.LogError(ex, "Error authenticating user {Email}.", input.Email);
                return UseCaseResponse<UserDto?>.Failure(ex.Message);
            }
        }
    }
}
