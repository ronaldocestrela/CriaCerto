using CriaCerto.BuildingBlocks.Abstractions.Results;

namespace CriaCerto.Modules.Sanitary.Application.Domain;

public static class SanitaryErrors
{
    public static readonly Error InvalidCampaignDates = Error.Validation(
        "Sanitary.InvalidCampaignDates",
        "A data de término da campanha sanitária deve ser posterior à data de início.");

    public static readonly Error InvalidWithdrawalDays = Error.Validation(
        "Sanitary.InvalidWithdrawalDays",
        "O período de carência em dias não pode ser negativo.");

    public static readonly Error EmptyAnimalOrLot = Error.Validation(
        "Sanitary.EmptyAnimalOrLot",
        "Deve ser informado o ID do animal ou o ID do lote para aplicar o tratamento.");

    public static readonly Error CampaignNotFound = Error.NotFound(
        "Sanitary.CampaignNotFound",
        "Campanha sanitária não encontrada.");

    public static readonly Error ActiveSlaughterWithdrawalPeriod = Error.Conflict(
        "Sanitary.ActiveSlaughterWithdrawalPeriod",
        "O animal possui tratamento veterinário ativo dentro do período de carência sanitária e não pode ser despachado para abate.");
}
