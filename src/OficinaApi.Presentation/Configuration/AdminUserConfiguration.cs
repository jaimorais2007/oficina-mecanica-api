using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;

namespace OficinaApi.Presentation.Configuration;

public static class AdminUserConfiguration
{
    public static async Task UseAdminUserConfiguration(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            try
            {
                var getAllUsersUseCase = scope.ServiceProvider.GetRequiredService<IUseCase<NoInput, IEnumerable<UserDto>>>();
                var createUserUseCase = scope.ServiceProvider.GetRequiredService<IUseCase<CreateUserDto, UserDto>>();
                
                var usersResponse = await getAllUsersUseCase.ExecuteAsync(new NoInput());
                if (usersResponse.IsSuccess)
                {
                    bool hasAdmin = false;
                    foreach (var u in usersResponse.Response)
                    {
                        if (u.Email == "admin@gmail.com") hasAdmin = true;
                    }

                    if (!hasAdmin)
                    {
                        await createUserUseCase.ExecuteAsync(new CreateUserDto
                        {
                            Name = "Admin Inicial",
                            Email = "admin@gmail.com",
                            Password = "123",
                            Role = "Admin"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Aviso: Não foi possível verificar/criar o usuário Admin inicial. O banco de dados pode estar indisponível ou a tabela Users ainda não foi criada. Detalhe: {ex.Message}");
            }
        }
    }
}
