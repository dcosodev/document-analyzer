using System;
using System.Globalization;
using ImageAnalysisAPI.Models;
using Microsoft.Extensions.Logging;

namespace ImageAnalysisAPI.Services
{
    /// <summary>
    /// Scores how plausibly a photo was taken at the reported place and time.
    /// See docs/SCORING.md for the derivation.
    /// </summary>
    public class LiveCheckService
    {
        private const double EarthRadiusKm = 6371;

        /// <summary>Maximum accepted gap between EXIF GPS and caller-reported GPS.</summary>
        private const double DeviceLocationToleranceKm = 10;

        /// <summary>
        /// Maximum accepted gap between EXIF GPS and the IP-derived location, which
        /// resolves only to a city or region.
        /// </summary>
        private const double IpLocationToleranceKm = 500;

        /// <summary>Maximum age of a capture still considered "live".</summary>
        private const double RecencyToleranceMinutes = 60;

        private readonly ILogger<LiveCheckService> _logger;

        public LiveCheckService(ILogger<LiveCheckService> logger)
        {
            _logger = logger;
        }

        public double CheckIfLive(string gpsDateTime, string gpsNow, string gpsLocMeta, GeoDetails geoDetails)
        {
            double locScore = 0;
            double dateScore = 0;
            double locFactors = 0;
            const double liveFactors = 2;

            if (!string.IsNullOrEmpty(gpsNow) && !string.IsNullOrEmpty(gpsLocMeta))
            {
                locFactors += 1;
                if (TryParseCoordinates(gpsNow, out var providedLat, out var providedLon) &&
                    TryParseCoordinates(gpsLocMeta, out var metadataLat, out var metadataLon))
                {
                    var distanceGps = CalculateDistance(providedLat, providedLon, metadataLat, metadataLon);
                    _logger.LogDebug("Distance between provided GPS and metadata GPS: {DistanceKm} km", distanceGps);
                    if (distanceGps <= DeviceLocationToleranceKm)
                    {
                        locScore += 1;
                    }
                }
            }

            if (!string.IsNullOrEmpty(gpsDateTime) && IsRecent(gpsDateTime))
            {
                dateScore += 1;
            }

            if (geoDetails != null && !string.IsNullOrEmpty(gpsLocMeta))
            {
                locFactors += 1;
                if (TryParseCoordinates(gpsLocMeta, out var metadataLat, out var metadataLon) &&
                    double.TryParse(geoDetails.Latitude, NumberStyles.Float, CultureInfo.InvariantCulture, out var ipLat) &&
                    double.TryParse(geoDetails.Longitude, NumberStyles.Float, CultureInfo.InvariantCulture, out var ipLon))
                {
                    var distanceIp = CalculateDistance(ipLat, ipLon, metadataLat, metadataLon);
                    _logger.LogDebug("Distance between IP location and metadata GPS: {DistanceKm} km", distanceIp);
                    if (distanceIp <= IpLocationToleranceKm)
                    {
                        locScore += 1;
                    }
                }
            }

            // No location comparison could run at all: the location half contributes
            // nothing rather than producing NaN through a division by zero.
            var locationComponent = locFactors > 0 ? locScore / locFactors : 0;

            var livePercentage = (locationComponent + dateScore) / liveFactors * 100;
            _logger.LogDebug(
                "Live score {LivePercentage}% (locScore {LocScore}/{LocFactors}, dateScore {DateScore})",
                livePercentage, locScore, locFactors, dateScore);

            return livePercentage;
        }

        private static bool TryParseCoordinates(string value, out double latitude, out double longitude)
        {
            latitude = 0;
            longitude = 0;

            var parts = value.Split(',');
            if (parts.Length != 2)
            {
                return false;
            }

            return double.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out latitude)
                && double.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out longitude);
        }

        internal static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var latDistance = DegreesToRadians(lat2 - lat1);
            var lonDistance = DegreesToRadians(lon2 - lon1);

            var a = Math.Sin(latDistance / 2) * Math.Sin(latDistance / 2)
                    + Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2))
                    * Math.Sin(lonDistance / 2) * Math.Sin(lonDistance / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EarthRadiusKm * c;
        }

        private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180;

        internal static bool IsRecent(string gpsDateTime)
        {
            if (!DateTime.TryParseExact(gpsDateTime, "yyyy:MM:dd HH:mm:ss",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var captured))
            {
                return false;
            }

            return (DateTime.UtcNow - captured).TotalMinutes <= RecencyToleranceMinutes;
        }
    }
}
