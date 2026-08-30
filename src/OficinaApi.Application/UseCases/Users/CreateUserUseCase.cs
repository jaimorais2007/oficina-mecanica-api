using System;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Exceptions;
using OficinaApi.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace OficinaApi.Application.UseCases.Users
{
    public class CreateUserUseCase : IUseCase<CreateUserDto, UserDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<CreateUserUseCase> _logger;

        public CreateUserUseCase(IUserRepository userRepository, ILogger<CreateUserUseCase> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<UseCaseResponse<UserDto>> ExecuteAsync(CreateUserDto input)
        {
            try
            {
                var existingUser = await _userRepository.GetByEmailAsync(input.Email);
                if (existingUser != null)
                {
                    _logger.LogInformation("Creation failed: User with email {Email} already exists.", input.Email);
                    throw new DomainException("E-mail já cadastrado.");
                }

                string passwordHash = BCrypt.Net.BCrypt.HashPassword(input.Password);

                var user = new User(input.Name, input.Email, passwordHash, input.Role);

                await _userRepository.AddAsync(user);

                var dto = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt
                };

                _logger.LogInformation("User {Email} created successfully.", input.Email);
                return UseCaseResponse<UserDto>.Success(dto);
            }
            catch (DomainException ex)
            {
                _logger.LogError(ex, "Error creating user {Email}.", input.Email);
                return UseCaseResponse<UserDto>.Failure(ex.Message);
            }
        }
    }
}
