using ImageAnalysisAPI.Services;
using Xunit;

namespace ImageAnalysisAPI.Tests;

public class GenuineCheckServiceTests
{
    private readonly GenuineCheckService _service = new();

    [Theory]
    [InlineData("Apple", "iPhone 14 Pro", 100)]
    [InlineData("Canon", "EOS R6", 100)]
    public void BothCameraTagsPresent_ScoresFull(string make, string model, double expected)
    {
        Assert.Equal(expected, _service.CheckIfGenuine(make, model));
    }

    [Theory]
    [InlineData("Apple", "")]
    [InlineData("", "iPhone 14 Pro")]
    [InlineData("Apple", null)]
    [InlineData(null, "iPhone 14 Pro")]
    public void OneCameraTagMissing_ScoresHalf(string? make, string? model)
    {
        Assert.Equal(50, _service.CheckIfGenuine(make!, model!));
    }

    [Theory]
    [InlineData("", "")]
    [InlineData(null, null)]
    public void NoCameraTags_ScoresZero(string? make, string? model)
    {
        Assert.Equal(0, _service.CheckIfGenuine(make!, model!));
    }
}
