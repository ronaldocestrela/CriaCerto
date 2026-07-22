using System.Text;
using CriaCerto.BuildingBlocks.Application;
using CriaCerto.BuildingBlocks.Infrastructure;
using CriaCerto.BuildingBlocks.Infrastructure.Persistence;
using CriaCerto.Modules.Breeding.Application;
using CriaCerto.Modules.Maternity.Application;
using CriaCerto.Modules.Tenancy.Application;
using CriaCerto.Modules.Tenancy.Application.Features.Login;
using CriaCerto.Modules.Tenancy.Application.Features.SelectTenant;
using CriaCerto.Modules.Tenancy.Infrastructure;
using CriaCerto.Modules.Tenancy.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Register Assemblies for MediatR and Validation discovery
builder.Services.AddBuildingBlocksApplication(
    typeof(Program).Assembly,
    typeof(BreedingAssemblyMarker).Assembly,
    typeof(MaternityAssemblyMarker).Assembly,
    typeof(TenancyAssemblyMarker).Assembly);

var connectionString = builder.Configuration.GetConnectionString("SqlServer")
    ?? "Server=localhost,1433;Database=criacerto_foundation;User Id=sa;Password=CriaCerto@123;TrustServerCertificate=True;Encrypt=False";

// Register Building Blocks and Tenancy Infrastructure
builder.Services.AddBuildingBlocksInfrastructure(connectionString);
builder.Services.AddTenancyInfrastructure(builder.Configuration);

// Configure JWT Authentication
var jwtSecret = builder.Configuration["Jwt:SecretKey"] ?? "CriaCertoSuperSecretKeyThatIsAtLeast32BytesLong!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "CriaCerto",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "CriaCertoClient",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

ApplyMigrations(app);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "CriaCerto.Api" }))
    .WithName("Health");

// Auth Endpoints
app.MapPost("/api/auth/login", async (LoginCommand command, ISender sender) =>
{
    var result = await sender.Send(command);
    return result.IsSuccess 
        ? Results.Ok(result.Value) 
        : Results.Json(result.Error, statusCode: 401);
});

app.MapPost("/api/auth/select-tenant", async (SelectTenantCommand command, ISender sender) =>
{
    var result = await sender.Send(command);
    return result.IsSuccess 
        ? Results.Ok(result.Value) 
        : Results.Json(result.Error, statusCode: 400);
});

app.Run();

static void ApplyMigrations(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
        .CreateLogger("Startup.Migrations");

    var dbContexts = new DbContext[]
    {
        scope.ServiceProvider.GetRequiredService<FoundationDbContext>(),
        scope.ServiceProvider.GetRequiredService<TenancyDbContext>()
    };

    foreach (var dbContext in dbContexts)
    {
        try
        {
            dbContext.Database.Migrate();
            logger.LogInformation("Migrations applied for DbContext {DbContextName}", dbContext.GetType().Name);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to apply migrations for DbContext {DbContextName}", dbContext.GetType().Name);
            throw;
        }
    }
}
