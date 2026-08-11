namespace OfflineMaps.Win.Core;

public sealed record RouteStop(string Query, SearchResult Location);
public sealed record RouteRequest(RouteStop Start, IReadOnlyList<RouteStop> Stops, RouteStop Destination);
public sealed record RoutePlan(RouteRequest Request, double DistanceKm, string Status);

public sealed class OfflineRoutePlanner
{
    private readonly OfflineSearchService _search;
    public OfflineRoutePlanner(OfflineSearchService search) => _search = search;

    public async Task<RoutePlan?> PlanAsync(string start, IEnumerable<string> stops, string destination, CancellationToken cancellationToken = default)
    {
        var startLocation = await ResolveAsync(start, cancellationToken);
        var destinationLocation = await ResolveAsync(destination, cancellationToken);
        if (startLocation is null || destinationLocation is null) return null;
        var resolvedStops = new List<RouteStop>();
        foreach (var stop in stops)
        {
            var location = await ResolveAsync(stop, cancellationToken);
            if (location is not null) resolvedStops.Add(new(stop, location));
        }
        var points = new[] { new RouteStop(start, startLocation) }.Concat(resolvedStops).Append(new RouteStop(destination, destinationLocation)).ToArray();
        var km = points.Zip(points.Skip(1)).Sum(pair => DistanceKm(pair.First.Location, pair.Second.Location));
        return new(new RouteRequest(points[0], resolvedStops, points[^1]), km, "Locations resolved. Road graph routing will replace this distance preview.");
    }

    private async Task<SearchResult?> ResolveAsync(string query, CancellationToken cancellationToken)
        => (await _search.SearchAsync(query, cancellationToken)).FirstOrDefault();

    private static double DistanceKm(SearchResult a, SearchResult b)
    {
        const double earth = 6371;
        double r = Math.PI / 180, dLat = (b.Latitude - a.Latitude) * r, dLon = (b.Longitude - a.Longitude) * r;
        double h = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(a.Latitude * r) * Math.Cos(b.Latitude * r) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return earth * 2 * Math.Asin(Math.Sqrt(h));
    }
}
