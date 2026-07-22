using CriaCerto.BuildingBlocks.Application;
using CriaCerto.BuildingBlocks.Infrastructure;
using CriaCerto.Modules.Breeding.Application;
using CriaCerto.Modules.Maternity.Application;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddBuildingBlocksApplication(
    typeof(Program).Assembly,
    typeof(BreedingAssemblyMarker).Assembly,
    typeof(MaternityAssemblyMarker).Assembly);

var connectionString = builder.Configuration.GetConnectionString("PostgreSql")
    ?? "Host=localhost;Port=5432;Database=criacerto_foundation;Username=postgres;Password=postgres";

builder.Services.AddBuildingBlocksInfrastructure(connectionString);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "CriaCerto.Api" }))
    .WithName("Health");

app.Run();
