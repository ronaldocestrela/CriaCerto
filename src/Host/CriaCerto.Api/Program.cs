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
using CriaCerto.Modules.Growth.Application.Contracts;
using CriaCerto.Modules.Growth.Infrastructure;
using CriaCerto.Modules.Growth.Infrastructure.Persistence;
using CriaCerto.Modules.Nutrition.Application;
using CriaCerto.Modules.Nutrition.Application.Contracts;
using CriaCerto.Modules.Nutrition.Application.Features.AnalyticsFeatures;
using CriaCerto.Modules.Nutrition.Application.Features.FeedingFeatures;
using CriaCerto.Modules.Nutrition.Application.Features.RationFeatures;
using CriaCerto.Modules.Nutrition.Application.Features.SiloStockFeatures;
using CriaCerto.Modules.Nutrition.Infrastructure;
using CriaCerto.Modules.Nutrition.Infrastructure.Persistence;
using CriaCerto.Modules.Tenancy.Application;
using CriaCerto.Modules.Tenancy.Application.Features.Login;
using CriaCerto.Modules.Tenancy.Application.Features.RegisterUser;
using CriaCerto.Modules.Tenancy.Application.Features.CreateTenant;
using CriaCerto.Modules.Tenancy.Application.Features.ForgotPassword;
using CriaCerto.Modules.Tenancy.Application.Features.ResetPassword;
using CriaCerto.Modules.Tenancy.Application.Features.SelectTenant;
using CriaCerto.Modules.Tenancy.Application.Features.GetSubscriptionPlans;
using CriaCerto.Modules.Tenancy.Infrastructure;
using CriaCerto.Modules.Tenancy.Infrastructure.Persistence;
using CriaCerto.Modules.Sanitary.Application.Contracts;
using CriaCerto.Modules.Sanitary.Infrastructure;
using CriaCerto.Modules.Sanitary.Infrastructure.Persistence;
using CriaCerto.Modules.Analytics.Application.Contracts;
using CriaCerto.Modules.Analytics.Application.Services;
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
    typeof(CriaCerto.Modules.Growth.Application.Contracts.PaddockDto).Assembly,
    typeof(NutritionAssemblyMarker).Assembly,
    typeof(VaccinationCampaignDto).Assembly,
    typeof(ExecutiveScorecardDto).Assembly,
    typeof(TenancyAssemblyMarker).Assembly);

var connectionString = builder.Configuration.GetConnectionString("SqlServer")
    ?? "Server=localhost,1433;Database=criacerto_foundation;User Id=sa;Password=CriaCerto@123;TrustServerCertificate=True;Encrypt=False";

// Register Building Blocks and Infrastructure
builder.Services.AddBuildingBlocksInfrastructure(connectionString);
builder.Services.AddTenancyInfrastructure(builder.Configuration);
builder.Services.AddBreedingInfrastructure();
builder.Services.AddCalvingInfrastructure();
builder.Services.AddGrowthInfrastructure();
builder.Services.AddNutritionInfrastructure();
builder.Services.AddSanitaryModule(builder.Configuration);

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

app.MapPost("/api/auth/register", async (RegisterUserCommand command, ISender sender) =>
{
    var result = await sender.Send(command);
    return result.IsSuccess
        ? Results.Json(result.Value, statusCode: StatusCodes.Status201Created)
        : Results.Json(result.Error, statusCode: ToStatusCode(result.Error.Type));
}).AllowAnonymous().WithTags("Auth");

app.MapPost("/api/auth/forgot-password", async (ForgotPasswordCommand command, ISender sender) =>
{
    var result = await sender.Send(command);
    return result.IsSuccess
        ? Results.Ok(new { token = result.Value, message = "Se o e-mail estiver cadastrado, as instruções para redefinição foram geradas com sucesso." })
        : Results.Json(result.Error, statusCode: ToStatusCode(result.Error.Type));
}).AllowAnonymous().WithTags("Auth");

app.MapPost("/api/auth/reset-password", async (ResetPasswordCommand command, ISender sender) =>
{
    var result = await sender.Send(command);
    return result.IsSuccess
        ? Results.Ok(new { message = "Senha redefinida com sucesso." })
        : Results.Json(result.Error, statusCode: ToStatusCode(result.Error.Type));
}).AllowAnonymous().WithTags("Auth");

