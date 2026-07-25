using System.Globalization;
using System.Text;
using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.Modules.Growth.Application.Contracts;

namespace CriaCerto.Modules.Growth.Application.Services.ScaleParsers;

public sealed class GenericCsvScaleParser : IWeighingScaleFileParser
{
    public ScaleModelEnum ScaleModel => ScaleModelEnum.GenericCsv;

    public Result<List<ParsedWeighingRow>> Parse(Stream fileStream, string fileName)
    {
        var rows = new List<ParsedWeighingRow>();
        using var reader = new StreamReader(fileStream, Encoding.UTF8, true, leaveOpen: true);

        string? line;
        int rowNumber = 0;
        int tagIndex = -1, weightIndex = -1, dateIndex = -1, yieldIndex = -1, notesIndex = -1;

        while ((line = reader.ReadLine()) != null)
        {
            rowNumber++;
            if (string.IsNullOrWhiteSpace(line)) continue;

            char delimiter = line.Contains(';') ? ';' : (line.Contains('\t') ? '\t' : ',');
            var parts = line.Split(delimiter, StringSplitOptions.TrimEntries);

            if (rowNumber == 1)
            {
                for (int i = 0; i < parts.Length; i++)
                {
                    string col = parts[i].Trim().ToLowerInvariant();
                    if (col is "brinco" or "tag" or "vid" or "rfid" or "id" or "animaltagid" or "animal") tagIndex = i;
                    else if (col is "peso" or "weight" or "pesokg" or "peso (kg)") weightIndex = i;
                    else if (col is "data" or "date" or "datapesagem" or "weighingdate") dateIndex = i;
                    else if (col is "rendimento" or "yield" or "carcassyield" or "rendimento%") yieldIndex = i;
                    else if (col is "obs" or "observacao" or "notes" or "observacoes") notesIndex = i;
                }

                if (tagIndex == -1) tagIndex = 0;
                if (weightIndex == -1) weightIndex = parts.Length > 1 ? 1 : -1;
                continue;
            }

            if (tagIndex >= parts.Length || weightIndex < 0 || weightIndex >= parts.Length)
            {
                rows.Add(new ParsedWeighingRow(rowNumber, "", 0, null, null, "", false, "Formato de linha inválido."));
                continue;
            }

            string tag = parts[tagIndex].Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(tag))
            {
                rows.Add(new ParsedWeighingRow(rowNumber, "", 0, null, null, "", false, "Tag / Brinco ausente."));
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
                if (DateTime.TryParse(parts[dateIndex], CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt) ||
                    DateTime.TryParseExact(parts[dateIndex], new[] { "dd/MM/yyyy", "yyyy-MM-dd", "dd-MM-yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                {
                    weighingDate = dt;
                }
            }

            decimal? yield = null;
            if (yieldIndex >= 0 && yieldIndex < parts.Length && !string.IsNullOrWhiteSpace(parts[yieldIndex]))
            {
                string yieldStr = parts[yieldIndex].Replace("%", "").Replace(",", ".").Trim();
                if (decimal.TryParse(yieldStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsedYield))
                {
                    yield = parsedYield;
                }
            }

            string notes = notesIndex >= 0 && notesIndex < parts.Length ? parts[notesIndex] : "Importação CSV Genérico";

            rows.Add(new ParsedWeighingRow(rowNumber, tag, weight, weighingDate, yield, notes, true, null));
        }

        return Result.Success(rows);
    }
}
