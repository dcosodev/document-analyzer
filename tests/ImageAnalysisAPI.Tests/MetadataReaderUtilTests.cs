using System.Globalization;
using ImageAnalysisAPI.Utils;
using Xunit;

namespace ImageAnalysisAPI.Tests;

public class MetadataReaderUtilTests
{
    [Fact]
    public void Coordinates_AreFormattedWithInvariantCulture()
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            // A culture that uses the comma as its decimal separator would
            // otherwise produce "40,7128,-74,0060", which cannot be parsed back.
            CultureInfo.CurrentCulture = new CultureInfo("es-ES");

            var formatted = MetadataReaderUtil.FormatCoordinates(40.7128, -74.006);

            Assert.Equal("40.7128,-74.006", formatted);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void SouthernAndWesternHemispheres_KeepTheirSign()
    {
        // Sydney is south of the equator; the previous System.Drawing reader
        // ignored the GPS hemisphere reference tags and always returned positives.
        var formatted = MetadataReaderUtil.FormatCoordinates(-33.8688, 151.2093);

        Assert.Equal("-33.8688,151.2093", formatted);
    }

    [Fact]
    public void UnreadableFile_ReturnsErrorMessageRatherThanThrowing()
    {
        var path = Path.Combine(Path.GetTempPath(), $"not-an-image-{Guid.NewGuid():N}.jpg");
        File.WriteAllText(path, "this is plain text, not an image");

        try
        {
            var response = MetadataReaderUtil.ReadMetadata(new FileInfo(path));

            Assert.False(string.IsNullOrEmpty(response.ErrorMessage));
            Assert.Equal(Path.GetFileName(path), response.PhotoName);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
