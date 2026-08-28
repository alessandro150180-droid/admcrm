# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY CucineCRM.sln .
COPY src/CucineCRM.Domain/CucineCRM.Domain.csproj src/CucineCRM.Domain/
COPY src/CucineCRM.Application/CucineCRM.Application.csproj src/CucineCRM.Application/
COPY src/CucineCRM.Infrastructure/CucineCRM.Infrastructure.csproj src/CucineCRM.Infrastructure/
COPY src/CucineCRM.API/CucineCRM.API.csproj src/CucineCRM.API/
COPY tests/CucineCRM.UnitTests/CucineCRM.UnitTests.csproj tests/CucineCRM.UnitTests/
COPY tests/CucineCRM.IntegrationTests/CucineCRM.IntegrationTests.csproj tests/CucineCRM.IntegrationTests/

RUN dotnet restore src/CucineCRM.API/CucineCRM.API.csproj

COPY . .
RUN dotnet publish src/CucineCRM.API/CucineCRM.API.csproj -c Release -o /app/publish --no-restore

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "CucineCRM.API.dll"]
