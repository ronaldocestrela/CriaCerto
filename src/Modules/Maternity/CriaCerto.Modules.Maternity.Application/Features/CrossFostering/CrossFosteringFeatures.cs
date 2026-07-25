using CriaCerto.BuildingBlocks.Abstractions.Licensing;
using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.BuildingBlocks.Abstractions.Tenancy;
using CriaCerto.BuildingBlocks.Application.Abstractions.Messaging;
using CriaCerto.Modules.Maternity.Application.Abstractions;
using CriaCerto.Modules.Maternity.Application.Contracts;
using CriaCerto.Modules.Maternity.Application.Domain;
using FluentValidation;
using MediatR;

namespace CriaCerto.Modules.Maternity.Application.Features.CrossFostering;

[RequiresModule("Maternity")]
public sealed record TransferPigletCommand(
    Guid SourceFarrowingId,
    Guid TargetFarrowingId,
    int Quantity,
    DateTime TransferDate,
    string? Notes) : ICommand<PigletTransferDto>;

public sealed class TransferPigletCommandValidator : AbstractValidator<TransferPigletCommand>
{
    public TransferPigletCommandValidator()
    {
        RuleFor(x => x.SourceFarrowingId)
            .NotEmpty()
            .WithMessage("O parto de origem é obrigatório.");

        RuleFor(x => x.TargetFarrowingId)
            .NotEmpty()
            .WithMessage("O parto de destino é obrigatório.");

        RuleFor(x => x)
            .Must(x => x.SourceFarrowingId != x.TargetFarrowingId)
            .WithMessage("O parto de origem e destino não podem ser iguais.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("A quantidade de leitões a transferir deve ser maior que zero.");

        RuleFor(x => x.TransferDate)
            .NotEmpty()
            .WithMessage("A data da transferência é obrigatória.");
    }
}

public sealed class TransferPigletCommandHandler : IRequestHandler<TransferPigletCommand, Result<PigletTransferDto>>
{
    private readonly IFarrowingRepository _farrowingRepository;
    private readonly IPigletTransferRepository _transferRepository;
    private readonly ITenantContext _tenantContext;

    public TransferPigletCommandHandler(
        IFarrowingRepository farrowingRepository,
        IPigletTransferRepository transferRepository,
        ITenantContext tenantContext)
    {
        _farrowingRepository = farrowingRepository;
        _transferRepository = transferRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<PigletTransferDto>> Handle(TransferPigletCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return Result.Failure<PigletTransferDto>(Error.Unauthorized("Tenant.Missing", "Inquilino não identificado."));
        }

        var sourceFarrowing = await _farrowingRepository.GetByIdAsync(request.SourceFarrowingId, cancellationToken);
        if (sourceFarrowing is null)
        {
            return Result.Failure<PigletTransferDto>(FarrowingErrors.NotFound);
        }

        var targetFarrowing = await _farrowingRepository.GetByIdAsync(request.TargetFarrowingId, cancellationToken);
        if (targetFarrowing is null)
        {
            return Result.Failure<PigletTransferDto>(FarrowingErrors.NotFound);
        }

        // Calculate current available live piglets in source farrowing litter
        var outgoingTransfers = await _transferRepository.GetBySourceFarrowingIdAsync(sourceFarrowing.Id, cancellationToken);
        var incomingTransfers = await _transferRepository.GetByTargetFarrowingIdAsync(sourceFarrowing.Id, cancellationToken);

        int netTransfers = incomingTransfers.Sum(t => t.Quantity) - outgoingTransfers.Sum(t => t.Quantity);
        int availablePiglets = sourceFarrowing.LiveBorn + netTransfers;

        if (request.Quantity > availablePiglets)
        {
            return Result.Failure<PigletTransferDto>(FarrowingErrors.InsufficientPigletsInLitter);
        }

        var transferResult = Domain.PigletTransfer.Create(
            _tenantContext.TenantId.Value,
            sourceFarrowing.Id,
            sourceFarrowing.SowId,
            targetFarrowing.Id,
            targetFarrowing.SowId,
            request.Quantity,
            request.TransferDate,
            request.Notes);

        if (transferResult.IsFailure)
        {
            return Result.Failure<PigletTransferDto>(transferResult.Error);
        }

        var transfer = transferResult.Value;
        await _transferRepository.AddAsync(transfer, cancellationToken);
        await _transferRepository.SaveChangesAsync(cancellationToken);

        var dto = new PigletTransferDto(
            transfer.Id,
            transfer.TenantId,
            transfer.SourceFarrowingId,
            transfer.SourceSowId,
            transfer.TargetFarrowingId,
            transfer.TargetSowId,
            transfer.Quantity,
            transfer.TransferDate,
            transfer.Notes);

        return Result.Success(dto);
    }
}

[RequiresModule("Maternity")]
public sealed record ListPigletTransfersQuery(Guid? FarrowingId) : IQuery<List<PigletTransferDto>>;

public sealed class ListPigletTransfersQueryHandler : IRequestHandler<ListPigletTransfersQuery, Result<List<PigletTransferDto>>>
{
    private readonly IPigletTransferRepository _repository;

    public ListPigletTransfersQueryHandler(IPigletTransferRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<PigletTransferDto>>> Handle(ListPigletTransfersQuery request, CancellationToken cancellationToken)
    {
        List<Domain.PigletTransfer> transfers;

        if (request.FarrowingId.HasValue)
        {
            var outgoing = await _repository.GetBySourceFarrowingIdAsync(request.FarrowingId.Value, cancellationToken);
            var incoming = await _repository.GetByTargetFarrowingIdAsync(request.FarrowingId.Value, cancellationToken);
            transfers = outgoing.Concat(incoming).DistinctBy(t => t.Id).ToList();
        }
        else
        {
            transfers = await _repository.GetAllAsync(cancellationToken);
        }

        var dtos = transfers.Select(t => new PigletTransferDto(
            t.Id,
            t.TenantId,
            t.SourceFarrowingId,
            t.SourceSowId,
            t.TargetFarrowingId,
            t.TargetSowId,
            t.Quantity,
            t.TransferDate,
            t.Notes)).ToList();

        return Result.Success(dtos);
    }
}
