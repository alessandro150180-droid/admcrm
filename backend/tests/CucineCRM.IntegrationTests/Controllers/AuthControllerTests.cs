using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CucineCRM.Application.DTOs;
using CucineCRM.Domain.Entities;
using CucineCRM.Domain.Enums;
using CucineCRM.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CucineCRM.IntegrationTests.Controllers;

public class AuthControllerTests : IClassFixture<CucineCrmWebApplicationFactory>
{
    private readonly CucineCrmWebApplicationFactory _factory;

    // Le stesse opzioni JSON configurate per i controller in Program.cs (enum come stringhe):
    // System.Net.Http.Json usa per default JsonSerializerOptions.Web, che non le conosce.
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public AuthControllerTests(CucineCrmWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_UtenteEsistenteEAttivo_Restituisce200EToken()
    {
        // Arrange: seed diretto nel DbContext InMemory
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.EnsureCreatedAsync();

            var hasher = scope.ServiceProvider.GetRequiredService<CucineCRM.Application.Interfaces.IPasswordHasher>();

            db.Utenti.Add(new Utente
            {
                Nome = "Mario",
                Cognome = "Rossi",
                Email = "mario.rossi@cucine.it",
                PasswordHash = hasher.Hash("Password123!"),
                Ruolo = RuoloUtente.DirettoreCommerciale,
                Attivo = true
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto("mario.rossi@cucine.it", "Password123!"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<LoginResponseDto>(JsonOptions);
        body.Should().NotBeNull();
        body!.Token.Should().NotBeNullOrWhiteSpace();
        body.Utente.Email.Should().Be("mario.rossi@cucine.it");
    }

    [Fact]
    public async Task Login_CredenzialiErrate_Restituisce401()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequestDto("inesistente@cucine.it", "password"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
