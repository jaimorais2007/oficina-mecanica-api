using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;

namespace OficinaApi.Application.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(Guid id);
    Task<UserDto> CreateUserAsync(CreateUserDto dto);
    Task UpdateUserAsync(Guid id, UpdateUserDto dto);
    Task DeleteUserAsync(Guid id);
    Task<UserDto?> AuthenticateAsync(string email, string password);
}
