using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.Modules.Analytics.Application.Contracts;
using CriaCerto.Modules.Analytics.Application.Services;
using FluentAssertions;
using Xunit;

namespace CriaCerto.Architecture.IntegrationTests;

public class ExportBovineReportIntegrationTests
{
    [Fact]
    public async Task ExportBovineReportQueryHandler_WhenCalledForGtaSupport_ShouldReturnValidResult()
    {
        // Arrange
        var scorecard = new ExecutiveScorecardDto(85m, 80m, 1.8m, 0.95m, 175m, 0, "Excelente");
        var query = new ExportBovineReportQuery(
            Scorecard: scorecard,
            ReportType: ReportTypeEnum.GtaSupport,
            Format: ReportFormatEnum.Pdf,
            PeriodType: PeriodTypeEnum.CurrentHarvest);

        var handler = new ExportBovineReportQueryHandler();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.FileName.Should().StartWith("criacerto_suporte_gta_");
        result.Value.ContentType.Should().Be("application/pdf");
        result.Value.FileContents.Should().NotBeEmpty();
    }
}
