using System;

namespace OficinaApi.Application.DTOs
{
    public record UpdateUserRequest(Guid Id, UpdateUserDto Dto);
}
