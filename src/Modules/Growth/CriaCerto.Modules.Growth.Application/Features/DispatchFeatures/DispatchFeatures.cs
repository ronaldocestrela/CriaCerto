using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.BuildingBlocks.Application.Abstractions.Messaging;
using CriaCerto.Modules.Growth.Application.Abstractions;
using CriaCerto.Modules.Growth.Application.Domain;
using CriaCerto.Modules.Sanitary.Application.Contracts;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.Modules.Growth.Application.Features.DispatchFeatures;

public sealed record AnimalDispatchResultDto(
    Guid DispatchId,
    Guid TenantId,
    Guid? AnimalId,
    Guid? LotId,
    string EarTagOrLotCode,
    string Destination,
    DateTime DispatchDate,
    bool IsSlaughter,
    string Status);

public sealed record DispatchAnimalCommand(
    Guid TenantId,
    Guid AnimalId,
    string AnimalEarTag,
    string Destination,
    DateTime DispatchDate,
    bool IsSlaughter) : ICommand<AnimalDispatchResultDto>;

public sealed record DispatchLotCommand(
    Guid TenantId,
    Guid LotId,
    string Destination,
    DateTime DispatchDate,
    bool IsSlaughter) : ICommand<AnimalDispatchResultDto>;

public sealed class DispatchAnimalCommandValidator : AbstractValidator<DispatchAnimalCommand>
{
    public DispatchAnimalCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.AnimalId).NotEmpty();
        RuleFor(x => x.AnimalEarTag).NotEmpty();
        RuleFor(x => x.Destination).NotEmpty().MaximumLength(150);
    }
}

public sealed class DispatchLotCommandValidator : AbstractValidator<DispatchLotCommand>
{
    public DispatchLotCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.LotId).NotEmpty();
        RuleFor(x => x.Destination).NotEmpty().MaximumLength(150);
    }
}

public sealed class DispatchAnimalCommandHandler : IRequestHandler<DispatchAnimalCommand, Result<AnimalDispatchResultDto>>
{
    private readonly ISender _sender;

    public DispatchAnimalCommandHandler(ISender sender)
    {
        _sender = sender;
    }

    public async Task<Result<AnimalDispatchResultDto>> Handle(DispatchAnimalCommand request, CancellationToken cancellationToken)
    {
        if (request.IsSlaughter)
        {
            var eligibilityResult = await _sender.Send(new ValidateSlaughterEligibilityQuery(request.AnimalId), cancellationToken);
            if (eligibilityResult.IsFailure)
            {
                return Result.Failure<AnimalDispatchResultDto>(eligibilityResult.Error);
            }
        }

        var dispatchDto = new AnimalDispatchResultDto(
            Guid.NewGuid(),
            request.TenantId,
            request.AnimalId,
            null,
            request.AnimalEarTag,
            request.Destination,
            request.DispatchDate,
            request.IsSlaughter,
            "Despachado");

        return Result.Success(dispatchDto);
    }
}

public sealed class DispatchLotCommandHandler : IRequestHandler<DispatchLotCommand, Result<AnimalDispatchResultDto>>
{
    private readonly IGrowthDbContext _growthDb;

    public DispatchLotCommandHandler(IGrowthDbContext growthDb)
    {
        _growthDb = growthDb;
    }

    public async Task<Result<AnimalDispatchResultDto>> Handle(DispatchLotCommand request, CancellationToken cancellationToken)
    {
        var lot = await _growthDb.Lots.FirstOrDefaultAsync(l => l.Id == request.LotId && l.TenantId == request.TenantId, cancellationToken);
        if (lot is null)
            return Result.Failure<AnimalDispatchResultDto>(Error.NotFound("Lot.NotFound", "Lote não encontrado."));

        var closeResult = lot.CloseLot();
        if (closeResult.IsFailure)
            return Result.Failure<AnimalDispatchResultDto>(closeResult.Error);

        await _growthDb.SaveChangesAsync(cancellationToken);

        var dispatchDto = new AnimalDispatchResultDto(
            Guid.NewGuid(),
            request.TenantId,
            null,
            lot.Id,
            lot.Code,
            request.Destination,
            request.DispatchDate,
            request.IsSlaughter,
            "Lote Despachado");

        return Result.Success(dispatchDto);
    }
}
