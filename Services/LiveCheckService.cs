using System;
using System.Globalization;
using ImageAnalysisAPI.Models;

namespace ImageAnalysisAPI.Services
{
    public class LiveCheckService
    {
        public double CheckIfLive(string gpsDateTime, string gpsNow, string gpsLocMeta, GeoDetails geoDetails)
        {
            double locScore = 0;
            double dateScore = 0;
            double locFactors = 0;
            double liveFactors = 2;

            Console.WriteLine($"gpsDateTime: {gpsDateTime}");
            Console.WriteLine($"gpsNow: {gpsNow}");
            Console.WriteLine($"gpsLocMeta: {gpsLocMeta}");
            Console.WriteLine($"geoDetails: {geoDetails.LocIP}, {geoDetails.Latitude}, {geoDetails.Longitude}");

            if (string.IsNullOrEmpty(gpsDateTime) || string.IsNullOrEmpty(gpsNow) || string.IsNullOrEmpty(gpsLocMeta))
            {
                dateScore += 0;
                locScore += 0;
            }

            if (!string.IsNullOrEmpty(gpsNow) && !string.IsNullOrEmpty(gpsLocMeta))
            {
                locFactors += 1;
                var gpsCoords = gpsNow.Split(',');
                try
                {
                    double providedLat = double.Parse(gpsCoords[0].Trim(), CultureInfo.InvariantCulture);
                    double providedLon = double.Parse(gpsCoords[1].Trim(), CultureInfo.InvariantCulture);
                    var metadataCoords = gpsLocMeta.Split(',');
                    double metadataLat = double.Parse(metadataCoords[0].Trim(), CultureInfo.InvariantCulture);
                    double metadataLon = double.Parse(metadataCoords[1].Trim(), CultureInfo.InvariantCulture);

                    double distanceGPS = CalculateDistance(providedLat, providedLon, metadataLat, metadataLon);
                    Console.WriteLine($"Distance between provided GPS and metadata GPS: {distanceGPS} km");
                    if (distanceGPS <= 10)
                    {
                        locScore += 1;
                    }
                }
                catch (FormatException)
                {
                    locScore += 0;
                }
            }

            if (!string.IsNullOrEmpty(gpsDateTime))
            {
                bool timeDifference = CalculateDateDifference(gpsDateTime);
                Console.WriteLine($"CalculateDateDifference: {timeDifference}");
                if (timeDifference)
                {
                    dateScore += 1;
                }
            }

            if (geoDetails != null && !string.IsNullOrEmpty(gpsLocMeta))
            {
                locFactors += 1;
                try
                {
                    var metadataCoords = gpsLocMeta.Split(',');
                    double metadataLat = double.Parse(metadataCoords[0].Trim(), CultureInfo.InvariantCulture);
                    double metadataLon = double.Parse(metadataCoords[1].Trim(), CultureInfo.InvariantCulture);

                    double distanceIP = CalculateDistance(double.Parse(geoDetails.Latitude, CultureInfo.InvariantCulture), double.Parse(geoDetails.Longitude, CultureInfo.InvariantCulture), metadataLat, metadataLon);
                    Console.WriteLine($"Distance between IP location and metadata GPS: {distanceIP} km");
                    if (distanceIP <= 500)
                    {
                        locScore += 1;
                    }
                }
                catch (FormatException)
                {
                    locScore += 0;
                }
            }

            double livePercentage = ((((locScore / locFactors) + dateScore) / liveFactors) * 100);
            Console.WriteLine($"locScore: {locScore}, locFactors: {locFactors}, dateScore: {dateScore}");
            Console.WriteLine($"Live score: {livePercentage}%");
            return livePercentage;
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const int R = 6371;

            double latDistance = DegreesToRadians(lat2 - lat1);
            double lonDistance = DegreesToRadians(lon2 - lon1);

            double a = Math.Sin(latDistance / 2) * Math.Sin(latDistance / 2)
                    + Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2))
                    * Math.Sin(lonDistance / 2) * Math.Sin(lonDistance / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }

        private double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        private bool CalculateDateDifference(string gpsDateTime)
        {
            try
            {
                DateTime dateTime = DateTime.ParseExact(gpsDateTime, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None);
                return (DateTime.UtcNow - dateTime).TotalMinutes <= 60;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
