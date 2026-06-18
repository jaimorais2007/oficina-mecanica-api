using System;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Users
{
    public class AuthenticateUserUseCase : IUseCase<AuthenticateUserRequest, UserDto?>
    {
        private readonly IUserRepository _userRepository;

        public AuthenticateUserUseCase(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UseCaseResponse<UserDto?>> ExecuteAsync(AuthenticateUserRequest input)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(input.Email);

                if (user == null)
                    return UseCaseResponse<UserDto?>.Success(null);

                bool isValidPassword = BCrypt.Net.BCrypt.Verify(input.Password, user.PasswordHash);

                if (!isValidPassword)
                    return UseCaseResponse<UserDto?>.Success(null);

                var dto = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt
                };

                return UseCaseResponse<UserDto?>.Success(dto);
            }
            catch (Exception ex)
            {
                return UseCaseResponse<UserDto?>.Failure(ex.Message);
            }
        }
    }
}