app.MapPost("/api/auth/select-tenant", async (SelectTenantCommand command, ISender sender) =>
{
    var result = await sender.Send(command);
    return result.IsSuccess 
        ? Results.Ok(result.Value) 
        : Results.Json(result.Error, statusCode: 400);
});

app.MapGet("/api/v1/tenancy/plans", async (ISender sender) =>
{
    var result = await sender.Send(new GetSubscriptionPlansQuery());
    return result.IsSuccess
        ? Results.Ok(result.Value)
        : Results.Json(result.Error, statusCode: 400);
}).AllowAnonymous().WithTags("Tenancy");

app.MapPost("/api/v1/tenancy/farms", async (CreateTenantCommand command, ISender sender) =>
{
    var result = await sender.Send(command);
    return result.IsSuccess
        ? Results.Json(result.Value, statusCode: StatusCodes.Status201Created)
        : Results.Json(result.Error, statusCode: ToStatusCode(result.Error.Type));
}).AllowAnonymous().WithTags("Tenancy");

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

// Growth & Pasture Management Endpoints
var growth = app.MapGroup("/api/growth")
    .RequireAuthorization()
    .WithTags("Growth");

growth.MapGet("/paddocks", async (Guid tenantId, ISender sender) =>
{
    var result = await sender.Send(new GetPaddocksWithStockingRateQuery(tenantId));
    return ToHttpResult(result);
});

growth.MapPost("/paddocks", async (CreatePaddockCommand command, ISender sender) =>
{
    var result = await sender.Send(command);
    return ToHttpResult(result, StatusCodes.Status201Created);
});

growth.MapGet("/lots", async (Guid tenantId, ISender sender) =>
{
    var result = await sender.Send(new GetLotsQuery(tenantId));
    return ToHttpResult(result);
});

growth.MapPost("/lots", async (CreateLotCommand command, ISender sender) =>
{
    var result = await sender.Send(command);
    return ToHttpResult(result, StatusCodes.Status201Created);
});

growth.MapPost("/lots/move", async (MoveLotToPaddockCommand command, ISender sender) =>
{
    var result = await sender.Send(command);
    return ToHttpResult(result, StatusCodes.Status200OK);
});

growth.MapPost("/lots/{id:guid}/close", async (Guid id, Guid tenantId, ISender sender) =>
{
    var result = await sender.Send(new CloseLotCommand(id, tenantId));
    return ToHttpResult(result, StatusCodes.Status200OK);
});

growth.MapPost("/weighings", async (RecordWeighingCommand command, ISender sender) =>
{
    var result = await sender.Send(command);
    return ToHttpResult(result, StatusCodes.Status201Created);
});

growth.MapPost("/weighings/batch", async (BatchRecordWeighingCommand command, ISender sender) =>
{
    var result = await sender.Send(command);
    return ToHttpResult(result, StatusCodes.Status201Created);
});

growth.MapGet("/weighings/history/{animalTagId}", async (Guid tenantId, string animalTagId, ISender sender) =>
{
    var result = await sender.Send(new GetAnimalWeighingHistoryQuery(tenantId, animalTagId));
    return ToHttpResult(result);
});

growth.MapGet("/weighings/lot-summary/{lotId:guid}", async (Guid tenantId, Guid lotId, ISender sender) =>
{
    var result = await sender.Send(new GetLotWeighingSummaryQuery(tenantId, lotId));
    return ToHttpResult(result);
});

growth.MapGet("/weighings/recent", async (Guid tenantId, Guid? lotId, int? top, ISender sender) =>
{
    var result = await sender.Send(new GetRecentWeighingsQuery(tenantId, lotId, top ?? 50));
    return ToHttpResult(result);
});

// Nutrition & Feed Management Endpoints
var nutrition = app.MapGroup("/api/nutrition")
    .RequireAuthorization()
    .WithTags("Nutrition");

nutrition.MapGet("/silos", async (Guid tenantId, ISender sender) =>
{
    var result = await sender.Send(new GetSiloStocksQuery(tenantId));
    return ToHttpResult(result);
});

