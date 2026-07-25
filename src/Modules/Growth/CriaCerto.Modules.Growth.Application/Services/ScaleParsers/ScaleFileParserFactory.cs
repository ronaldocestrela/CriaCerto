using CriaCerto.Modules.Growth.Application.Contracts;

namespace CriaCerto.Modules.Growth.Application.Services.ScaleParsers;

public interface IScaleFileParserFactory
{
    IWeighingScaleFileParser GetParser(ScaleModelEnum model);
}

public sealed class ScaleFileParserFactory : IScaleFileParserFactory
{
    private readonly IEnumerable<IWeighingScaleFileParser> _parsers;

    public ScaleFileParserFactory(IEnumerable<IWeighingScaleFileParser> parsers)
    {
        _parsers = parsers;
    }

    public IWeighingScaleFileParser GetParser(ScaleModelEnum model)
    {
        if (model == ScaleModelEnum.AutoDetect)
        {
            return _parsers.FirstOrDefault(p => p.ScaleModel == ScaleModelEnum.GenericCsv) ?? new GenericCsvScaleParser();
        }

        var parser = _parsers.FirstOrDefault(p => p.ScaleModel == model);
        return parser ?? new GenericCsvScaleParser();
    }
}
