using System.Globalization;
using System.Text;
using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.Modules.Growth.Application.Contracts;

namespace CriaCerto.Modules.Growth.Application.Services.ScaleParsers;

public sealed class CoimmaScaleParser : IWeighingScaleFileParser
{
    public ScaleModelEnum ScaleModel => ScaleModelEnum.Coimma;

    public Result<List<ParsedWeighingRow>> Parse(Stream fileStream, string fileName)
    {
        var rows = new List<ParsedWeighingRow>();
        using var reader = new StreamReader(fileStream, Encoding.UTF8, true, leaveOpen: true);

        string? line;
        int rowNumber = 0;
        int tagIndex = -1, weightIndex = -1, dateIndex = -1;

        while ((line = reader.ReadLine()) != null)
        {
            rowNumber++;
            if (string.IsNullOrWhiteSpace(line)) continue;

            var parts = line.Split(new[] { ';', ',', '\t' }, StringSplitOptions.TrimEntries);

            if (rowNumber == 1)
            {
                for (int i = 0; i < parts.Length; i++)
                {
                    string col = parts[i].Trim().ToLowerInvariant();
                    if (col is "brinco" or "tag" or "id" or "animal") tagIndex = i;
                    else if (col is "peso" or "weight" or "pesokg") weightIndex = i;
                    else if (col is "data" or "date" or "datapesagem") dateIndex = i;
                }

                if (tagIndex == -1) tagIndex = 0;
                if (weightIndex == -1) weightIndex = parts.Length > 1 ? 1 : -1;
                continue;
            }

            if (tagIndex >= parts.Length || weightIndex < 0 || weightIndex >= parts.Length)
            {
                rows.Add(new ParsedWeighingRow(rowNumber, "", 0, null, null, "", false, "Linha mal formatada para Coimma."));
                continue;
            }

            string tag = parts[tagIndex].Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(tag))
            {
                rows.Add(new ParsedWeighingRow(rowNumber, "", 0, null, null, "", false, "Brinco ausente."));
                continue;
            }

            string weightStr = parts[weightIndex].Replace(",", ".");
            if (!decimal.TryParse(weightStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal weight) || weight <= 0)
            {
                rows.Add(new ParsedWeighingRow(rowNumber, tag, 0, null, null, "", false, $"Peso inválido ({parts[weightIndex]})."));
                continue;
            }

            DateTime? weighingDate = null;
            if (dateIndex >= 0 && dateIndex < parts.Length && !string.IsNullOrWhiteSpace(parts[dateIndex]))
            {
                if (DateTime.TryParseExact(parts[dateIndex], new[] { "dd/MM/yyyy", "yyyy-MM-dd", "dd-MM-yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt) ||
                    DateTime.TryParse(parts[dateIndex], CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                {
                    weighingDate = dt;
                }
            }

            rows.Add(new ParsedWeighingRow(rowNumber, tag, weight, weighingDate, null, "Importado via Coimma", true, null));
        }

        return Result.Success(rows);
    }
}
