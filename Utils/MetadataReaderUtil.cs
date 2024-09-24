using System;
using System.IO;
using System.Linq;
using ImageAnalysisAPI.Models;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Directory = MetadataExtractor.Directory;

namespace ImageAnalysisAPI.Utils
{
    /// <summary>
    /// Reads the EXIF tags the scoring services rely on.
    /// </summary>
    /// <remarks>
    /// Backed by MetadataExtractor rather than <c>System.Drawing.Common</c>, which
    /// is Windows-only from .NET 7 onward. Unlike the previous implementation this
    /// honours the GPS hemisphere references, so coordinates south of the equator
    /// or west of Greenwich are correctly signed.
    /// </remarks>
    public static class MetadataReaderUtil
    {
        public static PhotoResponse ReadMetadata(FileInfo file)
        {
            var response = new PhotoResponse
            {
                PhotoName = file.Name
            };

            try
            {
                var directories = ImageMetadataReader.ReadMetadata(file.FullName);

                var ifd0 = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
                var subIfd = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
                var gps = directories.OfType<GpsDirectory>().FirstOrDefault();

                response.CameraMake = Describe(ifd0, ExifDirectoryBase.TagMake);
                response.CameraModel = Describe(ifd0, ExifDirectoryBase.TagModel);

                // 0x9003 DateTimeOriginal - when the shutter fired.
                response.DatetimeOriginal = Describe(subIfd, ExifDirectoryBase.TagDateTimeOriginal);

                // 0x9004 DateTimeDigitized - when the file was written; used for recency.
                response.GpsDateTime = Describe(subIfd, ExifDirectoryBase.TagDateTimeDigitized);

                // 0x0132 DateTime - last modification, in IFD0.
                response.DatetimeModified = Describe(ifd0, ExifDirectoryBase.TagDateTime);

                // 0x0131 Software is the tag editors actually write. The previous
                // implementation read UserComment (0x9286), which is kept as a
                // fallback because some tools stamp their name there instead.
                var software = Describe(ifd0, ExifDirectoryBase.TagSoftware);
                response.EditingSoftware = string.IsNullOrEmpty(software)
                    ? Describe(subIfd, ExifDirectoryBase.TagUserComment)
                    : software;

                var location = gps?.GetGeoLocation();
                if (location != null && !location.IsZero)
                {
                    response.GpsLocMeta = FormatCoordinates(location.Latitude, location.Longitude);
                }
            }
            catch (ImageProcessingException ex)
            {
                // Unsupported or malformed container, e.g. a PDF submitted as an image.
                response.ErrorMessage = ex.Message;
            }
            catch (IOException ex)
            {
                response.ErrorMessage = ex.Message;
            }

            return response;
        }

        private static string Describe(Directory? directory, int tagType)
        {
            var value = directory?.GetDescription(tagType);
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        internal static string FormatCoordinates(double latitude, double longitude)
        {
            return FormattableString.Invariant($"{latitude},{longitude}");
        }
    }
}
