using CriaCerto.BuildingBlocks.Abstractions.Licensing;
using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.BuildingBlocks.Application.Abstractions.Messaging;
using CriaCerto.Modules.Breeding.Application.Abstractions;
using CriaCerto.Modules.Breeding.Application.Contracts;
using CriaCerto.Modules.Breeding.Application.Domain;
using CriaCerto.Modules.Breeding.Application.Domain.Services;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.Modules.Breeding.Application.Features.BreedingOps;

internal static class BreedingOpsConstants
{
    public const int PregnancyCheckMinDays = 21;
    public const int PregnancyCheckMaxDays = 28;
    public const int DnpQueueAlertThreshold = 12;
}

[RequiresModule("Breeding")]
public sealed record RegisterBreedingBatchCommand(
    DateOnly EventDate,
    IReadOnlyList<BreedingBatchLineDto> Lines) : ICommand<RegisterBreedingBatchResponse>;

[RequiresModule("Breeding")]
public sealed record RegisterPregnancyDiagnosisCommand(
    Guid SowId,
    DateOnly DiagnosisDate,
    PregnancyDiagnosisMethod Method,
    PregnancyDiagnosisResult Result,
    string? Notes) : ICommand<PregnancyDiagnosisDto>;

[RequiresModule("Breeding")]
public sealed record ListPregnancyCheckTasksQuery(
    string? Search,
    int Page = 1,
    int PageSize = 25) : IQuery<PregnancyCheckQueueResponse>;

public sealed class RegisterBreedingBatchCommandValidator : AbstractValidator<RegisterBreedingBatchCommand>
{
    public RegisterBreedingBatchCommandValidator()
    {
        RuleFor(x => x.Lines).NotEmpty();
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(x => x.TagId).NotEmpty().MaximumLength(40);
        });
    }
}

public sealed class RegisterPregnancyDiagnosisCommandValidator : AbstractValidator<RegisterPregnancyDiagnosisCommand>
{
    public RegisterPregnancyDiagnosisCommandValidator()
    {
        RuleFor(x => x.SowId).NotEmpty();
    }
}

public sealed class RegisterBreedingBatchCommandHandler : IRequestHandler<RegisterBreedingBatchCommand, Result<RegisterBreedingBatchResponse>>
{
    private readonly IBreedingDbContext _dbContext;
    private readonly IDnpCalculator _dnpCalculator;

    public RegisterBreedingBatchCommandHandler(IBreedingDbContext dbContext, IDnpCalculator dnpCalculator)
    {
        _dbContext = dbContext;
        _dnpCalculator = dnpCalculator;
    }

    public async Task<Result<RegisterBreedingBatchResponse>> Handle(RegisterBreedingBatchCommand request, CancellationToken cancellationToken)
    {
        var normalizedTags = request.Lines.Select(l => Normalize(l.TagId)).Distinct().ToList();
        var sows = await _dbContext.Sows
            .Include(s => s.Events)
            .Where(s => normalizedTags.Contains(s.TagId))
            .ToListAsync(cancellationToken);

        if (sows.Count != normalizedTags.Count)
        {
            var missing = normalizedTags.Except(sows.Select(s => s.TagId)).FirstOrDefault();
            return Result.Failure<RegisterBreedingBatchResponse>(
                Error.NotFound("Sow.NotFound", $"Matriz não encontrada: {missing}."));
        }

        var sowMap = sows.ToDictionary(s => s.TagId);
        var sowById = sows.ToDictionary(s => s.Id);
        var createdEvents = new List<BreedingEvent>();
        var asOf = request.EventDate;

        foreach (var line in request.Lines)
        {
            var sow = sowMap[Normalize(line.TagId)];
            var domainResult = sow.RegisterBreeding(
                request.EventDate,
                line.Method,
                line.BoarOrSemenRef,
                line.Technician,
                line.BodyConditionScoreAtBreeding);

            if (domainResult.IsFailure)
            {
                return Result.Failure<RegisterBreedingBatchResponse>(
                    Error.Conflict(domainResult.Error.Code, $"{sow.TagId}: {domainResult.Error.Message}"));
            }

            var breedingEvent = new BreedingEvent(
                Guid.NewGuid(),
                sow.Id,
                request.EventDate,
                line.Method,
                line.BoarOrSemenRef,
                line.Technician,
                line.BodyConditionScoreAtBreeding,
                line.Location ?? sow.Location,
                line.Notes);

            createdEvents.Add(breedingEvent);
            _dbContext.BreedingEvents.Add(breedingEvent);

            var diagnoses = await _dbContext.PregnancyDiagnoses
                .Where(d => d.SowId == sow.Id)
                .ToListAsync(cancellationToken);
            var breedings = await _dbContext.BreedingEvents
                .Where(b => b.SowId == sow.Id)
                .ToListAsync(cancellationToken);
            breedings.Add(breedingEvent);
            sow.SetDnpDays(_dnpCalculator.Calculate(sow, breedings, diagnoses, asOf));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var dtos = createdEvents
            .Select(e => new BreedingEventDto(
                e.Id,
                e.SowId,
                sowById[e.SowId].TagId,
                e.EventDate,
                e.Method,
                e.BoarOrSemenRef,
                e.Technician,
                e.BodyConditionScoreAtBreeding,
                e.Location))
            .ToList();

        return Result.Success(new RegisterBreedingBatchResponse(dtos, dtos.Count));
    }

    private static string Normalize(string tagId) => tagId.Trim().ToUpperInvariant();
}

public sealed class RegisterPregnancyDiagnosisCommandHandler : IRequestHandler<RegisterPregnancyDiagnosisCommand, Result<PregnancyDiagnosisDto>>
{
    private readonly IBreedingDbContext _dbContext;
    private readonly IDnpCalculator _dnpCalculator;

