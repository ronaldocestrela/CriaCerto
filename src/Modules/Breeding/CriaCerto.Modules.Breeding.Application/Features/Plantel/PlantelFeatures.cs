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

namespace CriaCerto.Modules.Breeding.Application.Features.Plantel;

[RequiresModule("Breeding")]
public sealed record CreateCowCommand(
    string EarTag,
    string Breed,
    DateTime BirthDate,
    Guid TenantId,
    string? SisbovId = null,
    string? RfidTag = null,
    string? Tattoo = null) : ICommand<CowDetailDto>;

[RequiresModule("Breeding")]
public sealed record GetCowQuery(Guid Id) : IQuery<CowDetailDto>;

[RequiresModule("Breeding")]
public sealed record ListCowsQuery(string? Search, ReproductiveStatus? Status, int Page = 1, int PageSize = 25) : IQuery<CattleListResponse<CowSummaryDto>>;

public sealed class CreateCowCommandValidator : AbstractValidator<CreateCowCommand>
{
    public CreateCowCommandValidator()
    {
        RuleFor(x => x.EarTag).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Breed).NotEmpty().MaximumLength(100);
        RuleFor(x => x.BirthDate).LessThanOrEqualTo(DateTime.UtcNow);
    }
}

public sealed class CreateCowCommandHandler : IRequestHandler<CreateCowCommand, Result<CowDetailDto>>
{
    private readonly IBreedingDbContext _dbContext;

    public CreateCowCommandHandler(IBreedingDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<CowDetailDto>> Handle(CreateCowCommand request, CancellationToken cancellationToken)
    {
        var normalizedEarTag = request.EarTag.Trim().ToUpperInvariant();
        if (await _dbContext.Cows.AnyAsync(c => c.EarTag.ToUpper() == normalizedEarTag, cancellationToken))
        {
            return Result.Failure<CowDetailDto>(Error.Conflict("Cow.EarTagAlreadyExists", "Já existe uma matriz cadastrada com este brinco."));
        }

        var cowResult = Cow.Create(request.EarTag, request.Breed, request.BirthDate, request.TenantId, request.SisbovId, request.RfidTag, request.Tattoo);
        if (cowResult.IsFailure)
            return Result.Failure<CowDetailDto>(cowResult.Error);

        _dbContext.Cows.Add(cowResult.Value);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(cowResult.Value.ToDetailDto());
    }
}

public sealed class GetCowQueryHandler : IRequestHandler<GetCowQuery, Result<CowDetailDto>>
{
    private readonly IBreedingDbContext _dbContext;

    public GetCowQueryHandler(IBreedingDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<CowDetailDto>> Handle(GetCowQuery request, CancellationToken cancellationToken)
    {
        var cow = await _dbContext.Cows.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (cow is null)
            return Result.Failure<CowDetailDto>(Error.NotFound("Cow.NotFound", "Matriz bovina não encontrada."));

        return Result.Success(cow.ToDetailDto());
    }
}

public sealed class ListCowsQueryHandler : IRequestHandler<ListCowsQuery, Result<CattleListResponse<CowSummaryDto>>>
{
    private readonly IBreedingDbContext _dbContext;

    public ListCowsQueryHandler(IBreedingDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<CattleListResponse<CowSummaryDto>>> Handle(ListCowsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Cows.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpperInvariant();
            query = query.Where(c => c.EarTag.ToUpper().Contains(search) ||
                                     (c.SisbovId != null && c.SisbovId.ToUpper().Contains(search)) ||
                                     (c.RfidTag != null && c.RfidTag.ToUpper().Contains(search)));
        }

        if (request.Status.HasValue)
        {
            query = query.Where(c => c.Status == request.Status.Value);
        }

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(c => c.EarTag)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => c.ToSummaryDto())
            .ToListAsync(cancellationToken);

        return Result.Success(new CattleListResponse<CowSummaryDto>(items, total, page, pageSize));
    }
}

internal static class CowMappings
{
    public static CowSummaryDto ToSummaryDto(this Cow cow) => new(
        cow.Id,
        cow.EarTag,
        cow.SisbovId,
        cow.RfidTag,
        cow.Breed,
        cow.Status,
        cow.ParityCount,
        cow.LastCalvingDate,
        IepCalculator.CalculateIepMonths(null, cow.LastCalvingDate ?? DateTime.UtcNow));

    public static CowDetailDto ToDetailDto(this Cow cow) => new(
        cow.Id,
        cow.EarTag,
        cow.SisbovId,
        cow.RfidTag,
        cow.Tattoo,
        cow.Breed,
        cow.BirthDate,
        cow.Status,
        cow.ParityCount,
        cow.LastCalvingDate,
        IepCalculator.CalculateIepMonths(null, cow.LastCalvingDate ?? DateTime.UtcNow),
        IepCalculator.CalculateOpenDays(cow.LastCalvingDate, DateTime.UtcNow));
}
