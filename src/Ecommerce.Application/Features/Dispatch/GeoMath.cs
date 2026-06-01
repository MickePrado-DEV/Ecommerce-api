namespace Ecommerce.Application.Features.Dispatch;

public static class GeoMath
{
    private const double EarthRadiusKm = 6371.0;

    public static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
            + Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2))
            * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusKm * c;
    }

    public static (double Lat, double Lng) Centroid(IReadOnlyList<(double Lat, double Lng)> points)
    {
        if (points.Count == 0) return (0, 0);
        return (points.Average(p => p.Lat), points.Average(p => p.Lng));
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;
}
