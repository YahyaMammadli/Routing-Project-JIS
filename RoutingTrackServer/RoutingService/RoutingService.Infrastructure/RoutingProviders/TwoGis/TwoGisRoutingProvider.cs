using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RoutingService.Application.Interfaces;
using RoutingService.Application.Models;
using RoutingService.Domain.Enums;

namespace RoutingService.Infrastructure.RoutingProviders.TwoGis;

public class TwoGisRoutingProvider : IRoutingProvider
{
    private const string GeocodeUrl = "https://catalog.api.2gis.com/3.0/items/geocode";
    private const string RoutesUrl = "https://routing.api.2gis.com/routing/7.0.0/global";

    // 2GIS interprets this as the number of additional routes.
    // 1 additional route = up to 2 routes in the response.
    private const int AlternativeRoutes = 1;

    private readonly HttpClient _httpClient;
    private readonly TwoGisOptions _options;
    private readonly ILogger<TwoGisRoutingProvider> _logger;

    public string Name => "2gis";

    public TwoGisRoutingProvider(
        HttpClient httpClient,
        IOptions<TwoGisOptions> options,
        ILogger<TwoGisRoutingProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new InvalidOperationException("2GIS API key is not configured.");
    }

    public async Task<ProviderRouteResult> CalculateAsync(
        RouteCalculationRequest request,
        TransportMode transportMode,
        CancellationToken cancellationToken = default)
    {
        var transport = transportMode switch
        {
            TransportMode.Car => "driving",
            TransportMode.Bicycle => "bicycle",
            TransportMode.Scooter => "scooter",
            _ => throw new ArgumentOutOfRangeException(nameof(transportMode), transportMode, null)
        };

        var fromPoints = await GeocodeAsync(request.From, cancellationToken);
        var toPoints = await GeocodeAsync(request.To, cancellationToken);

        if (fromPoints.Count == 0)
            return ErrorResult(transportMode, $"2GIS could not geocode address: '{request.From}'");

        if (toPoints.Count == 0)
            return ErrorResult(transportMode, $"2GIS could not geocode address: '{request.To}'");

        _logger.LogInformation(
            "2GIS routing: {FromCandidates} From candidates, {ToCandidates} To candidates.",
            fromPoints.Count,
            toPoints.Count);

        // The first geocoder result can be non-routable. Try the returned
        // candidates until one coordinate pair produces a route.
        foreach (var fromPoint in fromPoints)
        {
            foreach (var toPoint in toPoints)
            {
                var routes = await TryRouteAsync(
                    fromPoint,
                    toPoint,
                    transport,
                    transportMode,
                    cancellationToken);

                if (routes.Count == 0)
                    continue;

                var primary = routes[0];

                return new ProviderRouteResult
                {
                    Provider = Name,
                    TransportMode = TransportModeToString(transportMode),
                    DistanceKm = primary.DistanceKm,
                    DurationMinutes = primary.DurationMinutes,
                    TrafficAvailable = transportMode == TransportMode.Car,
                    TrafficUsed = transportMode == TransportMode.Car,
                    Routes = routes
                };
            }
        }

        return ErrorResult(
            transportMode,
            "2GIS could not build a route for any geocoded coordinate combination.");
    }

