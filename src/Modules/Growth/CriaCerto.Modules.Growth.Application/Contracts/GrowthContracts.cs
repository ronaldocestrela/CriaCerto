using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.BuildingBlocks.Application.Abstractions.Messaging;
using CriaCerto.Modules.Growth.Application.Abstractions;
using CriaCerto.Modules.Growth.Application.Domain;
using CriaCerto.Modules.Growth.Application.Domain.Services;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.Modules.Growth.Application.Contracts;

// --- DTOs ---
public sealed record PaddockDto(
    Guid Id,
    string Name,
    string Code,
    decimal AreaHectares,
    decimal MaxCapacityUA,
    PaddockStatus Status,
    DateTime CreatedAtUtc);

public sealed record PaddockStockingRateDto(
    Guid Id,
    string Name,
    string Code,
    decimal AreaHectares,
    decimal MaxCapacityUA,
    PaddockStatus Status,
    decimal CurrentTotalUA,
    decimal CurrentStockingRateUAPerHa,
    int TotalHeadCount,
    int AssignedLotsCount,
    bool IsOvergrazed,
    bool IsNearCapacity);

public sealed record LotDto(
    Guid Id,
    string Name,
    string Code,
    LotCategory Category,
    Guid? CurrentPaddockId,
    string? PaddockName,
    int HeadCount,
    decimal AverageWeightKg,
    decimal TotalWeightKg,
    decimal TotalUA,
    LotStatus Status,
    DateTime CreatedAtUtc);

public sealed record LotMovementDto(
    Guid Id,
    Guid LotId,
    Guid? SourcePaddockId,
    Guid? DestinationPaddockId,
    DateTime MovementDate,
    int HeadCountMoved,
    string Notes);

// --- COMMANDS ---
public sealed record CreatePaddockCommand(
    string Name,
    string Code,
    decimal AreaHectares,
    decimal MaxCapacityUA,
    Guid TenantId,
    PaddockStatus Status = PaddockStatus.Active) : ICommand<PaddockDto>;

public sealed record CreateLotCommand(
    string Name,
    string Code,
    LotCategory Category,
    int HeadCount,
    decimal AverageWeightKg,
    Guid TenantId,
    Guid? InitialPaddockId = null) : ICommand<LotDto>;

public sealed record MoveLotToPaddockCommand(
    Guid LotId,
    Guid? DestinationPaddockId,
    string Notes,
    Guid TenantId) : ICommand<LotMovementDto>;

public sealed record CloseLotCommand(
    Guid LotId,
    Guid TenantId) : ICommand<LotDto>;

// --- QUERIES ---
public sealed record GetPaddocksWithStockingRateQuery(
    Guid TenantId) : IQuery<List<PaddockStockingRateDto>>;

public sealed record GetLotsQuery(
    Guid TenantId) : IQuery<List<LotDto>>;

// --- VALIDATORS ---
public sealed class CreatePaddockCommandValidator : AbstractValidator<CreatePaddockCommand>
{
    public CreatePaddockCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(30);
        RuleFor(x => x.AreaHectares).GreaterThan(0);
        RuleFor(x => x.MaxCapacityUA).GreaterThan(0);
        RuleFor(x => x.TenantId).NotEmpty();
    }
}

public sealed class CreateLotCommandValidator : AbstractValidator<CreateLotCommand>
{
    public CreateLotCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(30);
        RuleFor(x => x.HeadCount).GreaterThan(0);
        RuleFor(x => x.AverageWeightKg).GreaterThan(0);
        RuleFor(x => x.TenantId).NotEmpty();
    }
}

public sealed class MoveLotToPaddockCommandValidator : AbstractValidator<MoveLotToPaddockCommand>
{
    public MoveLotToPaddockCommandValidator()
    {
        RuleFor(x => x.LotId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
    }
}

public sealed class CloseLotCommandValidator : AbstractValidator<CloseLotCommand>
{
    public CloseLotCommandValidator()
    {
        RuleFor(x => x.LotId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
    }
}

// --- HANDLERS ---
public sealed class CreatePaddockCommandHandler : IRequestHandler<CreatePaddockCommand, Result<PaddockDto>>
{
    private readonly IGrowthDbContext _dbContext;

