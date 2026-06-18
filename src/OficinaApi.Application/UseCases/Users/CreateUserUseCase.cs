using System;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Users
{
    public class CreateUserUseCase : IUseCase<CreateUserDto, UserDto>
    {
        private readonly IUserRepository _userRepository;

        public CreateUserUseCase(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UseCaseResponse<UserDto>> ExecuteAsync(CreateUserDto input)
        {
            try
            {
                var existingUser = await _userRepository.GetByEmailAsync(input.Email);
                if (existingUser != null)
                {
                    throw new Exception("E-mail já cadastrado.");
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

                return UseCaseResponse<UserDto>.Success(dto);
            }
            catch (Exception ex)
            {
                return UseCaseResponse<UserDto>.Failure(ex.Message);
            }
        }
    }
}
