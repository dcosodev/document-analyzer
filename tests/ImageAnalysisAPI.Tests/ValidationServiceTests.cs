using ImageAnalysisAPI.Services;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace ImageAnalysisAPI.Tests;

public class ValidationServiceTests
{
    [Fact]
    public void Rejects_a_file_larger_than_the_configured_limit()
    {
        var service = new ValidationService();
        var file = new FormFile(new MemoryStream(new byte[ValidationService.MaxFileSizeBytes + 1]), 0,
            ValidationService.MaxFileSizeBytes + 1, "images", "large.jpg");

        Assert.False(service.ValidateInput(Array.Empty<string>(), new[] { file }));
    }

    [Fact]
    public void Rejects_a_request_with_too_many_files()
    {
        var service = new ValidationService();
        var files = Enumerable.Range(0, ValidationService.MaxFileCount + 1)
            .Select(i => new FormFile(new MemoryStream(new byte[] { 1 }), 0, 1, "images", $"{i}.jpg"))
            .Cast<IFormFile>()
            .ToArray();

        Assert.False(service.ValidateInput(Array.Empty<string>(), files));
    }
}