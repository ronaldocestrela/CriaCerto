# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution file and web project references
COPY CriaCerto.slnx ./
COPY src/Web/CriaCerto.Web/CriaCerto.Web/CriaCerto.Web.csproj src/Web/CriaCerto.Web/CriaCerto.Web/
COPY src/Web/CriaCerto.Web/CriaCerto.Web.Client/CriaCerto.Web.Client.csproj src/Web/CriaCerto.Web/CriaCerto.Web.Client/

RUN dotnet restore src/Web/CriaCerto.Web/CriaCerto.Web/CriaCerto.Web.csproj

# Copy source and publish
COPY src/Web/CriaCerto.Web/ src/Web/CriaCerto.Web/
RUN dotnet publish src/Web/CriaCerto.Web/CriaCerto.Web/CriaCerto.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CriaCerto.Web.dll"]
