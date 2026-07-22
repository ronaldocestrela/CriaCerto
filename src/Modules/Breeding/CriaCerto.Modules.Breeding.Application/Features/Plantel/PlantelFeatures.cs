using CriaCerto.BuildingBlocks.Abstractions.Licensing;
using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.BuildingBlocks.Application.Abstractions.Messaging;
using CriaCerto.Modules.Breeding.Application.Abstractions;
using CriaCerto.Modules.Breeding.Application.Contracts;
using CriaCerto.Modules.Breeding.Application.Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.Modules.Breeding.Application.Features.Plantel;

[RequiresModule("Breeding")]
public sealed record CreateSowCommand(
    string TagId,
    string Breed,
    DateOnly EntryDate,
    decimal EntryWeightKg,
    string? Nickname,
    string? PbbRegistration,
    string? Origin,
    DateOnly? BirthDate,
    decimal? CurrentWeightKg,
    decimal? AverageDailyGain,
    string? FatherTagId,
    string? MotherTagId,
    BodyConditionScore BodyConditionScore,
    int Parity,
    int DnpDays,
    ReproductiveStatus ReproductiveStatus,
    string? Location) : ICommand<SowDetailDto>;

[RequiresModule("Breeding")]
public sealed record UpdateSowCommand(
    Guid Id,
    string TagId,
    string Breed,
    DateOnly EntryDate,
    decimal EntryWeightKg,
    decimal CurrentWeightKg,
    ReproductiveStatus ReproductiveStatus,
    BodyConditionScore BodyConditionScore,
    int Parity,
    int DnpDays,
    string? Nickname,
    string? PbbRegistration,
    string? Origin,
    DateOnly? BirthDate,
    decimal? AverageDailyGain,
    string? FatherTagId,
    string? MotherTagId,
    string? Location) : ICommand<SowDetailDto>;

[RequiresModule("Breeding")]
public sealed record ChangeSowStatusCommand(Guid Id, LifecycleStatus Status, DateOnly EventDate, string? Notes) : ICommand<SowDetailDto>;

[RequiresModule("Breeding")]
public sealed record GetSowQuery(Guid Id) : IQuery<SowDetailDto>;

[RequiresModule("Breeding")]
public sealed record ListSowsQuery(string? Search, ReproductiveStatus? Status, int Page = 1, int PageSize = 25) : IQuery<PlantelListResponse<SowSummaryDto>>;

[RequiresModule("Breeding")]
public sealed record CreateBoarCommand(
    string TagId,
    string Breed,
    DateOnly EntryDate,
    decimal EntryWeightKg,
    string? Nickname,
    string? PbbRegistration,
    string? Origin,
    DateOnly? BirthDate,
    decimal? CurrentWeightKg,
    decimal? AverageDailyGain,
    string? FatherTagId,
    string? MotherTagId,
    BodyConditionScore BodyConditionScore,
    string? Location) : ICommand<BoarDetailDto>;

[RequiresModule("Breeding")]
public sealed record UpdateBoarCommand(
    Guid Id,
    string TagId,
    string Breed,
    DateOnly EntryDate,
    decimal EntryWeightKg,
    decimal CurrentWeightKg,
    BodyConditionScore BodyConditionScore,
    string? Nickname,
    string? PbbRegistration,
    string? Origin,
    DateOnly? BirthDate,
    decimal? AverageDailyGain,
    string? FatherTagId,
    string? MotherTagId,
    string? Location) : ICommand<BoarDetailDto>;

[RequiresModule("Breeding")]
public sealed record ChangeBoarStatusCommand(Guid Id, LifecycleStatus Status, DateOnly EventDate, string? Notes) : ICommand<BoarDetailDto>;

[RequiresModule("Breeding")]
public sealed record GetBoarQuery(Guid Id) : IQuery<BoarDetailDto>;

[RequiresModule("Breeding")]
public sealed record ListBoarsQuery(string? Search, int Page = 1, int PageSize = 25) : IQuery<PlantelListResponse<BoarSummaryDto>>;

