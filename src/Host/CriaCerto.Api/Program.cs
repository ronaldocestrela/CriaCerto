using System.Text;
using CriaCerto.BuildingBlocks.Application;
using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.BuildingBlocks.Infrastructure;
using CriaCerto.BuildingBlocks.Infrastructure.Persistence;
using CriaCerto.Modules.Breeding.Application.Domain;
using CriaCerto.Modules.Breeding.Application.Contracts;
using CriaCerto.Modules.Breeding.Application.Features.Plantel;
using CriaCerto.Modules.Breeding.Application.Features.BreedingOps;
using CriaCerto.Modules.Breeding.Infrastructure;
using CriaCerto.Modules.Breeding.Infrastructure.Persistence;
using CriaCerto.Modules.Calving.Application.Contracts;
using CriaCerto.Modules.Calving.Infrastructure;
using CriaCerto.Modules.Calving.Infrastructure.Persistence;
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
    typeof(CriaCerto.Modules.Breeding.Application.BreedingAssemblyMarker).Assembly,
    typeof(CriaCerto.Modules.Calving.Application.Contracts.CalvingDto).Assembly,
    typeof(TenancyAssemblyMarker).Assembly);

var connectionString = builder.Configuration.GetConnectionString("SqlServer")
    ?? "Server=localhost,1433;Database=criacerto_foundation;User Id=sa;Password=CriaCerto@123;TrustServerCertificate=True;Encrypt=False";

// Register Building Blocks and Infrastructure
builder.Services.AddBuildingBlocksInfrastructure(connectionString);
builder.Services.AddTenancyInfrastructure(builder.Configuration);
builder.Services.AddBreedingInfrastructure();
builder.Services.AddCalvingInfrastructure();

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

// Cattle Breeding Endpoints
var breeding = app.MapGroup("/api/breeding")
    .RequireAuthorization()
    .WithTags("Breeding");

breeding.MapGet("/cows", async (
    ISender sender,
    string? search,
    ReproductiveStatus? status,
    int page = 1,
    int pageSize = 25) =>
{
    var result = await sender.Send(new ListCowsQuery(search, status, page, pageSize));
    return ToHttpResult(result);
});

breeding.MapPost("/cows", async (CreateCowCommand command, ISender sender) =>
{
    var result = await sender.Send(command);
    return ToHttpResult(result, StatusCodes.Status201Created);
});

breeding.MapGet("/cows/{id:guid}", async (Guid id, ISender sender) =>
{
    var result = await sender.Send(new GetCowQuery(id));
    return ToHttpResult(result);
});

breeding.MapPost("/iatf-protocols", async (RegisterIatfProtocolCommand command, ISender sender) =>
{
    var result = await sender.Send(command);
    return ToHttpResult(result, StatusCodes.Status201Created);
});

breeding.MapPost("/diagnoses", async (RegisterPregnancyDiagnosisCommand command, ISender sender) =>
{
    var result = await sender.Send(command);
    return ToHttpResult(result, StatusCodes.Status201Created);
});

// Calving & Nursery Endpoints
var calving = app.MapGroup("/api/calving")
    .RequireAuthorization()
    .WithTags("Calving");

calving.MapPost("/calvings", async (RegisterCalvingCommand command, ISender sender) =>
{
    var result = await sender.Send(command);
    return ToHttpResult(result, StatusCodes.Status201Created);
});

calving.MapPost("/weanings", async (RegisterWeaningCommand command, ISender sender) =>
{
    var result = await sender.Send(command);
    return ToHttpResult(result, StatusCodes.Status201Created);
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
        scope.ServiceProvider.GetRequiredService<TenancyDbContext>(),
        scope.ServiceProvider.GetRequiredService<BreedingDbContext>(),
        scope.ServiceProvider.GetRequiredService<CalvingDbContext>()
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

static IResult ToHttpResult<TValue>(Result<TValue> result, int successStatusCode = StatusCodes.Status200OK)
{
    if (result.IsSuccess)
    {
        return successStatusCode == StatusCodes.Status200OK
            ? Results.Ok(result.Value)
            : Results.Json(result.Value, statusCode: successStatusCode);
    }

    return Results.Json(result.Error, statusCode: ToStatusCode(result.Error.Type));
}

static int ToStatusCode(ErrorType errorType) => errorType switch
{
    ErrorType.Validation => StatusCodes.Status400BadRequest,
    ErrorType.NotFound => StatusCodes.Status404NotFound,
    ErrorType.Conflict => StatusCodes.Status409Conflict,
    ErrorType.Unauthorized => StatusCodes.Status403Forbidden,
    _ => StatusCodes.Status400BadRequest
};
