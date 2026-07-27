using CriaCerto.BuildingBlocks.Abstractions.Licensing;
using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.BuildingBlocks.Application.Abstractions.Messaging;
using CriaCerto.Modules.Calving.Application.Abstractions;
using CriaCerto.Modules.Calving.Application.Domain;
using CriaCerto.Modules.Calving.Application.Domain.Services;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.Modules.Calving.Application.Contracts;

public sealed record CalvingDto(
    Guid Id,
    Guid MotherCowId,
    DateTime CalvingDate,
    CalvingType Type,
    Guid CalfId,
    string CalfTagId,
    BirthCondition Condition);

public sealed record WeaningDto(
    Guid Id,
    Guid CalfId,
    string CalfTagId,
    Guid MotherCowId,
    DateTime WeaningDate,
    decimal WeaningWeightKg,
    decimal Adjusted205DayWeightKg,
    Guid? DestinationLotId);

public sealed record RegisterCalvingCommand(
    Guid MotherCowId,
    DateTime CalvingDate,
    CalvingType Type,
    string CalfTagId,
    string CalfSex,
    string CalfBreed,
    decimal CalfBirthWeightKg,
    BirthCondition Condition,
    Guid TenantId) : ICommand<CalvingDto>;

public sealed record RegisterWeaningCommand(
    Guid CalfId,
    Guid MotherCowId,
    DateTime WeaningDate,
    decimal WeaningWeightKg,
    int MotherAgeYears,
    Guid TenantId,
    Guid? DestinationLotId = null) : ICommand<WeaningDto>;

public sealed class RegisterCalvingCommandValidator : AbstractValidator<RegisterCalvingCommand>
{
    public RegisterCalvingCommandValidator()
    {
        RuleFor(x => x.MotherCowId).NotEmpty();
        RuleFor(x => x.CalfTagId).NotEmpty().MaximumLength(50);
        RuleFor(x => x.CalfBirthWeightKg).GreaterThan(0);
    }
}

public sealed class RegisterWeaningCommandValidator : AbstractValidator<RegisterWeaningCommand>
{
    public RegisterWeaningCommandValidator()
    {
        RuleFor(x => x.CalfId).NotEmpty();
        RuleFor(x => x.MotherCowId).NotEmpty();
        RuleFor(x => x.WeaningWeightKg).GreaterThan(0);
    }
}

public sealed class RegisterCalvingCommandHandler : IRequestHandler<RegisterCalvingCommand, Result<CalvingDto>>
{
    private readonly ICalvingDbContext _dbContext;

    public RegisterCalvingCommandHandler(ICalvingDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<CalvingDto>> Handle(RegisterCalvingCommand request, CancellationToken cancellationToken)
    {
        var calfResult = Calf.Create(
            request.CalfTagId,
            request.MotherCowId,
            request.CalvingDate,
            request.CalfSex,
            request.CalfBreed,
            request.CalfBirthWeightKg,
            request.TenantId);

        if (calfResult.IsFailure)
            return Result.Failure<CalvingDto>(calfResult.Error);

        var calvingResult = Domain.Calving.Create(
            request.MotherCowId,
            request.CalvingDate,
            request.Type,
            calfResult.Value.Id,
            request.Condition,
            request.TenantId);

        if (calvingResult.IsFailure)
            return Result.Failure<CalvingDto>(calvingResult.Error);

        _dbContext.Calves.Add(calfResult.Value);
        _dbContext.Calvings.Add(calvingResult.Value);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new CalvingDto(
            calvingResult.Value.Id,
            calvingResult.Value.MotherCowId,
            calvingResult.Value.CalvingDate,
            calvingResult.Value.Type,
            calfResult.Value.Id,
            calfResult.Value.TagId,
            calvingResult.Value.Condition));
    }
}

public sealed class RegisterWeaningCommandHandler : IRequestHandler<RegisterWeaningCommand, Result<WeaningDto>>
{
    private readonly ICalvingDbContext _dbContext;