public sealed class CreateSowCommandValidator : AbstractValidator<CreateSowCommand>
{
    public CreateSowCommandValidator()
    {
        RuleFor(x => x.TagId).NotEmpty().MaximumLength(40);
        RuleFor(x => x.Breed).NotEmpty().MaximumLength(120);
        RuleFor(x => x.EntryWeightKg).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CurrentWeightKg).GreaterThanOrEqualTo(0).When(x => x.CurrentWeightKg.HasValue);
        RuleFor(x => x.Parity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DnpDays).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateSowCommandValidator : AbstractValidator<UpdateSowCommand>
{
    public UpdateSowCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.TagId).NotEmpty().MaximumLength(40);
        RuleFor(x => x.Breed).NotEmpty().MaximumLength(120);
        RuleFor(x => x.EntryWeightKg).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CurrentWeightKg).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Parity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DnpDays).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreateBoarCommandValidator : AbstractValidator<CreateBoarCommand>
{
    public CreateBoarCommandValidator()
    {
        RuleFor(x => x.TagId).NotEmpty().MaximumLength(40);
        RuleFor(x => x.Breed).NotEmpty().MaximumLength(120);
        RuleFor(x => x.EntryWeightKg).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CurrentWeightKg).GreaterThanOrEqualTo(0).When(x => x.CurrentWeightKg.HasValue);
    }
}

