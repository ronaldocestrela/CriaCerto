using CriaCerto.BuildingBlocks.Abstractions.Licensing;
using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.BuildingBlocks.Application.Abstractions.Messaging;
using CriaCerto.Modules.Maternity.Application.Abstractions;
using CriaCerto.Modules.Maternity.Application.Contracts;
using MediatR;

namespace CriaCerto.Modules.Maternity.Application.Features.Metrics;

[RequiresModule("Maternity")]
public sealed record GetMaternityMetricsQuery(DateTime? StartDate = null, DateTime? EndDate = null) : IQuery<MaternityMetricsDto>;

public sealed class GetMaternityMetricsQueryHandler : IRequestHandler<GetMaternityMetricsQuery, Result<MaternityMetricsDto>>
{
    private readonly IFarrowingRepository _farrowingRepository;
    private readonly IPigletTransferRepository _transferRepository;
    private readonly IWeaningRepository _weaningRepository;

    public GetMaternityMetricsQueryHandler(
        IFarrowingRepository farrowingRepository,
        IPigletTransferRepository transferRepository,
        IWeaningRepository weaningRepository)
    {
        _farrowingRepository = farrowingRepository;
        _transferRepository = transferRepository;
        _weaningRepository = weaningRepository;
    }

    public async Task<Result<MaternityMetricsDto>> Handle(GetMaternityMetricsQuery request, CancellationToken cancellationToken)
    {
        var farrowings = await _farrowingRepository.GetAllAsync(cancellationToken);
        var transfers = await _transferRepository.GetAllAsync(cancellationToken);
        var weanings = await _weaningRepository.GetAllAsync(cancellationToken);

        if (request.StartDate.HasValue)
        {
            farrowings = farrowings.Where(f => f.FarrowingDate >= request.StartDate.Value).ToList();
            transfers = transfers.Where(t => t.TransferDate >= request.StartDate.Value).ToList();
            weanings = weanings.Where(w => w.WeaningDate >= request.StartDate.Value).ToList();
        }

        if (request.EndDate.HasValue)
        {
            farrowings = farrowings.Where(f => f.FarrowingDate <= request.EndDate.Value).ToList();
            transfers = transfers.Where(t => t.TransferDate <= request.EndDate.Value).ToList();
            weanings = weanings.Where(w => w.WeaningDate <= request.EndDate.Value).ToList();
        }

        int totalLiveBorn = farrowings.Sum(f => f.LiveBorn);
        int totalWeaned = weanings.Sum(w => w.WeanedCount);
        int totalTransferred = transfers.Sum(t => t.Quantity);

        var sowIds = farrowings.Select(f => f.SowId).Concat(weanings.Select(w => w.SowId)).Distinct().ToList();
        int activeSowCount = sowIds.Count > 0 ? sowIds.Count : 1;

        // Calculate annualized metrics (assuming sample standard period or scaling)
        decimal nvma = Math.Round((decimal)totalLiveBorn / activeSowCount, 2);
        decimal dma = Math.Round((decimal)totalWeaned / activeSowCount, 2);

        decimal mortalityRate = 0m;
        if (totalLiveBorn > 0)
        {
            int deaths = totalLiveBorn - totalWeaned;
            if (deaths > 0)
            {
                mortalityRate = Math.Round((decimal)deaths / totalLiveBorn * 100m, 2);
            }
        }

        var metrics = new MaternityMetricsDto(
            Nvma: nvma,
            Dma: dma,
            PreWeaningMortalityRate: mortalityRate,
            TotalActiveSows: sowIds.Count,
            TotalLiveBornInPeriod: totalLiveBorn,
            TotalWeanedInPeriod: totalWeaned,
            TotalTransferredInPeriod: totalTransferred);

        return Result.Success(metrics);
    }
}
