using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Exceptions;
using OficinaApi.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace OficinaApi.Application.UseCases.Users
{
    public class GetAllUsersUseCase : IUseCase<NoInput, IEnumerable<UserDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<GetAllUsersUseCase> _logger;

        public GetAllUsersUseCase(IUserRepository userRepository, ILogger<GetAllUsersUseCase> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<UseCaseResponse<IEnumerable<UserDto>>> ExecuteAsync(NoInput input)
        {
            try
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
                
                _logger.LogInformation("All users retrieved successfully.");
                return UseCaseResponse<IEnumerable<UserDto>>.Success(dtos);
            }
            catch (DomainException ex)
            {
                _logger.LogError(ex, "Error retrieving all users.");
                return UseCaseResponse<IEnumerable<UserDto>>.Failure(ex.Message);
            }
        }
    }
}
