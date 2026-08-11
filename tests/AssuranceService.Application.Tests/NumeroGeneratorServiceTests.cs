using AssuranceService.Application.Common;
using AssuranceService.Application.Services;
using AssuranceService.Domain.Models;
using Moq;
using Xunit;

namespace AssuranceService.Application.Tests;

public class NumeroGeneratorServiceTests
{
    [Fact]
    public async Task GenerateNoPolice_UsesLegacyFormatAtSignature()
    {
        var repository = new Mock<IAssuranceRepository>();
        repository
            .Setup(item => item.GetAllAsync())
            .ReturnsAsync(new[]
            {
                new Assurance { NoPolice = "AMC100017250723" }
            });
        var service = new NumeroGeneratorService(repository.Object);
        var dateBefore = DateTime.Now.ToString("ddMMyy");

        var result = await service.GenerateNoPoliceLAsync("aac assurances");

        var dateAfter = DateTime.Now.ToString("ddMMyy");
        Assert.StartsWith("AAC100018", result);
        Assert.Equal(15, result.Length);
        Assert.Contains(result[9..], new[] { dateBefore, dateAfter });
    }
}
