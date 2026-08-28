using CucineCRM.Application.Common;
using CucineCRM.Application.DTOs;
using CucineCRM.Application.Interfaces;
using CucineCRM.Application.Services;
using CucineCRM.Domain.Entities;
using CucineCRM.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace CucineCRM.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IRepository<Utente>> _utentiRepo = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IJwtTokenGenerator> _jwtGenerator = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _unitOfWork.Setup(u => u.Utenti).Returns(_utentiRepo.Object);
        _sut = new AuthService(_unitOfWork.Object, _passwordHasher.Object, _jwtGenerator.Object);
    }

    [Fact]
    public async Task Login_CredenzialiValide_RestituisceToken()
    {
        // Arrange
        var utente = new Utente
        {
            Id = 1,
            Email = "mario.rossi@cucine.it",
            PasswordHash = "hash_valido",
            Ruolo = RuoloUtente.Agente,
            Attivo = true
        };

        _utentiRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Utente, bool>>>(), default))
            .ReturnsAsync(new List<Utente> { utente });

        _passwordHasher.Setup(h => h.Verify("password123", "hash_valido")).Returns(true);
        _jwtGenerator.Setup(j => j.GenerateToken(utente)).Returns("token.jwt.finto");

        // Act
        var result = await _sut.LoginAsync(new LoginRequestDto("mario.rossi@cucine.it", "password123"));

        // Assert
        result.Token.Should().Be("token.jwt.finto");
        result.Utente.Email.Should().Be("mario.rossi@cucine.it");
    }

    [Fact]
    public async Task Login_PasswordErrata_LanciaAuthenticationException()
    {
        var utente = new Utente { Id = 1, Email = "a@b.it", PasswordHash = "hash", Attivo = true };

        _utentiRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Utente, bool>>>(), default))
            .ReturnsAsync(new List<Utente> { utente });
        _passwordHasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var act = async () => await _sut.LoginAsync(new LoginRequestDto("a@b.it", "wrong"));

        await act.Should().ThrowAsync<AuthenticationException>();
    }

    [Fact]
    public async Task Login_UtenteDisattivato_LanciaAuthenticationException()
    {
        var utente = new Utente { Id = 1, Email = "a@b.it", PasswordHash = "hash", Attivo = false };

        _utentiRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Utente, bool>>>(), default))
            .ReturnsAsync(new List<Utente> { utente });
        _passwordHasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        var act = async () => await _sut.LoginAsync(new LoginRequestDto("a@b.it", "password"));

        await act.Should().ThrowAsync<AuthenticationException>()
            .WithMessage("*disattivato*");
    }
}