    private async Task<List<GeoPoint>> GeocodeAsync(
        string address,
        CancellationToken cancellationToken)
    {
        var url =
            $"{GeocodeUrl}" +
            $"?key={Uri.EscapeDataString(_options.ApiKey)}" +
            $"&q={Uri.EscapeDataString(address)}" +
            "&locale=az_AZ" +
            "&fields=items.point,items.type,items.is_routing_available";

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(
                $"2GIS Geocoder returned {(int)response.StatusCode}: {responseContent}");

        using var document = JsonDocument.Parse(responseContent);

        if (!document.RootElement.TryGetProperty("result", out var result) ||
            !result.TryGetProperty("items", out var items) ||
            items.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var points = new List<GeoPoint>();

        foreach (var item in items.EnumerateArray())
        {
            if (!item.TryGetProperty("point", out var point))
                continue;

            if (!point.TryGetProperty("lat", out var latElement) ||
                !point.TryGetProperty("lon", out var lonElement))
                continue;

            var lat = latElement.GetDouble();
            var lon = lonElement.GetDouble();

            bool? routingAvailable = null;
            if (item.TryGetProperty("is_routing_available", out var routingElement) &&
                (routingElement.ValueKind == JsonValueKind.True ||
                 routingElement.ValueKind == JsonValueKind.False))
            {
                routingAvailable = routingElement.GetBoolean();
            }

            points.Add(new GeoPoint(lat, lon, routingAvailable));
        }

        // Prefer candidates explicitly marked as routable, but keep all
        // candidates as fallback because the field is not guaranteed for every item.
        return points
            .DistinctBy(x => (x.Lat, x.Lon))
            .OrderByDescending(x => x.IsRoutingAvailable == true)
            .Take(5)
            .ToList();
    }

    private async Task<List<RouteOption>> TryRouteAsync(
        GeoPoint fromPoint,
        GeoPoint toPoint,
        string transport,
        TransportMode transportMode,
        CancellationToken cancellationToken)
    {
        var requestBody = new Dictionary<string, object>
        {
            ["points"] = new object[]
            {
                new
                {
                    type = "stop",
                    lon = fromPoint.Lon,
                    lat = fromPoint.Lat,
                    start = true
                },
                new
                {
                    type = "stop",
                    lon = toPoint.Lon,
                    lat = toPoint.Lat
                }
            },
            ["transport"] = transport,
            ["route_mode"] = "fastest",
            ["output"] = "summary",
            ["locale"] = "en",
            ["alternative"] = AlternativeRoutes
        };

        if (transportMode == TransportMode.Car)
            requestBody["traffic_mode"] = "jam";

        var url = $"{RoutesUrl}?key={Uri.EscapeDataString(_options.ApiKey)}";

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json")
        };

        _logger.LogInformation(
            "2GIS routing request: {Body}",
            JsonSerializer.Serialize(requestBody));

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        _logger.LogInformation(
            "2GIS routing response HTTP {StatusCode}: {Body}",
            (int)response.StatusCode,
            responseContent);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(
                $"2GIS Routing API returned {(int)response.StatusCode}: {responseContent}");

        using var document = JsonDocument.Parse(responseContent);

        var status = document.RootElement.TryGetProperty("status", out var statusElement)
            ? statusElement.GetString()
            : null;

        if (!string.Equals(status, "OK", StringComparison.OrdinalIgnoreCase))
        {
            // This pair can fail while another geocoder candidate works.
            return [];
        }

        if (!document.RootElement.TryGetProperty("result", out var result) ||
            result.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var routes = new List<RouteOption>();
        var routeNumber = 1;

        foreach (var route in result.EnumerateArray())
        {
            if (!route.TryGetProperty("total_distance", out var distanceElement) ||
                !route.TryGetProperty("total_duration", out var durationElement))
                continue;

            var distanceMeters = distanceElement.GetDouble();
            var durationSeconds = durationElement.GetDouble();

            if (distanceMeters <= 0 || durationSeconds <= 0)
                continue;

            string? routeId = null;
            if (route.TryGetProperty("route_id", out var routeIdElement))
                routeId = routeIdElement.GetString();

            string? algorithm = null;
            if (route.TryGetProperty("algorithm", out var algorithmElement))
                algorithm = algorithmElement.GetString();

            routes.Add(new RouteOption
            {
                RouteNumber = routeNumber++,
                RouteId = routeId,
                DistanceKm = Math.Round(distanceMeters / 1000.0, 3),
                DurationMinutes = Math.Round(durationSeconds / 60.0, 2),
                Algorithm = algorithm
            });
        }

        return routes;
    }

    private static ProviderRouteResult ErrorResult(
        TransportMode mode,
        string error)
    {
        return new ProviderRouteResult
        {
            Provider = "2gis",
            TransportMode = TransportModeToString(mode),
            TrafficAvailable = false,
            TrafficUsed = false,
            Routes = [],
            Error = error
        };
    }

    private static string TransportModeToString(TransportMode mode) => mode switch
    {
        TransportMode.Car => "car",
        TransportMode.Bicycle => "bicycle",
        TransportMode.Scooter => "scooter",
        _ => mode.ToString().ToLowerInvariant()
    };

    private sealed record GeoPoint(
        double Lat,
        double Lon,
        bool? IsRoutingAvailable);
}
