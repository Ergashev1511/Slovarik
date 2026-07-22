FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Slovarik.Api/Slovarik.Api.csproj Slovarik.Api/
COPY Slovarik.Application/Slovarik.Application.csproj Slovarik.Application/
COPY Slovarik.Infrastructure/Slovarik.Infrastructure.csproj Slovarik.Infrastructure/
COPY Slovarik.Domain/Slovarik.Domain.csproj Slovarik.Domain/

RUN dotnet restore Slovarik.Api/Slovarik.Api.csproj

COPY . .

WORKDIR /src/Slovarik.Api
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-80} dotnet Slovarik.Api.dll"]
