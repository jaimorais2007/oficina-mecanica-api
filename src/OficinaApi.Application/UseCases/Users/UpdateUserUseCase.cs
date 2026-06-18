using System;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Users
{
    public class UpdateUserUseCase : IUseCase<UpdateUserRequest, bool>
    {
        private readonly IUserRepository _userRepository;

        public UpdateUserUseCase(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UseCaseResponse<bool>> ExecuteAsync(UpdateUserRequest input)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(input.Id);
                if (user == null)
                    throw new Exception("Usuário não encontrado.");

                user.UpdateName(input.Dto.Name);
                user.UpdateRole(input.Dto.Role);

                await _userRepository.UpdateAsync(user);

                return UseCaseResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return UseCaseResponse<bool>.Failure(ex.Message);
            }
        }
    }
}
