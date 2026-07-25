using CriaCerto.BuildingBlocks.Abstractions.Licensing;
using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.BuildingBlocks.Abstractions.Tenancy;
using CriaCerto.BuildingBlocks.Application.Abstractions.Messaging;
using CriaCerto.Modules.Maternity.Application.Abstractions;
using CriaCerto.Modules.Maternity.Application.Contracts;
using CriaCerto.Modules.Maternity.Application.Domain;
using FluentValidation;
using MediatR;

namespace CriaCerto.Modules.Maternity.Application.Features.Weaning;

[RequiresModule("Maternity")]
public sealed record RegisterWeaningCommand(
    Guid FarrowingId,
    DateTime WeaningDate,
    int WeanedCount,
    decimal TotalWeanedWeightKg,
    string DestinationPenOrBatch,
    string? Notes) : ICommand<WeaningDto>;

public sealed class RegisterWeaningCommandValidator : AbstractValidator<RegisterWeaningCommand>
{
    public RegisterWeaningCommandValidator()
    {
        RuleFor(x => x.FarrowingId)
            .NotEmpty()
            .WithMessage("O ID do parto é obrigatório.");

        RuleFor(x => x.WeaningDate)
            .NotEmpty()
            .WithMessage("A data do desmame é obrigatória.");

        RuleFor(x => x.WeanedCount)
            .GreaterThan(0)
            .WithMessage("A quantidade de leitões desmamados deve ser maior que zero.");

        RuleFor(x => x.TotalWeanedWeightKg)
            .GreaterThan(0)
            .WithMessage("O peso total desmamado deve ser maior que zero.");

        RuleFor(x => x.DestinationPenOrBatch)
            .NotEmpty()
            .WithMessage("A baia ou lote de destino é obrigatório.");
    }
}

public sealed class RegisterWeaningCommandHandler : IRequestHandler<RegisterWeaningCommand, Result<WeaningDto>>
{
    private readonly IFarrowingRepository _farrowingRepository;
    private readonly IPigletTransferRepository _transferRepository;
    private readonly IWeaningRepository _weaningRepository;
    private readonly ITenantContext _tenantContext;

    public RegisterWeaningCommandHandler(
        IFarrowingRepository farrowingRepository,
        IPigletTransferRepository transferRepository,
        IWeaningRepository weaningRepository,
        ITenantContext tenantContext)
    {
        _farrowingRepository = farrowingRepository;
        _transferRepository = transferRepository;
        _weaningRepository = weaningRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<WeaningDto>> Handle(RegisterWeaningCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return Result.Failure<WeaningDto>(Error.Unauthorized("Tenant.Missing", "Inquilino não identificado."));
        }

        var farrowing = await _farrowingRepository.GetByIdAsync(request.FarrowingId, cancellationToken);
        if (farrowing is null)
        {
            return Result.Failure<WeaningDto>(FarrowingErrors.NotFound);
        }

        var existingWeaning = await _weaningRepository.GetByFarrowingIdAsync(farrowing.Id, cancellationToken);
        if (existingWeaning is not null)
        {
            return Result.Failure<WeaningDto>(FarrowingErrors.FarrowingAlreadyWeaned);
        }

        // Calculate available live piglets in litter
        var outgoingTransfers = await _transferRepository.GetBySourceFarrowingIdAsync(farrowing.Id, cancellationToken);
        var incomingTransfers = await _transferRepository.GetByTargetFarrowingIdAsync(farrowing.Id, cancellationToken);

        int netTransfers = incomingTransfers.Sum(t => t.Quantity) - outgoingTransfers.Sum(t => t.Quantity);
        int availablePiglets = farrowing.LiveBorn + netTransfers;

        if (request.WeanedCount > availablePiglets)
        {
            return Result.Failure<WeaningDto>(FarrowingErrors.InsufficientPigletsInLitter);
        }

        var weaningResult = Domain.Weaning.Create(
            _tenantContext.TenantId.Value,
            farrowing.Id,
            farrowing.SowId,
            request.WeaningDate,
            request.WeanedCount,
            request.TotalWeanedWeightKg,
            request.DestinationPenOrBatch,
            request.Notes);

        if (weaningResult.IsFailure)
        {
            return Result.Failure<WeaningDto>(weaningResult.Error);
        }

        var weaning = weaningResult.Value;
        await _weaningRepository.AddAsync(weaning, cancellationToken);
        await _weaningRepository.SaveChangesAsync(cancellationToken);

        var dto = new WeaningDto(
            weaning.Id,
            weaning.TenantId,
            weaning.FarrowingId,
            weaning.SowId,
            weaning.WeaningDate,
            weaning.WeanedCount,
            weaning.TotalWeanedWeightKg,
            weaning.AverageWeanedWeightKg,
            weaning.DestinationPenOrBatch,
            weaning.Notes);

        return Result.Success(dto);
    }
}

[RequiresModule("Maternity")]
public sealed record ListWeaningsQuery(Guid? SowId) : IQuery<List<WeaningDto>>;

public sealed class ListWeaningsQueryHandler : IRequestHandler<ListWeaningsQuery, Result<List<WeaningDto>>>
{
    private readonly IWeaningRepository _repository;

    public ListWeaningsQueryHandler(IWeaningRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<WeaningDto>>> Handle(ListWeaningsQuery request, CancellationToken cancellationToken)
    {
        List<Domain.Weaning> weanings;

        if (request.SowId.HasValue)
        {
            weanings = await _repository.GetBySowIdAsync(request.SowId.Value, cancellationToken);
        }
        else
        {
            weanings = await _repository.GetAllAsync(cancellationToken);
        }

        var dtos = weanings.Select(w => new WeaningDto(
            w.Id,
            w.TenantId,
            w.FarrowingId,
            w.SowId,
            w.WeaningDate,
            w.WeanedCount,
            w.TotalWeanedWeightKg,
            w.AverageWeanedWeightKg,
            w.DestinationPenOrBatch,
            w.Notes)).ToList();

        return Result.Success(dtos);
    }
}
