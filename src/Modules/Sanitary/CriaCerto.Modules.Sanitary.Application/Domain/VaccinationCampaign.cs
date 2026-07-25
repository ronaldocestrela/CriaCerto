using CriaCerto.BuildingBlocks.Abstractions.Results;

namespace CriaCerto.Modules.Sanitary.Application.Domain;

public sealed class VaccinationCampaign
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public CampaignType Type { get; private set; }
    public DateTime StartDateUtc { get; private set; }
    public DateTime EndDateUtc { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private VaccinationCampaign() { }

    public static Result<VaccinationCampaign> Create(
        string name,
        CampaignType type,
        DateTime startDateUtc,
        DateTime endDateUtc,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<VaccinationCampaign>(Error.Validation("Sanitary.EmptyName", "O nome da campanha é obrigatório."));

        if (endDateUtc <= startDateUtc)
            return Result.Failure<VaccinationCampaign>(SanitaryErrors.InvalidCampaignDates);

        var campaign = new VaccinationCampaign
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Type = type,
            StartDateUtc = startDateUtc,
            EndDateUtc = endDateUtc,
            Description = description?.Trim(),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        return Result.Success(campaign);
    }
}