    public CreatePaddockCommandHandler(IGrowthDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<PaddockDto>> Handle(CreatePaddockCommand request, CancellationToken cancellationToken)
    {
        var existingCode = await _dbContext.Paddocks
            .AnyAsync(p => p.TenantId == request.TenantId && p.Code == request.Code.Trim().ToUpperInvariant(), cancellationToken);

        if (existingCode)
            return Result.Failure<PaddockDto>(Error.Conflict("PasturePaddock.DuplicateCode", "Já existe um piquete cadastrado com este código."));

        var paddockResult = PasturePaddock.Create(
            request.Name,
            request.Code,
            request.AreaHectares,
            request.MaxCapacityUA,
            request.TenantId,
            request.Status);

        if (paddockResult.IsFailure)
            return Result.Failure<PaddockDto>(paddockResult.Error);

        _dbContext.Paddocks.Add(paddockResult.Value);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new PaddockDto(
            paddockResult.Value.Id,
            paddockResult.Value.Name,
            paddockResult.Value.Code,
            paddockResult.Value.AreaHectares,
            paddockResult.Value.MaxCapacityUA,
            paddockResult.Value.Status,
            paddockResult.Value.CreatedAtUtc));
    }
}

public sealed class CreateLotCommandHandler : IRequestHandler<CreateLotCommand, Result<LotDto>>
{
    private readonly IGrowthDbContext _dbContext;

    public CreateLotCommandHandler(IGrowthDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<LotDto>> Handle(CreateLotCommand request, CancellationToken cancellationToken)
    {
        var existingCode = await _dbContext.Lots
            .AnyAsync(l => l.TenantId == request.TenantId && l.Code == request.Code.Trim().ToUpperInvariant() && l.Status == LotStatus.Active, cancellationToken);

        if (existingCode)
            return Result.Failure<LotDto>(Error.Conflict("Lot.DuplicateCode", "Já existe um lote ativo com este código."));

        string? paddockName = null;
        if (request.InitialPaddockId.HasValue)
        {
            var paddock = await _dbContext.Paddocks
                .FirstOrDefaultAsync(p => p.Id == request.InitialPaddockId.Value && p.TenantId == request.TenantId, cancellationToken);

            if (paddock is null)
                return Result.Failure<LotDto>(Error.NotFound("PasturePaddock.NotFound", "Piquete inicial não encontrado."));

            paddockName = paddock.Name;
        }

        var lotResult = Lot.Create(
            request.Name,
            request.Code,
            request.Category,
            request.HeadCount,
            request.AverageWeightKg,
            request.TenantId,
            request.InitialPaddockId);

        if (lotResult.IsFailure)
            return Result.Failure<LotDto>(lotResult.Error);

        _dbContext.Lots.Add(lotResult.Value);

        if (request.InitialPaddockId.HasValue)
        {
            var movementResult = LotMovement.Create(
                lotResult.Value.Id,
                null,
                request.InitialPaddockId.Value,
                request.HeadCount,
                request.TenantId,
                "Entrada inicial no lote");

            if (movementResult.IsSuccess)
                _dbContext.LotMovements.Add(movementResult.Value);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new LotDto(
            lotResult.Value.Id,
            lotResult.Value.Name,
            lotResult.Value.Code,
            lotResult.Value.Category,
            lotResult.Value.CurrentPaddockId,
            paddockName,
            lotResult.Value.HeadCount,
            lotResult.Value.AverageWeightKg,
            lotResult.Value.TotalWeightKg,
            lotResult.Value.TotalUA,
            lotResult.Value.Status,
            lotResult.Value.CreatedAtUtc));
    }
}

public sealed class MoveLotToPaddockCommandHandler : IRequestHandler<MoveLotToPaddockCommand, Result<LotMovementDto>>
{
    private readonly IGrowthDbContext _dbContext;

    public MoveLotToPaddockCommandHandler(IGrowthDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<LotMovementDto>> Handle(MoveLotToPaddockCommand request, CancellationToken cancellationToken)
    {
        var lot = await _dbContext.Lots
            .FirstOrDefaultAsync(l => l.Id == request.LotId && l.TenantId == request.TenantId, cancellationToken);

        if (lot is null)
            return Result.Failure<LotMovementDto>(Error.NotFound("Lot.NotFound", "Lote não encontrado."));

        if (request.DestinationPaddockId.HasValue)
        {
            var paddock = await _dbContext.Paddocks
                .FirstOrDefaultAsync(p => p.Id == request.DestinationPaddockId.Value && p.TenantId == request.TenantId, cancellationToken);

            if (paddock is null)
                return Result.Failure<LotMovementDto>(Error.NotFound("PasturePaddock.NotFound", "Piquete de destino não encontrado."));
        }

        Guid? sourcePaddockId = lot.CurrentPaddockId;

        Result assignResult = request.DestinationPaddockId.HasValue
            ? lot.AssignToPaddock(request.DestinationPaddockId.Value)
            : lot.RemoveFromPaddock();

        if (assignResult.IsFailure)
            return Result.Failure<LotMovementDto>(assignResult.Error);

        var movementResult = LotMovement.Create(
            lot.Id,
            sourcePaddockId,
            request.DestinationPaddockId,
            lot.HeadCount,
            request.TenantId,
            request.Notes);

        if (movementResult.IsFailure)
            return Result.Failure<LotMovementDto>(movementResult.Error);

        _dbContext.LotMovements.Add(movementResult.Value);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new LotMovementDto(
            movementResult.Value.Id,
            movementResult.Value.LotId,
            movementResult.Value.SourcePaddockId,
            movementResult.Value.DestinationPaddockId,
            movementResult.Value.MovementDate,
            movementResult.Value.HeadCountMoved,
            movementResult.Value.Notes));
    }
}

public sealed class CloseLotCommandHandler : IRequestHandler<CloseLotCommand, Result<LotDto>>
{
    private readonly IGrowthDbContext _dbContext;