public sealed class UpdateBoarCommandValidator : AbstractValidator<UpdateBoarCommand>
{
    public UpdateBoarCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.TagId).NotEmpty().MaximumLength(40);
        RuleFor(x => x.Breed).NotEmpty().MaximumLength(120);
        RuleFor(x => x.EntryWeightKg).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CurrentWeightKg).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreateSowCommandHandler : IRequestHandler<CreateSowCommand, Result<SowDetailDto>>
{
    private readonly IBreedingDbContext _dbContext;

    public CreateSowCommandHandler(IBreedingDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<SowDetailDto>> Handle(CreateSowCommand request, CancellationToken cancellationToken)
    {
        if (await _dbContext.Sows.AnyAsync(s => s.TagId == Normalize(request.TagId), cancellationToken) ||
            await _dbContext.Boars.AnyAsync(b => b.TagId == Normalize(request.TagId), cancellationToken))
        {
            return Result.Failure<SowDetailDto>(Error.Conflict("Plantel.TagIdAlreadyExists", "Já existe um animal com este identificador no plantel."));
        }

        var sow = new Sow(
            Guid.NewGuid(), request.TagId, request.Breed, request.EntryDate, request.EntryWeightKg,
            request.Nickname, request.PbbRegistration, request.Origin, request.BirthDate,
            request.CurrentWeightKg, request.AverageDailyGain, request.FatherTagId, request.MotherTagId,
            request.BodyConditionScore, request.Parity, request.DnpDays, request.ReproductiveStatus, request.Location);

        _dbContext.Sows.Add(sow);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(sow.ToDetailDto());
    }

    private static string Normalize(string tagId) => tagId.Trim().ToUpperInvariant();
}

public sealed class UpdateSowCommandHandler : IRequestHandler<UpdateSowCommand, Result<SowDetailDto>>
{
    private readonly IBreedingDbContext _dbContext;

    public UpdateSowCommandHandler(IBreedingDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<SowDetailDto>> Handle(UpdateSowCommand request, CancellationToken cancellationToken)
    {
        var sow = await _dbContext.Sows.Include(s => s.Events).FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (sow is null)
        {
            return Result.Failure<SowDetailDto>(Error.NotFound("Sow.NotFound", "Matriz não encontrada."));
        }

        var normalized = request.TagId.Trim().ToUpperInvariant();
        var duplicate = await _dbContext.Sows.AnyAsync(s => s.Id != request.Id && s.TagId == normalized, cancellationToken) ||
            await _dbContext.Boars.AnyAsync(b => b.TagId == normalized, cancellationToken);
        if (duplicate)
        {
            return Result.Failure<SowDetailDto>(Error.Conflict("Plantel.TagIdAlreadyExists", "Já existe um animal com este identificador no plantel."));
        }

        var result = sow.UpdateProfile(
            request.TagId, request.Breed, request.EntryDate, request.EntryWeightKg, request.CurrentWeightKg,
            request.ReproductiveStatus, request.BodyConditionScore, request.Parity, request.DnpDays,
            request.Nickname, request.PbbRegistration, request.Origin, request.BirthDate, request.AverageDailyGain,
            request.FatherTagId, request.MotherTagId, request.Location);
        if (result.IsFailure)
        {
            return Result.Failure<SowDetailDto>(result.Error);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(sow.ToDetailDto());
    }
}

public sealed class ChangeSowStatusCommandHandler : IRequestHandler<ChangeSowStatusCommand, Result<SowDetailDto>>
{
    private readonly IBreedingDbContext _dbContext;

    public ChangeSowStatusCommandHandler(IBreedingDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<SowDetailDto>> Handle(ChangeSowStatusCommand request, CancellationToken cancellationToken)
    {
        var sow = await _dbContext.Sows.Include(s => s.Events).FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (sow is null)
        {
            return Result.Failure<SowDetailDto>(Error.NotFound("Sow.NotFound", "Matriz não encontrada."));
        }

        var result = sow.ChangeLifecycleStatus(request.Status, request.EventDate, request.Notes);
        if (result.IsFailure)
        {
            return Result.Failure<SowDetailDto>(result.Error);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(sow.ToDetailDto());
    }
}

public sealed class GetSowQueryHandler : IRequestHandler<GetSowQuery, Result<SowDetailDto>>
{
    private readonly IBreedingDbContext _dbContext;

    public GetSowQueryHandler(IBreedingDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<SowDetailDto>> Handle(GetSowQuery request, CancellationToken cancellationToken)
    {
        var sow = await _dbContext.Sows.Include(s => s.Events).FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        return sow is null
            ? Result.Failure<SowDetailDto>(Error.NotFound("Sow.NotFound", "Matriz não encontrada."))
            : Result.Success(sow.ToDetailDto());
    }
}

public sealed class ListSowsQueryHandler : IRequestHandler<ListSowsQuery, Result<PlantelListResponse<SowSummaryDto>>>
{
    private readonly IBreedingDbContext _dbContext;

    public ListSowsQueryHandler(IBreedingDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<PlantelListResponse<SowSummaryDto>>> Handle(ListSowsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Sows.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpperInvariant();
            query = query.Where(s => s.TagId.Contains(search) || (s.Nickname != null && s.Nickname.ToUpper().Contains(search)));
        }

        if (request.Status.HasValue)
        {
            query = query.Where(s => s.ReproductiveStatus == request.Status.Value);
        }

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(s => s.RequiresAttention)
            .ThenBy(s => s.TagId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => s.ToSummaryDto())
            .ToListAsync(cancellationToken);

        return Result.Success(new PlantelListResponse<SowSummaryDto>(items, total, page, pageSize));
    }
}

public sealed class CreateBoarCommandHandler : IRequestHandler<CreateBoarCommand, Result<BoarDetailDto>>
{
    private readonly IBreedingDbContext _dbContext;

    public CreateBoarCommandHandler(IBreedingDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<BoarDetailDto>> Handle(CreateBoarCommand request, CancellationToken cancellationToken)
    {
        var normalized = request.TagId.Trim().ToUpperInvariant();
        if (await _dbContext.Boars.AnyAsync(b => b.TagId == normalized, cancellationToken) ||
            await _dbContext.Sows.AnyAsync(s => s.TagId == normalized, cancellationToken))
        {
            return Result.Failure<BoarDetailDto>(Error.Conflict("Plantel.TagIdAlreadyExists", "Já existe um animal com este identificador no plantel."));
        }

        var boar = new Boar(
            Guid.NewGuid(), request.TagId, request.Breed, request.EntryDate, request.EntryWeightKg,
            request.Nickname, request.PbbRegistration, request.Origin, request.BirthDate,
            request.CurrentWeightKg, request.AverageDailyGain, request.FatherTagId, request.MotherTagId,
            request.BodyConditionScore, request.Location);

        _dbContext.Boars.Add(boar);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(boar.ToDetailDto());
    }
}

public sealed class UpdateBoarCommandHandler : IRequestHandler<UpdateBoarCommand, Result<BoarDetailDto>>
{
    private readonly IBreedingDbContext _dbContext;

    public UpdateBoarCommandHandler(IBreedingDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<BoarDetailDto>> Handle(UpdateBoarCommand request, CancellationToken cancellationToken)
    {
        var boar = await _dbContext.Boars.Include(b => b.Events).FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);
        if (boar is null)
        {
            return Result.Failure<BoarDetailDto>(Error.NotFound("Boar.NotFound", "Cachaço não encontrado."));
        }

        var normalized = request.TagId.Trim().ToUpperInvariant();
        var duplicate = await _dbContext.Boars.AnyAsync(b => b.Id != request.Id && b.TagId == normalized, cancellationToken) ||
            await _dbContext.Sows.AnyAsync(s => s.TagId == normalized, cancellationToken);
        if (duplicate)
        {
            return Result.Failure<BoarDetailDto>(Error.Conflict("Plantel.TagIdAlreadyExists", "Já existe um animal com este identificador no plantel."));
        }

        var result = boar.UpdateProfile(
            request.TagId, request.Breed, request.EntryDate, request.EntryWeightKg, request.CurrentWeightKg,
            request.BodyConditionScore, request.Nickname, request.PbbRegistration, request.Origin, request.BirthDate,
            request.AverageDailyGain, request.FatherTagId, request.MotherTagId, request.Location);
        if (result.IsFailure)
        {
            return Result.Failure<BoarDetailDto>(result.Error);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(boar.ToDetailDto());
    }
}

public sealed class ChangeBoarStatusCommandHandler : IRequestHandler<ChangeBoarStatusCommand, Result<BoarDetailDto>>
{
    private readonly IBreedingDbContext _dbContext;

    public ChangeBoarStatusCommandHandler(IBreedingDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<BoarDetailDto>> Handle(ChangeBoarStatusCommand request, CancellationToken cancellationToken)
    {
        var boar = await _dbContext.Boars.Include(b => b.Events).FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);
        if (boar is null)
        {
            return Result.Failure<BoarDetailDto>(Error.NotFound("Boar.NotFound", "Cachaço não encontrado."));
        }

        var result = boar.ChangeLifecycleStatus(request.Status, request.EventDate, request.Notes);
        if (result.IsFailure)
        {
            return Result.Failure<BoarDetailDto>(result.Error);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(boar.ToDetailDto());
    }
}

public sealed class GetBoarQueryHandler : IRequestHandler<GetBoarQuery, Result<BoarDetailDto>>
{
    private readonly IBreedingDbContext _dbContext;

    public GetBoarQueryHandler(IBreedingDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<BoarDetailDto>> Handle(GetBoarQuery request, CancellationToken cancellationToken)
    {
        var boar = await _dbContext.Boars.Include(b => b.Events).FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);
        return boar is null
            ? Result.Failure<BoarDetailDto>(Error.NotFound("Boar.NotFound", "Cachaço não encontrado."))
            : Result.Success(boar.ToDetailDto());
    }
}

public sealed class ListBoarsQueryHandler : IRequestHandler<ListBoarsQuery, Result<PlantelListResponse<BoarSummaryDto>>>
{
    private readonly IBreedingDbContext _dbContext;

    public ListBoarsQueryHandler(IBreedingDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<PlantelListResponse<BoarSummaryDto>>> Handle(ListBoarsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Boars.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpperInvariant();
            query = query.Where(b => b.TagId.Contains(search) || (b.Nickname != null && b.Nickname.ToUpper().Contains(search)));
        }

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(b => b.RequiresAttention)
            .ThenBy(b => b.TagId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => b.ToSummaryDto())
            .ToListAsync(cancellationToken);

        return Result.Success(new PlantelListResponse<BoarSummaryDto>(items, total, page, pageSize));
    }
}

internal static class PlantelMappings
{
    public static SowSummaryDto ToSummaryDto(this Sow sow) => new(
        sow.Id, sow.TagId, sow.Nickname, sow.Breed, sow.ReproductiveStatus, sow.LifecycleStatus,
        sow.Parity, sow.DnpDays, sow.LastEventName, sow.LastEventDate, sow.Location, sow.RequiresAttention);

    public static BoarSummaryDto ToSummaryDto(this Boar boar) => new(
        boar.Id, boar.TagId, boar.Nickname, boar.Breed, boar.LifecycleStatus, boar.CurrentWeightKg,
        boar.LastEventName, boar.LastEventDate, boar.Location, boar.RequiresAttention);

    public static SowDetailDto ToDetailDto(this Sow sow) => new(
        sow.Id, sow.TagId, sow.Nickname, sow.PbbRegistration, sow.Breed, sow.Origin, sow.BirthDate,
        sow.EntryDate, sow.EntryWeightKg, sow.CurrentWeightKg, sow.AverageDailyGain, sow.FatherTagId,
        sow.MotherTagId, sow.BodyConditionScore, sow.Parity, sow.DnpDays, sow.ReproductiveStatus,
        sow.LifecycleStatus, sow.Location, sow.RequiresAttention, sow.Events.OrderByDescending(e => e.EventDate).Select(ToDto).ToList());

    public static BoarDetailDto ToDetailDto(this Boar boar) => new(
        boar.Id, boar.TagId, boar.Nickname, boar.PbbRegistration, boar.Breed, boar.Origin, boar.BirthDate,
        boar.EntryDate, boar.EntryWeightKg, boar.CurrentWeightKg, boar.AverageDailyGain, boar.FatherTagId,
        boar.MotherTagId, boar.BodyConditionScore, boar.LifecycleStatus, boar.Location, boar.RequiresAttention,
        boar.Events.OrderByDescending(e => e.EventDate).Select(ToDto).ToList());

    private static PlantelEventDto ToDto(PlantelEvent plantelEvent) =>
        new(plantelEvent.EventType, plantelEvent.Title, plantelEvent.EventDate, plantelEvent.Notes);
}
