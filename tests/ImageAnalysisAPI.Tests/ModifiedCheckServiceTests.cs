using ImageAnalysisAPI.Services;
using Xunit;

namespace ImageAnalysisAPI.Tests;

/// <summary>
/// The Modified score counts the ABSENCE of tampering signals: 100 means clean.
/// </summary>
public class ModifiedCheckServiceTests
{
    private readonly ModifiedCheckService _service = new();

    [Fact]
    public void NoEditingSoftwareAndMatchingTimestamps_ScoresClean()
    {
        var score = _service.CheckIfModified(
            editingSoftware: "",
            dateTimeOriginal: "2024:09:13 10:22:31",
            dateTimeModified: "2024:09:13 10:22:31");

        Assert.Equal(100, score);
    }

    [Fact]
    public void EditingSoftwareAndDivergentTimestamps_ScoresZero()
    {
        var score = _service.CheckIfModified(
            editingSoftware: "Adobe Photoshop 25.0",
            dateTimeOriginal: "2024:09:13 10:22:31",
            dateTimeModified: "2024:09:14 08:00:00");

        Assert.Equal(0, score);
    }

    [Fact]
    public void EditingSoftwareOnly_ScoresHalf()
    {
        var score = _service.CheckIfModified(
            editingSoftware: "GIMP 2.10",
            dateTimeOriginal: "2024:09:13 10:22:31",
            dateTimeModified: "2024:09:13 10:22:31");

        Assert.Equal(50, score);
    }

    [Fact]
    public void DivergentTimestampsOnly_ScoresHalf()
    {
        var score = _service.CheckIfModified(
            editingSoftware: "",
            dateTimeOriginal: "2024:09:13 10:22:31",
            dateTimeModified: "2024:09:14 08:00:00");

        Assert.Equal(50, score);
    }

    [Fact]
    public void MissingOriginalTimestamp_CountsAsUnmodified()
    {
        // An absent original timestamp cannot evidence tampering, so the factor
        // is awarded rather than penalised.
        var score = _service.CheckIfModified(
            editingSoftware: "",
            dateTimeOriginal: "",
            dateTimeModified: "2024:09:14 08:00:00");

        Assert.Equal(100, score);
    }
}
