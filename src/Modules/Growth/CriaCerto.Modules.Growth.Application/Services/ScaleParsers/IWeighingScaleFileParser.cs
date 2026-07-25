using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.Modules.Growth.Application.Contracts;

namespace CriaCerto.Modules.Growth.Application.Services.ScaleParsers;

public sealed record ParsedWeighingRow(
    int RowNumber,
    string AnimalTagId,
    decimal WeightKg,
    DateTime? WeighingDate,
    decimal? CarcassYieldPercentage,
    string Notes,
    bool IsValid,
    string? ErrorMessage);

public interface IWeighingScaleFileParser
{
    ScaleModelEnum ScaleModel { get; }
    Result<List<ParsedWeighingRow>> Parse(Stream fileStream, string fileName);
}
