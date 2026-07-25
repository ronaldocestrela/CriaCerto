using System.Text;
using CriaCerto.Modules.Growth.Application.Contracts;
using CriaCerto.Modules.Growth.Application.Services.ScaleParsers;
using FluentAssertions;
using Xunit;

namespace CriaCerto.Modules.Growth.UnitTests.Services;

public class ScaleParsersUnitTests
{
    [Fact]
    public void TruTestScaleParser_ValidCsv_ShouldParseRowsCorrectly()
    {
        // Arrange
        var csvContent = "VID,Weight,Date\nBR-101,450.5,2026-07-20\nBR-102,420.0,2026-07-20";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
        var parser = new TruTestScaleParser();

        // Act
        var result = parser.Parse(stream, "tru_test_export.csv");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].AnimalTagId.Should().Be("BR-101");
        result.Value[0].WeightKg.Should().Be(450.5m);
        result.Value[0].IsValid.Should().BeTrue();
        result.Value[1].AnimalTagId.Should().Be("BR-102");
        result.Value[1].WeightKg.Should().Be(420.0m);
    }

    [Fact]
    public void CoimmaScaleParser_SemicolonDelimited_ShouldParseRowsCorrectly()
    {
        // Arrange
        var csvContent = "Brinco;Peso;Data\nBR-201;380.0;20/07/2026\nBR-202;510.5;20/07/2026";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
        var parser = new CoimmaScaleParser();

        // Act
        var result = parser.Parse(stream, "coimma_pesagem.txt");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].AnimalTagId.Should().Be("BR-201");
        result.Value[0].WeightKg.Should().Be(380.0m);
        result.Value[1].AnimalTagId.Should().Be("BR-202");
        result.Value[1].WeightKg.Should().Be(510.5m);
    }

    [Fact]
    public void ToledoScaleParser_ValidCsv_ShouldParseRowsCorrectly()
    {
        // Arrange
        var csvContent = "TAG,PESO,DATA_PESAGEM\nBR-301,490.0,2026-07-20\nBR-302,505.0,2026-07-20";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
        var parser = new ToledoScaleParser();

        // Act
        var result = parser.Parse(stream, "toledo_pesagem.csv");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].AnimalTagId.Should().Be("BR-301");
        result.Value[0].WeightKg.Should().Be(490.0m);
    }

    [Fact]
    public void GenericCsvScaleParser_AutoDetectHeader_ShouldParseRowsCorrectly()
    {
        // Arrange
        var csvContent = "Tag;PesoKg;Rendimento\nBR-401;400.0;52.0\nBR-402;430.0;54.0";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
        var parser = new GenericCsvScaleParser();

        // Act
        var result = parser.Parse(stream, "generic_export.csv");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].AnimalTagId.Should().Be("BR-401");
        result.Value[0].WeightKg.Should().Be(400.0m);
        result.Value[0].CarcassYieldPercentage.Should().Be(52.0m);
    }

    [Fact]
    public void ScaleFileParserFactory_AutoDetect_ShouldReturnAppropriateParser()
    {
        // Arrange
        var factory = new ScaleFileParserFactory(new IWeighingScaleFileParser[]
        {
            new TruTestScaleParser(),
            new CoimmaScaleParser(),
            new ToledoScaleParser(),
            new GenericCsvScaleParser()
        });

        // Act
        var truTestParser = factory.GetParser(ScaleModelEnum.TruTest);
        var genericParser = factory.GetParser(ScaleModelEnum.GenericCsv);

        // Assert
        truTestParser.ScaleModel.Should().Be(ScaleModelEnum.TruTest);
        genericParser.ScaleModel.Should().Be(ScaleModelEnum.GenericCsv);
    }
}
