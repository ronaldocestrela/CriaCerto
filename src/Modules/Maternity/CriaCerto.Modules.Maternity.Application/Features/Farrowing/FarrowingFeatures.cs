using CriaCerto.BuildingBlocks.Abstractions.Licensing;
using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.BuildingBlocks.Abstractions.Tenancy;
using CriaCerto.BuildingBlocks.Application.Abstractions.Messaging;
using CriaCerto.Modules.Maternity.Application.Abstractions;
using CriaCerto.Modules.Maternity.Application.Contracts;
using CriaCerto.Modules.Maternity.Application.Domain;
using FluentValidation;
using MediatR;

namespace CriaCerto.Modules.Maternity.Application.Features.Farrowing;

[RequiresModule("Maternity")]
public sealed record RegisterFarrowingCommand(
    Guid SowId,
    DateTime FarrowingDate,
    int LiveBorn,
    int Stillborn,
    int Mummified,
    decimal LitterWeightKg,
    string? MaternityRoomId,
    bool Assisted,
    string? Notes) : ICommand<FarrowingDto>;

public sealed class RegisterFarrowingCommandValidator : AbstractValidator<RegisterFarrowingCommand>
{
    public RegisterFarrowingCommandValidator()
    {
        RuleFor(x => x.SowId)
            .NotEmpty()
            .WithMessage("O ID da matriz é obrigatório.");

        RuleFor(x => x.FarrowingDate)
            .NotEmpty()
            .WithMessage("A data do parto é obrigatória.");

        RuleFor(x => x.LiveBorn)
            .GreaterThanOrEqualTo(0)
            .WithMessage("O número de nascidos vivos não pode ser negativo.");

        RuleFor(x => x.Stillborn)
            .GreaterThanOrEqualTo(0)
            .WithMessage("O número de natimortos não pode ser negativo.");

        RuleFor(x => x.Mummified)
            .GreaterThanOrEqualTo(0)
            .WithMessage("O número de mumificados não pode ser negativo.");

        RuleFor(x => x.LiveBorn + x.Stillborn + x.Mummified)
            .GreaterThan(0)
            .WithMessage("O total de leitões nascidos (vivos + natimortos + mumificados) deve ser maior que zero.");

        RuleFor(x => x.LitterWeightKg)
            .GreaterThan(0)
            .When(x => x.LiveBorn > 0)
            .WithMessage("O peso total da ninhada deve ser maior que zero quando há leitões nascidos vivos.");
    }
}

public sealed class RegisterFarrowingCommandHandler : IRequestHandler<RegisterFarrowingCommand, Result<FarrowingDto>>
{
    private readonly IFarrowingRepository _repository;
    private readonly ITenantContext _tenantContext;

    public RegisterFarrowingCommandHandler(IFarrowingRepository repository, ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<FarrowingDto>> Handle(RegisterFarrowingCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return Result.Failure<FarrowingDto>(Error.Unauthorized("Tenant.Missing", "Inquilino não identificado."));
        }

        var farrowingResult = Domain.Farrowing.Create(
            request.SowId,
            _tenantContext.TenantId.Value,
            request.FarrowingDate,
            request.LiveBorn,
            request.Stillborn,
            request.Mummified,
            request.LitterWeightKg,
            request.MaternityRoomId,
            request.Assisted,
            request.Notes);

        if (farrowingResult.IsFailure)
        {
            return Result.Failure<FarrowingDto>(farrowingResult.Error);
        }

        var farrowing = farrowingResult.Value;
        await _repository.AddAsync(farrowing, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var dto = new FarrowingDto(
            farrowing.Id,
            farrowing.SowId,
            farrowing.TenantId,
            farrowing.FarrowingDate,
            farrowing.LiveBorn,
            farrowing.Stillborn,
            farrowing.Mummified,
            farrowing.TotalBorn,
            farrowing.LitterWeightKg,
            farrowing.AveragePigletWeightKg,
            farrowing.MaternityRoomId,
            farrowing.Assisted,
            farrowing.Notes);

        return Result.Success(dto);
    }
}

[RequiresModule("Maternity")]
public sealed record GetFarrowingByIdQuery(Guid Id) : IQuery<FarrowingDto>;

public sealed class GetFarrowingByIdQueryHandler : IRequestHandler<GetFarrowingByIdQuery, Result<FarrowingDto>>
{
    private readonly IFarrowingRepository _repository;

    public GetFarrowingByIdQueryHandler(IFarrowingRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<FarrowingDto>> Handle(GetFarrowingByIdQuery request, CancellationToken cancellationToken)
    {
        var farrowing = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (farrowing is null)
        {
            return Result.Failure<FarrowingDto>(FarrowingErrors.NotFound);
        }

        var dto = new FarrowingDto(
            farrowing.Id,
            farrowing.SowId,
            farrowing.TenantId,
            farrowing.FarrowingDate,
            farrowing.LiveBorn,
            farrowing.Stillborn,
            farrowing.Mummified,
            farrowing.TotalBorn,
            farrowing.LitterWeightKg,
            farrowing.AveragePigletWeightKg,
            farrowing.MaternityRoomId,
            farrowing.Assisted,
            farrowing.Notes);

        return Result.Success(dto);
    }
}

[RequiresModule("Maternity")]
public sealed record ListFarrowingsQuery(Guid? SowId, string? MaternityRoomId) : IQuery<List<FarrowingSummaryDto>>;

public sealed class ListFarrowingsQueryHandler : IRequestHandler<ListFarrowingsQuery, Result<List<FarrowingSummaryDto>>>
{
    private readonly IFarrowingRepository _repository;

    public ListFarrowingsQueryHandler(IFarrowingRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<FarrowingSummaryDto>>> Handle(ListFarrowingsQuery request, CancellationToken cancellationToken)
    {
        List<Domain.Farrowing> farrowings;

        if (request.SowId.HasValue)
        {
            farrowings = await _repository.GetBySowIdAsync(request.SowId.Value, cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(request.MaternityRoomId))
        {
            farrowings = await _repository.GetByMaternityRoomAsync(request.MaternityRoomId, cancellationToken);
        }
        else
        {
            farrowings = await _repository.GetAllAsync(cancellationToken);
        }

        var summaries = farrowings
            .Select(f => new FarrowingSummaryDto(
                f.Id,
                f.SowId,
                f.FarrowingDate,
                f.LiveBorn,
                f.Stillborn,
                f.Mummified,
                f.TotalBorn,
                f.LitterWeightKg,
                f.MaternityRoomId))
            .ToList();

        return Result.Success(summaries);
    }
}
