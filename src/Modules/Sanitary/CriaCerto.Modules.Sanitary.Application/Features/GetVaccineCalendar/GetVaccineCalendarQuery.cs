using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.Modules.Sanitary.Application.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.Modules.Sanitary.Application.Features.GetVaccineCalendar;

public sealed record VaccineReferenceDto(
    Guid Id,
    string Code,
    string DiseaseName,
    string CommercialCategory,
    bool IsMandatoryMAPA,
    string TargetAudience,
    int? RecommendedAgeMonths,
    int? BoosterIntervalDays,
    int DefaultWithdrawalDays,
    string Notes);

public sealed record GetVaccineCalendarQuery : IRequest<Result<List<VaccineReferenceDto>>>;

public sealed class GetVaccineCalendarQueryHandler : IRequestHandler<GetVaccineCalendarQuery, Result<List<VaccineReferenceDto>>>
{
    private readonly ISanitaryDbContext _dbContext;

    public GetVaccineCalendarQueryHandler(ISanitaryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<VaccineReferenceDto>>> Handle(GetVaccineCalendarQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var vaccines = await _dbContext.VaccineReferences
                .AsNoTracking()
                .OrderByDescending(v => v.IsMandatoryMAPA)
                .ThenBy(v => v.DiseaseName)
                .Select(v => new VaccineReferenceDto(
                    v.Id,
                    v.Code,
                    v.DiseaseName,
                    v.CommercialCategory,
                    v.IsMandatoryMAPA,
                    v.TargetAudience,
                    v.RecommendedAgeMonths,
                    v.BoosterIntervalDays,
                    v.DefaultWithdrawalDays,
                    v.Notes))
                .ToListAsync(cancellationToken);

            return Result.Success(vaccines);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<VaccineReferenceDto>>(
                Error.Failure("Sanitary.VaccineCalendarError", ex.Message));
        }
    }
}
