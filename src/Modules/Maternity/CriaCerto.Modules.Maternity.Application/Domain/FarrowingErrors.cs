using CriaCerto.BuildingBlocks.Abstractions.Results;

namespace CriaCerto.Modules.Maternity.Application.Domain;

public static class FarrowingErrors
{
    public static readonly Error ZeroTotalBorn = Error.Validation(
        "Farrowing.ZeroTotalBorn",
        "O total de leitões nascidos (vivos + natimortos + mumificados) deve ser maior que zero.");

    public static readonly Error NegativeCounts = Error.Validation(
        "Farrowing.NegativeCounts",
        "As contagens de leitões (vivos, natimortos e mumificados) não podem ser negativas.");

    public static readonly Error InvalidLitterWeight = Error.Validation(
        "Farrowing.InvalidLitterWeight",
        "Quando há leitões nascidos vivos, o peso total da ninhada deve ser maior que zero.");

    public static readonly Error UnrealisticWeight = Error.Validation(
        "Farrowing.UnrealisticWeight",
        "O peso médio por leitão nascido vivo deve estar entre 0.3 kg e 3.5 kg.");

    public static readonly Error NotFound = Error.NotFound(
        "Farrowing.NotFound",
        "O registo de parto especificado não foi encontrado.");

    public static readonly Error SowNotEligible = Error.Conflict(
        "Farrowing.SowNotEligible",
        "A matriz não está elegível para registo de parto.");

    public static readonly Error SameSourceAndTarget = Error.Validation(
        "PigletTransfer.SameSourceAndTarget",
        "A matriz de origem e destino da adoção não podem ser a mesma.");

    public static readonly Error InvalidTransferQuantity = Error.Validation(
        "PigletTransfer.InvalidQuantity",
        "A quantidade de leitões a transferir deve ser maior que zero.");

    public static readonly Error InsufficientPigletsInLitter = Error.Conflict(
        "PigletTransfer.InsufficientPiglets",
        "A ninhada de origem não possui leitões vivos suficientes para esta transferência.");

    public static readonly Error InvalidWeaningCount = Error.Validation(
        "Weaning.InvalidCount",
        "A quantidade de leitões desmamados deve ser maior que zero.");

    public static readonly Error InvalidWeanedWeight = Error.Validation(
        "Weaning.InvalidWeight",
        "O peso total desmamado deve ser maior que zero.");

    public static readonly Error UnrealisticWeanedWeight = Error.Validation(
        "Weaning.UnrealisticWeanedWeight",
        "O peso médio por leitão desmamado deve estar entre 4.0 kg e 12.0 kg.");

    public static readonly Error FarrowingAlreadyWeaned = Error.Conflict(
        "Weaning.AlreadyWeaned",
        "Este parto já possui registro de desmame concluído.");
}

