using CucineCRM.Application.Interfaces;
using CucineCRM.Application.Services;
using CucineCRM.Domain.Entities;
using CucineCRM.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace CucineCRM.UnitTests.Services;

public class DataScopingServiceTests
{
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IRepository<Agente>> _agentiRepo = new();
    private readonly DataScopingService _sut;

    public DataScopingServiceTests()
    {
        _unitOfWork.Setup(u => u.Agenti).Returns(_agentiRepo.Object);
        _sut = new DataScopingService(_currentUser.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task DirettoreCommerciale_VedeTutto_NessunFiltro()
    {
        _currentUser.Setup(u => u.IsAuthenticated).Returns(true);
        _currentUser.Setup(u => u.Ruolo).Returns(RuoloUtente.DirettoreCommerciale);

        var result = await _sut.GetAgentiVisibiliAsync();

        result.Should().BeNull(); // null = nessun filtro applicato
    }

    [Fact]
    public async Task Agente_VedeSoloSeStesso()
    {
        _currentUser.Setup(u => u.IsAuthenticated).Returns(true);
        _currentUser.Setup(u => u.Ruolo).Returns(RuoloUtente.Agente);
        _currentUser.Setup(u => u.AgenteId).Returns(42);

        var result = await _sut.GetAgentiVisibiliAsync();

        result.Should().BeEquivalentTo(new[] { 42 });
    }

    [Fact]
    public async Task AreaManager_VedeSoloAgentiGestiti()
    {
        _currentUser.Setup(u => u.IsAuthenticated).Returns(true);
        _currentUser.Setup(u => u.Ruolo).Returns(RuoloUtente.AreaManager);
        _currentUser.Setup(u => u.AgenteId).Returns(1);

        _agentiRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Agente, bool>>>(), default))
            .ReturnsAsync(new List<Agente>
            {
                new() { Id = 10, AreaManagerId = 1 },
                new() { Id = 11, AreaManagerId = 1 }
            });

        var result = await _sut.GetAgentiVisibiliAsync();

        result.Should().BeEquivalentTo(new[] { 10, 11 });
    }
}
