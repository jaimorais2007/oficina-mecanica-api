using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.UseCases.Users;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Interfaces;
using Xunit;
using Microsoft.Extensions.Logging;

namespace Unit.Tests.Application.UseCases.Users
{
    public class UserUseCaseTests
    {
        private readonly Mock<IUserRepository> _repoMock;

        public UserUseCaseTests()
        {
            _repoMock = new Mock<IUserRepository>();
        }

        private static User BuildUser(string email = "user@oficina.com")
        {
            string hash = BCrypt.Net.BCrypt.HashPassword("senha123");
            return new User("João Silva", email, hash, "Admin");
        }

        [Fact]
        public async Task GetAllUsersUseCase_RetornaListaDtos()
        {
            _repoMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<User> { BuildUser() });

            var useCase = new GetAllUsersUseCase(_repoMock.Object, Mock.Of<ILogger<GetAllUsersUseCase>>());
            var result = await useCase.ExecuteAsync(new NoInput());

            result.IsSuccess.Should().BeTrue();
            result.Response.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetUserByIdUseCase_UsuarioExistente_RetornaDto()
        {
            var user = BuildUser();
            _repoMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

            var useCase = new GetUserByIdUseCase(_repoMock.Object, Mock.Of<ILogger<GetUserByIdUseCase>>());
            var result = await useCase.ExecuteAsync(user.Id);

            result.IsSuccess.Should().BeTrue();
            result.Response.Should().NotBeNull();
            result.Response!.Email.Should().Be(user.Email);
        }

        [Fact]
        public async Task GetUserByIdUseCase_UsuarioInexistente_RetornaNull()
        {
            _repoMock
                .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((User?)null);

            var useCase = new GetUserByIdUseCase(_repoMock.Object, Mock.Of<ILogger<GetUserByIdUseCase>>());
            var result = await useCase.ExecuteAsync(Guid.NewGuid());

            result.IsSuccess.Should().BeTrue();
            result.Response.Should().BeNull();
        }

        [Fact]
        public async Task CreateUserUseCase_EmailNovo_CriaERetornaDto()
        {
            _repoMock
                .Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            _repoMock
                .Setup(r => r.AddAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            var dto = new CreateUserDto
            {
                Name     = "Maria",
                Email    = "maria@oficina.com",
                Password = "senha123",
                Role     = "User"
            };

            var useCase = new CreateUserUseCase(_repoMock.Object, Mock.Of<ILogger<CreateUserUseCase>>());
            var result = await useCase.ExecuteAsync(dto);

            result.IsSuccess.Should().BeTrue();
            result.Response.Should().NotBeNull();
            result.Response.Email.Should().Be("maria@oficina.com");
            _repoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task CreateUserUseCase_EmailDuplicado_RetornaFailure()
        {
            var existingUser = BuildUser("duplicado@oficina.com");
            _repoMock
                .Setup(r => r.GetByEmailAsync("duplicado@oficina.com"))
                .ReturnsAsync(existingUser);

            var dto = new CreateUserDto
            {
                Name     = "Clone",
                Email    = "duplicado@oficina.com",
                Password = "senha123",
                Role     = "User"
            };

            var useCase = new CreateUserUseCase(_repoMock.Object, Mock.Of<ILogger<CreateUserUseCase>>());
            var result = await useCase.ExecuteAsync(dto);

            result.IsSuccess.Should().BeFalse();
            result.Messages.Should().Contain("E-mail já cadastrado.");
        }

        [Fact]
        public async Task UpdateUserUseCase_UsuarioExistente_AtualizaEChama()
        {
            var user = BuildUser();
            _repoMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
            _repoMock.Setup(r => r.UpdateAsync(user)).Returns(Task.CompletedTask);

            var dto = new UpdateUserDto { Name = "Novo Nome", Role = "User" };

            var useCase = new UpdateUserUseCase(_repoMock.Object, Mock.Of<ILogger<UpdateUserUseCase>>());
            var result = await useCase.ExecuteAsync(new UpdateUserRequest(user.Id, dto));

            result.IsSuccess.Should().BeTrue();
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task UpdateUserUseCase_UsuarioInexistente_RetornaFailure()
        {
            _repoMock
                .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((User?)null);

            var dto = new UpdateUserDto { Name = "Nome", Role = "User" };

            var useCase = new UpdateUserUseCase(_repoMock.Object, Mock.Of<ILogger<UpdateUserUseCase>>());
            var result = await useCase.ExecuteAsync(new UpdateUserRequest(Guid.NewGuid(), dto));

            result.IsSuccess.Should().BeFalse();
            result.Messages.Should().Contain("Usuário não encontrado.");
        }

        [Fact]
        public async Task DeleteUserUseCase_ChamaRepositorio()
        {
            var id = Guid.NewGuid();
            _repoMock.Setup(r => r.DeleteAsync(id)).Returns(Task.CompletedTask);

            var useCase = new DeleteUserUseCase(_repoMock.Object, Mock.Of<ILogger<DeleteUserUseCase>>());
            var result = await useCase.ExecuteAsync(id);

            result.IsSuccess.Should().BeTrue();
            _repoMock.Verify(r => r.DeleteAsync(id), Times.Once);
        }

        [Fact]
        public async Task AuthenticateUserUseCase_CredenciaisValidas_RetornaDto()
        {
            var user = BuildUser("auth@oficina.com");
            _repoMock.Setup(r => r.GetByEmailAsync("auth@oficina.com")).ReturnsAsync(user);

            var useCase = new AuthenticateUserUseCase(_repoMock.Object, Mock.Of<ILogger<AuthenticateUserUseCase>>());
            var result = await useCase.ExecuteAsync(new AuthenticateUserRequest("auth@oficina.com", "senha123"));

            result.IsSuccess.Should().BeTrue();
            result.Response.Should().NotBeNull();
            result.Response!.Email.Should().Be("auth@oficina.com");
        }

        [Fact]
        public async Task AuthenticateUserUseCase_SenhaErrada_RetornaNull()
        {
            var user = BuildUser("auth@oficina.com");
            _repoMock.Setup(r => r.GetByEmailAsync("auth@oficina.com")).ReturnsAsync(user);

            var useCase = new AuthenticateUserUseCase(_repoMock.Object, Mock.Of<ILogger<AuthenticateUserUseCase>>());
            var result = await useCase.ExecuteAsync(new AuthenticateUserRequest("auth@oficina.com", "senha_errada"));

            result.IsSuccess.Should().BeTrue();
            result.Response.Should().BeNull();
        }

        [Fact]
        public async Task AuthenticateUserUseCase_UsuarioInexistente_RetornaNull()
        {
            _repoMock
                .Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            var useCase = new AuthenticateUserUseCase(_repoMock.Object, Mock.Of<ILogger<AuthenticateUserUseCase>>());
            var result = await useCase.ExecuteAsync(new AuthenticateUserRequest("naoexiste@oficina.com", "senha123"));

            result.IsSuccess.Should().BeTrue();
            result.Response.Should().BeNull();
        }
    }
}
