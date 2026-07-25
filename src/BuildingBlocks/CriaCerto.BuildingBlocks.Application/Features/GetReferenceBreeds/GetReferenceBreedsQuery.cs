using CriaCerto.BuildingBlocks.Abstractions.ReferenceData;
using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.BuildingBlocks.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.BuildingBlocks.Application.Features.GetReferenceBreeds;

public sealed record BovineBreedDto(
    Guid Id,
    string Code,
    string Name,
    string Category,
    string Aptitude,
    string Origin,
    bool IsOfficial);

public sealed record GetReferenceBreedsQuery : IRequest<Result<List<BovineBreedDto>>>;

public sealed class GetReferenceBreedsQueryHandler : IRequestHandler<GetReferenceBreedsQuery, Result<List<BovineBreedDto>>>
{
    private readonly IFoundationDbContext _dbContext;

    public GetReferenceBreedsQueryHandler(IFoundationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<BovineBreedDto>>> Handle(GetReferenceBreedsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var list = await _dbContext.BovineBreeds
                .AsNoTracking()
                .OrderBy(b => b.Name)
                .Select(b => new BovineBreedDto(
                    b.Id,
                    b.Code,
                    b.Name,
                    b.Category,
                    b.Aptitude,
                    b.Origin,
                    b.IsOfficial))
                .ToListAsync(cancellationToken);

            return Result.Success(list);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<BovineBreedDto>>(
                Error.Failure("Breeds.QueryError", ex.Message));
        }
    }
}
