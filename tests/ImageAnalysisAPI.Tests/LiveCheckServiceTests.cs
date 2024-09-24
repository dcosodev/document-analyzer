using System.Globalization;
using ImageAnalysisAPI.Models;
using ImageAnalysisAPI.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ImageAnalysisAPI.Tests;

public class LiveCheckServiceTests
{
    private readonly LiveCheckService _service = new(NullLogger<LiveCheckService>.Instance);

    private const string ManhattanMeta = "40.712801,-74.006012";
    private const string ManhattanDevice = "40.712776,-74.005974";
    private const string ParisDevice = "48.856614,2.352222";

    private static string Recent() =>
        DateTime.UtcNow.AddMinutes(-5).ToString("yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);

    private static string Stale() =>
        DateTime.UtcNow.AddHours(-9).ToString("yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);

    private static GeoDetails Manhattan() =>
        new("203.0.113.7", "US", "New York, New York, United States", "40.7128", "-74.0060");

    [Fact]
    public void NoLocationSignalsAtAll_ScoresZeroInsteadOfNaN()
    {
        // Regression: locFactors used to stay at 0 and the score divided by it,
        // returning NaN and serialising as null.
        var score = _service.CheckIfLive(
            gpsDateTime: string.Empty,
            gpsNow: string.Empty,
            gpsLocMeta: string.Empty,
            geoDetails: null!);

        Assert.False(double.IsNaN(score));
        Assert.Equal(0, score);
    }

    [Fact]
    public void RecentCaptureWithNoLocationSignals_ScoresRecencyHalfOnly()
    {
        var score = _service.CheckIfLive(Recent(), string.Empty, string.Empty, null!);

        Assert.False(double.IsNaN(score));
        Assert.Equal(50, score);
    }

    [Fact]
    public void MatchingDeviceLocationAndRecentCapture_ScoresFull()
    {
        var score = _service.CheckIfLive(Recent(), ManhattanDevice, ManhattanMeta, null!);

        Assert.Equal(100, score);
    }

    [Fact]
    public void DeviceLocationFarFromMetadata_LosesLocationComponent()
    {
        var score = _service.CheckIfLive(Recent(), ParisDevice, ManhattanMeta, null!);

        Assert.Equal(50, score);
    }

    [Fact]
    public void StaleCaptureWithMatchingLocation_LosesRecencyComponent()
    {
        var score = _service.CheckIfLive(Stale(), ManhattanDevice, ManhattanMeta, null!);

        Assert.Equal(50, score);
    }

    [Fact]
    public void BothLocationComparisonsAgreeAndCaptureIsRecent_ScoresFull()
    {
        var score = _service.CheckIfLive(Recent(), ManhattanDevice, ManhattanMeta, Manhattan());

        Assert.Equal(100, score);
    }

    [Fact]
    public void OneOfTwoLocationComparisonsFails_ScoresThreeQuarters()
    {
        // IP says Manhattan and the metadata agrees, but the device reported Paris.
        var score = _service.CheckIfLive(Recent(), ParisDevice, ManhattanMeta, Manhattan());

        Assert.Equal(75, score);
    }

    [Fact]
    public void MalformedCoordinates_AreTreatedAsAFailedComparison()
    {
        var score = _service.CheckIfLive(Recent(), "not-a-coordinate", ManhattanMeta, null!);

        Assert.False(double.IsNaN(score));
        Assert.Equal(50, score);
    }

    [Theory]
    [InlineData("40.7128")]           // missing longitude
    [InlineData("40.7128,-74,extra")] // too many components
    public void CoordinatesWithWrongShape_DoNotThrow(string malformed)
    {
        var score = _service.CheckIfLive(Recent(), malformed, ManhattanMeta, null!);

        Assert.False(double.IsNaN(score));
    }

    [Fact]
    public void HaversineDistance_MatchesKnownSeparation()
    {
        // One degree of longitude at the equator is ~111.19 km.
        var distance = LiveCheckService.CalculateDistance(0, 0, 0, 1);

        Assert.Equal(111.19, distance, precision: 1);
    }

    [Fact]
    public void HaversineDistance_IsZeroForIdenticalPoints()
    {
        Assert.Equal(0, LiveCheckService.CalculateDistance(40.7128, -74.0060, 40.7128, -74.0060));
    }

    [Theory]
    [InlineData("")]
    [InlineData("13/09/2024 10:22:31")]
    [InlineData("not a date")]
    public void UnparseableTimestamps_AreNotRecent(string value)
    {
        Assert.False(LiveCheckService.IsRecent(value));
    }
}
