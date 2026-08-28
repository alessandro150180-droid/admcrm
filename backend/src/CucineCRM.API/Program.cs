using System.Text.Json.Serialization;
using CucineCRM.API.Middleware;
using CucineCRM.Infrastructure;
using CucineCRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;

// QuestPDF richiede la dichiarazione esplicita della licenza dalla 2023.x: Community è gratuita
// per uso interno/aziende sotto la soglia di fatturato prevista dai suoi termini.
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// --- Servizi ---
builder.Services.AddControllers()
    // Gli enum viaggiano come stringhe leggibili ("Confermato", non 1): coerente con come sono
    // già salvati nel DB (HasConversion<string>() nelle Configurations) e molto più usabile per
    // chi consuma l'API (Swagger, frontend) senza dover conoscere a memoria i valori numerici.
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:3000" };

        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ADMcrm API",
        Version = "v1",
        Description = "API REST per la gestione della rete vendita di ADM Group."
    });

    var jwtScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Inserire il token JWT preceduto da 'Bearer ', es: Bearer eyJhbGciOi...",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };

    options.AddSecurityDefinition("Bearer", jwtScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { { jwtScheme, Array.Empty<string>() } });
});

var app = builder.Build();

// --- Pipeline ---
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "CucineCRM API v1");
    });

    // Applica automaticamente le migration pendenti in ambiente di sviluppo per comodità.
    // In produzione le migration vanno applicate esplicitamente (vedi README) per maggiore controllo.
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}

app.UseHttpsRedirection();

app.UseCors("FrontendPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Necessario per rendere Program accessibile ai progetti di Integration Test (WebApplicationFactory<Program>)
public partial class Program { }
