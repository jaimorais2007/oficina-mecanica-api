using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Users
{
    public class GetAllUsersUseCase : IUseCase<NoInput, IEnumerable<UserDto>>
    {
        private readonly IUserRepository _userRepository;

        public GetAllUsersUseCase(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UseCaseResponse<IEnumerable<UserDto>>> ExecuteAsync(NoInput input)
        {
            var users = await _userRepository.GetAllAsync();
            var dtos = users.Select(user => new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            });
            return UseCaseResponse<IEnumerable<UserDto>>.Success(dtos);
        }
    }
}
