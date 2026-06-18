using System;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Users
{
    public class GetUserByIdUseCase : IUseCase<Guid, UserDto?>
    {
        private readonly IUserRepository _userRepository;

        public GetUserByIdUseCase(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UseCaseResponse<UserDto?>> ExecuteAsync(Guid input)
        {
            var user = await _userRepository.GetByIdAsync(input);
            if (user == null) return UseCaseResponse<UserDto?>.Success(null);

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
    }
}