nutrition.MapPost("/silos", async (CreateSiloStockCommand command, ISender sender) =>
{
    var result = await sender.Send(command);
    return ToHttpResult(result, StatusCodes.Status201Created);
});

nutrition.MapPost("/silos/restock", async (RestockSiloCommand command, ISender sender) =>
{
    var result = await sender.Send(command);
    return ToHttpResult(result);
});

nutrition.MapGet("/rations", async (Guid tenantId, ISender sender) =>
{
    var result = await sender.Send(new GetFeedRationsQuery(tenantId));
    return ToHttpResult(result);
});

nutrition.MapPost("/rations", async (CreateFeedRationCommand command, ISender sender) =>
{
    var result = await sender.Send(command);
    return ToHttpResult(result, StatusCodes.Status201Created);
});

nutrition.MapPost("/supplementation", async (RecordSupplementationCommand command, ISender sender) =>
{
    var result = await sender.Send(command);
    return ToHttpResult(result, StatusCodes.Status201Created);
});

nutrition.MapPost("/tmr-batches", async (RecordFeedlotTmrCommand command, ISender sender) =>
{
    var result = await sender.Send(command);
    return ToHttpResult(result, StatusCodes.Status201Created);
});

nutrition.MapGet("/analytics/feed-conversion", async (Guid tenantId, Guid lotId, decimal totalWeightGainKg, ISender sender) =>
{
    var result = await sender.Send(new GetFeedlotPerformanceQuery(tenantId, lotId, totalWeightGainKg));
    return ToHttpResult(result);
});

nutrition.MapGet("/analytics/cost-per-arroba", async (Guid tenantId, Guid lotId, decimal totalWeightGainKg, decimal? carcassYieldPercentage, ISender sender) =>
{
    var result = await sender.Send(new GetCostPerArrobaQuery(tenantId, lotId, totalWeightGainKg, carcassYieldPercentage));
    return ToHttpResult(result);
});

// --- SANITARY ENDPOINTS ---
var sanitary = app.MapGroup("/api/sanitary").RequireAuthorization();

sanitary.MapGet("/campaigns", async (ISender sender) =>
{
    var result = await sender.Send(new GetActiveCampaignsQuery());
    return ToHttpResult(result);
});

sanitary.MapPost("/campaigns", async (CreateVaccinationCampaignCommand command, ISender sender) =>
{
    var result = await sender.Send(command);
    return ToHttpResult(result, StatusCodes.Status201Created);
});

sanitary.MapPost("/treatments", async (ApplyTreatmentCommand command, ISender sender) =>
{
    var result = await sender.Send(command);
    return ToHttpResult(result, StatusCodes.Status201Created);
});

sanitary.MapGet("/slaughter-validation/{animalId:guid}", async (Guid animalId, ISender sender) =>
{
    var result = await sender.Send(new ValidateSlaughterEligibilityQuery(animalId));
    return ToHttpResult(result);
});

// --- EXECUTIVE ANALYTICS ENDPOINTS ---
var analytics = app.MapGroup("/api/analytics").RequireAuthorization();

analytics.MapGet("/executive-scorecard", async (
    int totalCows,
    int pregnantCows,
    int calvesWeaned,
    decimal totalPastureHectares,
    decimal totalAnimalUnits,
    decimal averageGpdKg,
    decimal averageCostPerArroba,
    int animalsUnderWithdrawal,
    ISender sender) =>
{
    var query = new GetExecutiveAnalyticsQuery(
        totalCows, pregnantCows, calvesWeaned, totalPastureHectares, totalAnimalUnits, averageGpdKg, averageCostPerArroba, animalsUnderWithdrawal);
    var result = await sender.Send(query);
    return ToHttpResult(result);
});

analytics.MapPost("/export-csv", async (ExecutiveScorecardDto scorecard, ISender sender) =>
{
    var result = await sender.Send(new ExportBovineReportQuery(scorecard));
    return ToHttpResult(result);
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
        scope.ServiceProvider.GetRequiredService<CalvingDbContext>(),
        scope.ServiceProvider.GetRequiredService<GrowthDbContext>(),
        scope.ServiceProvider.GetRequiredService<NutritionDbContext>()
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