    public RegisterPregnancyDiagnosisCommandHandler(IBreedingDbContext dbContext, IDnpCalculator dnpCalculator)
    {
        _dbContext = dbContext;
        _dnpCalculator = dnpCalculator;
    }

    public async Task<Result<PregnancyDiagnosisDto>> Handle(RegisterPregnancyDiagnosisCommand request, CancellationToken cancellationToken)
    {
        var sow = await _dbContext.Sows
            .Include(s => s.Events)
            .FirstOrDefaultAsync(s => s.Id == request.SowId, cancellationToken);

        if (sow is null)
        {
            return Result.Failure<PregnancyDiagnosisDto>(Error.NotFound("Sow.NotFound", "Matriz não encontrada."));
        }

        var latestBreeding = await _dbContext.BreedingEvents
            .Where(b => b.SowId == sow.Id)
            .OrderByDescending(b => b.EventDate)
            .FirstOrDefaultAsync(cancellationToken);

        var domainResult = sow.ApplyPregnancyDiagnosis(request.DiagnosisDate, request.Result);
        if (domainResult.IsFailure)
        {
            return Result.Failure<PregnancyDiagnosisDto>(domainResult.Error);
        }

        var method = request.Result == PregnancyDiagnosisResult.ReturnToEstrus
            ? PregnancyDiagnosisMethod.ReturnToEstrus
            : request.Method;

        var diagnosis = new PregnancyDiagnosis(
            Guid.NewGuid(),
            sow.Id,
            latestBreeding?.Id,
            request.DiagnosisDate,
            method,
            request.Result,
            request.Notes);

        _dbContext.PregnancyDiagnoses.Add(diagnosis);

        var breedings = await _dbContext.BreedingEvents.Where(b => b.SowId == sow.Id).ToListAsync(cancellationToken);
        var diagnoses = await _dbContext.PregnancyDiagnoses.Where(d => d.SowId == sow.Id).ToListAsync(cancellationToken);
        diagnoses.Add(diagnosis);
        sow.SetDnpDays(_dnpCalculator.Calculate(sow, breedings, diagnoses, request.DiagnosisDate));

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new PregnancyDiagnosisDto(
            diagnosis.Id,
            sow.Id,
            sow.TagId,
            diagnosis.BreedingEventId,
            diagnosis.DiagnosisDate,
            diagnosis.Method,
            diagnosis.Result,
            sow.ReproductiveStatus,
            sow.DnpDays));
    }
}

public sealed class ListPregnancyCheckTasksQueryHandler : IRequestHandler<ListPregnancyCheckTasksQuery, Result<PregnancyCheckQueueResponse>>
{
    private readonly IBreedingDbContext _dbContext;

    public ListPregnancyCheckTasksQueryHandler(IBreedingDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<PregnancyCheckQueueResponse>> Handle(ListPregnancyCheckTasksQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var bredSows = await _dbContext.Sows
            .AsNoTracking()
            .Where(s => s.ReproductiveStatus == ReproductiveStatus.Bred && s.LifecycleStatus != LifecycleStatus.Culled)
            .ToListAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpperInvariant();
            bredSows = bredSows
                .Where(s => s.TagId.Contains(search, StringComparison.Ordinal) ||
                            (s.Nickname?.ToUpperInvariant().Contains(search, StringComparison.Ordinal) ?? false))
                .ToList();
        }

        var sowIds = bredSows.Select(s => s.Id).ToList();
        var latestBreedings = await _dbContext.BreedingEvents
            .AsNoTracking()
            .Where(b => sowIds.Contains(b.SowId))
            .GroupBy(b => b.SowId)
            .Select(g => g.OrderByDescending(b => b.EventDate).First())
            .ToListAsync(cancellationToken);

        var breedingMap = latestBreedings.ToDictionary(b => b.SowId);

        var tasks = bredSows
            .Where(s => breedingMap.ContainsKey(s.Id))
            .Select(s =>
            {
                var breeding = breedingMap[s.Id];
                var daysPost = today.DayNumber - breeding.EventDate.DayNumber;
                return new PregnancyCheckTaskDto(
                    s.Id,
                    s.TagId,
                    s.Nickname,
                    s.Location,
                    breeding.Id,
                    breeding.EventDate,
                    daysPost,
                    breeding.Technician,
                    breeding.Method,
                    s.DnpDays,
                    s.DnpDays >= BreedingOpsConstants.DnpQueueAlertThreshold);
            })
            .Where(t => t.DaysPostBreeding >= BreedingOpsConstants.PregnancyCheckMinDays && t.DaysPostBreeding <= BreedingOpsConstants.PregnancyCheckMaxDays)
            .OrderByDescending(t => t.DnpDays)
            .ThenByDescending(t => t.DaysPostBreeding)
            .ToList();

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var total = tasks.Count;
        var pageItems = tasks.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var dnpAlertCount = tasks.Count(t => t.RequiresAttention);

        return Result.Success(new PregnancyCheckQueueResponse(
            pageItems,
            total,
            total,
            dnpAlertCount,
            page,
            pageSize));
    }
}
