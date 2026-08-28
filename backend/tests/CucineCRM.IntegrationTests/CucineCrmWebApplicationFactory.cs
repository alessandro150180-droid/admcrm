using CucineCRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CucineCRM.IntegrationTests;

public class CucineCrmWebApplicationFactory : WebApplicationFactory<Program>
{
    // Generato una sola volta per istanza della factory: il delegate passato ad AddDbContext viene
    // rieseguito a ogni scope (una richiesta HTTP = uno scope), quindi calcolare qui il nome garantisce
    // che tutti gli scope (arrange dei test e richieste HTTP) condividano lo stesso database InMemory.
    private readonly string _databaseName = $"CucineCRM_Test_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Rimuove sia le opzioni risolte sia il delegate di configurazione Npgsql registrato da
            // AddInfrastructure: lasciando quest'ultimo, verrebbe comunque eseguito insieme a
            // UseInMemoryDatabase, causando "due provider registrati nello stesso service provider".
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();

            // Sostituisce con un database EF Core InMemory, isolato per ogni esecuzione dei test
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });

        builder.UseEnvironment("Testing");
    }
}
