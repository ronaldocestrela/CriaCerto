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

var connectionString = builder.Configuration.GetConnectionString("SqlServer")
    ?? "Server=localhost,1433;Database=criacerto_foundation;User Id=sa;Password=CriaCerto@123;TrustServerCertificate=True;Encrypt=False";

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