    public CloseLotCommandHandler(IGrowthDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<LotDto>> Handle(CloseLotCommand request, CancellationToken cancellationToken)
    {
        var lot = await _dbContext.Lots
            .FirstOrDefaultAsync(l => l.Id == request.LotId && l.TenantId == request.TenantId, cancellationToken);

        if (lot is null)
            return Result.Failure<LotDto>(Error.NotFound("Lot.NotFound", "Lote não encontrado."));

        var closeResult = lot.CloseLot();
        if (closeResult.IsFailure)
            return Result.Failure<LotDto>(closeResult.Error);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new LotDto(
            lot.Id,
            lot.Name,
            lot.Code,
            lot.Category,
            lot.CurrentPaddockId,
            null,
            lot.HeadCount,
            lot.AverageWeightKg,
            lot.TotalWeightKg,
            lot.TotalUA,
            lot.Status,
            lot.CreatedAtUtc));
    }
}

public sealed class GetPaddocksWithStockingRateQueryHandler : IRequestHandler<GetPaddocksWithStockingRateQuery, Result<List<PaddockStockingRateDto>>>
{
    private readonly IGrowthDbContext _dbContext;

    public GetPaddocksWithStockingRateQueryHandler(IGrowthDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<List<PaddockStockingRateDto>>> Handle(GetPaddocksWithStockingRateQuery request, CancellationToken cancellationToken)
    {
        var paddocks = await _dbContext.Paddocks
            .Where(p => p.TenantId == request.TenantId)
            .OrderBy(p => p.Code)
            .ToListAsync(cancellationToken);

        var activeLots = await _dbContext.Lots
            .Where(l => l.TenantId == request.TenantId && l.Status == LotStatus.Active && l.CurrentPaddockId != null)
            .ToListAsync(cancellationToken);

        var result = new List<PaddockStockingRateDto>();

        foreach (var p in paddocks)
        {
            var paddockLots = activeLots.Where(l => l.CurrentPaddockId == p.Id).ToList();
            decimal totalWeight = paddockLots.Sum(l => l.TotalWeightKg);
            decimal totalUA = StockingRateCalculator.CalculateTotalUA(totalWeight);
            decimal stockingRate = StockingRateCalculator.CalculateStockingRate(totalUA, p.AreaHectares);
            int headCount = paddockLots.Sum(l => l.HeadCount);
            int lotsCount = paddockLots.Count;
            bool overgrazed = StockingRateCalculator.IsOvergrazed(totalUA, p.MaxCapacityUA);
            bool nearCapacity = StockingRateCalculator.IsNearCapacity(totalUA, p.MaxCapacityUA);

            result.Add(new PaddockStockingRateDto(
                p.Id,
                p.Name,
                p.Code,
                p.AreaHectares,
                p.MaxCapacityUA,
                p.Status,
                totalUA,
                stockingRate,
                headCount,
                lotsCount,
                overgrazed,
                nearCapacity));
        }

        return Result.Success(result);
    }
}

public sealed class GetLotsQueryHandler : IRequestHandler<GetLotsQuery, Result<List<LotDto>>>
{
    private readonly IGrowthDbContext _dbContext;

    public GetLotsQueryHandler(IGrowthDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<List<LotDto>>> Handle(GetLotsQuery request, CancellationToken cancellationToken)
    {
        var lots = await _dbContext.Lots
            .Where(l => l.TenantId == request.TenantId)
            .OrderBy(l => l.Code)
            .ToListAsync(cancellationToken);

        var paddockIds = lots.Where(l => l.CurrentPaddockId != null).Select(l => l.CurrentPaddockId!.Value).Distinct().ToList();
        var paddocksDict = await _dbContext.Paddocks
            .Where(p => paddockIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Name, cancellationToken);

        var result = lots.Select(l => new LotDto(
            l.Id,
            l.Name,
            l.Code,
            l.Category,
            l.CurrentPaddockId,
            l.CurrentPaddockId.HasValue && paddocksDict.TryGetValue(l.CurrentPaddockId.Value, out var name) ? name : null,
            l.HeadCount,
            l.AverageWeightKg,
            l.TotalWeightKg,
            l.TotalUA,
            l.Status,
            l.CreatedAtUtc)).ToList();

        return Result.Success(result);
    }
}
