# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution file and all project references for caching restore layers
COPY CriaCerto.slnx ./
COPY src/BuildingBlocks/CriaCerto.BuildingBlocks.Abstractions/CriaCerto.BuildingBlocks.Abstractions.csproj src/BuildingBlocks/CriaCerto.BuildingBlocks.Abstractions/
COPY src/BuildingBlocks/CriaCerto.BuildingBlocks.Application/CriaCerto.BuildingBlocks.Application.csproj src/BuildingBlocks/CriaCerto.BuildingBlocks.Application/
COPY src/BuildingBlocks/CriaCerto.BuildingBlocks.Infrastructure/CriaCerto.BuildingBlocks.Infrastructure.csproj src/BuildingBlocks/CriaCerto.BuildingBlocks.Infrastructure/
COPY src/Modules/Analytics/CriaCerto.Modules.Analytics.Application/CriaCerto.Modules.Analytics.Application.csproj src/Modules/Analytics/CriaCerto.Modules.Analytics.Application/
COPY src/Modules/Analytics/CriaCerto.Modules.Analytics.Infrastructure/CriaCerto.Modules.Analytics.Infrastructure.csproj src/Modules/Analytics/CriaCerto.Modules.Analytics.Infrastructure/
COPY src/Modules/Breeding/CriaCerto.Modules.Breeding.Application/CriaCerto.Modules.Breeding.Application.csproj src/Modules/Breeding/CriaCerto.Modules.Breeding.Application/
COPY src/Modules/Breeding/CriaCerto.Modules.Breeding.Infrastructure/CriaCerto.Modules.Breeding.Infrastructure.csproj src/Modules/Breeding/CriaCerto.Modules.Breeding.Infrastructure/
COPY src/Modules/Calving/CriaCerto.Modules.Calving.Application/CriaCerto.Modules.Calving.Application.csproj src/Modules/Calving/CriaCerto.Modules.Calving.Application/
COPY src/Modules/Calving/CriaCerto.Modules.Calving.Infrastructure/CriaCerto.Modules.Calving.Infrastructure.csproj src/Modules/Calving/CriaCerto.Modules.Calving.Infrastructure/
COPY src/Modules/Growth/CriaCerto.Modules.Growth.Application/CriaCerto.Modules.Growth.Application.csproj src/Modules/Growth/CriaCerto.Modules.Growth.Application/
COPY src/Modules/Growth/CriaCerto.Modules.Growth.Infrastructure/CriaCerto.Modules.Growth.Infrastructure.csproj src/Modules/Growth/CriaCerto.Modules.Growth.Infrastructure/
COPY src/Modules/Nutrition/CriaCerto.Modules.Nutrition.Application/CriaCerto.Modules.Nutrition.Application.csproj src/Modules/Nutrition/CriaCerto.Modules.Nutrition.Application/
COPY src/Modules/Nutrition/CriaCerto.Modules.Nutrition.Infrastructure/CriaCerto.Modules.Nutrition.Infrastructure.csproj src/Modules/Nutrition/CriaCerto.Modules.Nutrition.Infrastructure/
COPY src/Modules/Sanitary/CriaCerto.Modules.Sanitary.Application/CriaCerto.Modules.Sanitary.Application.csproj src/Modules/Sanitary/CriaCerto.Modules.Sanitary.Application/
COPY src/Modules/Sanitary/CriaCerto.Modules.Sanitary.Infrastructure/CriaCerto.Modules.Sanitary.Infrastructure.csproj src/Modules/Sanitary/CriaCerto.Modules.Sanitary.Infrastructure/
COPY src/Modules/Tenancy/CriaCerto.Modules.Tenancy.Application/CriaCerto.Modules.Tenancy.Application.csproj src/Modules/Tenancy/CriaCerto.Modules.Tenancy.Application/
COPY src/Modules/Tenancy/CriaCerto.Modules.Tenancy.Infrastructure/CriaCerto.Modules.Tenancy.Infrastructure.csproj src/Modules/Tenancy/CriaCerto.Modules.Tenancy.Infrastructure/
COPY src/Host/CriaCerto.Api/CriaCerto.Api.csproj src/Host/CriaCerto.Api/

RUN dotnet restore src/Host/CriaCerto.Api/CriaCerto.Api.csproj

# Copy source and publish
COPY src/ src/
RUN dotnet publish src/Host/CriaCerto.Api/CriaCerto.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CriaCerto.Api.dll"]