    public RegisterWeaningCommandHandler(ICalvingDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<WeaningDto>> Handle(RegisterWeaningCommand request, CancellationToken cancellationToken)
    {
        var calf = await _dbContext.Calves.FirstOrDefaultAsync(c => c.Id == request.CalfId, cancellationToken);
        if (calf is null)
            return Result.Failure<WeaningDto>(Error.NotFound("Calf.NotFound", "Bezerro não encontrado."));

        decimal p205 = P205Calculator.CalculateP205(
            calf.BirthWeightKg,
            request.WeaningWeightKg,
            calf.BirthDate,
            request.WeaningDate,
            request.MotherAgeYears);

        var weaningResult = Weaning.Create(
            request.CalfId,
            request.MotherCowId,
            request.WeaningDate,
            request.WeaningWeightKg,
            p205,
            request.TenantId,
            request.DestinationLotId);

        if (weaningResult.IsFailure)
            return Result.Failure<WeaningDto>(weaningResult.Error);

        var markResult = calf.MarkWeaned();
        if (markResult.IsFailure)
            return Result.Failure<WeaningDto>(markResult.Error);

        _dbContext.Weanings.Add(weaningResult.Value);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new WeaningDto(
            weaningResult.Value.Id,
            calf.Id,
            calf.TagId,
            request.MotherCowId,
            weaningResult.Value.WeaningDate,
            weaningResult.Value.WeaningWeightKg,
            weaningResult.Value.Adjusted205DayWeightKg,
            weaningResult.Value.DestinationLotId));
    }
}

public sealed record CalvingRecordListItemDto(
    Guid CalvingId,
    Guid MotherCowId,
    DateTime CalvingDate,
    CalvingType Type,
    BirthCondition Condition,
    Guid CalfId,
    string CalfTagId,
    string CalfSex,
    string CalfBreed,
    decimal CalfBirthWeightKg,
    CalfStatus CalfStatus,
    DateTime? WeaningDate,
    decimal? WeaningWeightKg,
    decimal? Adjusted205DayWeightKg,
    Guid? DestinationLotId);

public sealed record GetCalvingRecordsQuery(Guid TenantId) : IQuery<List<CalvingRecordListItemDto>>;

public sealed class GetCalvingRecordsQueryHandler : IRequestHandler<GetCalvingRecordsQuery, Result<List<CalvingRecordListItemDto>>>
{
    private readonly ICalvingDbContext _dbContext;

    public GetCalvingRecordsQueryHandler(ICalvingDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<List<CalvingRecordListItemDto>>> Handle(GetCalvingRecordsQuery request, CancellationToken cancellationToken)
    {
        var calvings = await _dbContext.Calvings
            .AsNoTracking()
            .Where(c => c.TenantId == request.TenantId)
            .OrderByDescending(c => c.CalvingDate)
            .ToListAsync(cancellationToken);

        var calves = await _dbContext.Calves
            .AsNoTracking()
            .Where(c => c.TenantId == request.TenantId)
            .ToDictionaryAsync(c => c.Id, cancellationToken);

        var weanings = await _dbContext.Weanings
            .AsNoTracking()
            .Where(w => w.TenantId == request.TenantId)
            .ToDictionaryAsync(w => w.CalfId, cancellationToken);

        var dtos = calvings.Select(c =>
        {
            calves.TryGetValue(c.CalfId, out var calf);
            weanings.TryGetValue(c.CalfId, out var weaning);

            return new CalvingRecordListItemDto(
                c.Id,
                c.MotherCowId,
                c.CalvingDate,
                c.Type,
                c.Condition,
                c.CalfId,
                calf?.TagId ?? string.Empty,
                calf?.Sex ?? "M",
                calf?.Breed ?? string.Empty,
                calf?.BirthWeightKg ?? 0m,
                calf?.Status ?? CalfStatus.Unweaned,
                weaning?.WeaningDate,
                weaning?.WeaningWeightKg,
                weaning?.Adjusted205DayWeightKg,
                weaning?.DestinationLotId);
        }).ToList();

        return Result.Success(dtos);
    }
}

