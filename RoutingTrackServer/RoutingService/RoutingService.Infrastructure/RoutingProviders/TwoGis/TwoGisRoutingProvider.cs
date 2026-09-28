using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RoutingService.Application.Interfaces;
using RoutingService.Application.Models;
using RoutingService.Domain.Enums;

namespace RoutingService.Infrastructure.RoutingProviders.TwoGis;

public class TwoGisRoutingProvider : IRoutingProvider
{
    private const string GeocodeUrl = "https://catalog.api.2gis.com/3.0/items/geocode";
    private const string RoutesUrl = "https://routing.api.2gis.com/routing/7.0.0/global";

    private readonly HttpClient _httpClient;
    private readonly TwoGisOptions _options;

    public string Name => "2gis";

    public TwoGisRoutingProvider(HttpClient httpClient, IOptions<TwoGisOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new InvalidOperationException("2GIS API key is not configured.");
    }

    public async Task<ProviderRouteResult> CalculateAsync(RouteCalculationRequest request, TransportMode transportMode, CancellationToken cancellationToken = default)
    {
        var fromPoint = await GeocodeAsync(request.From, cancellationToken);
        var toPoint = await GeocodeAsync(request.To, cancellationToken);

        var transport = transportMode switch
        {
            TransportMode.Car => "driving",
            TransportMode.Bicycle => "bicycle",
            TransportMode.Scooter => "scooter",
            _ => throw new ArgumentOutOfRangeException(nameof(transportMode), transportMode, null)
        };

        var requestBody = new Dictionary<string, object>
        {
            ["points"] = new[]
            {
                new
                {
                    type = "stop",
                    lon = fromPoint.Lon,
                    lat = fromPoint.Lat
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
            ["locale"] = "en"
        };

        if (transportMode == TransportMode.Car)
            requestBody["traffic_mode"] = "jam";

        var url = $"{RoutesUrl}?key={Uri.EscapeDataString(_options.ApiKey)}";

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);

        httpRequest.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"2GIS Routing API returned {(int)response.StatusCode}: " + responseContent);

        using var document = JsonDocument.Parse(responseContent);

        if (!document.RootElement.TryGetProperty("status", out var statusElement))
            throw new InvalidOperationException("2GIS response does not contain status.");

        var status = statusElement.GetString();

        if (!string.Equals(status, "OK", StringComparison.OrdinalIgnoreCase))
        {
            var message = document.RootElement.TryGetProperty("message", out var messageElement) ? messageElement.GetString() : null;

            throw new InvalidOperationException($"2GIS Routing API failed. Status: {status}. Message: {message}");
        }

        if (!document.RootElement.TryGetProperty("result", out var routes) || routes.GetArrayLength() == 0) 
            throw new InvalidOperationException("2GIS Routing API returned no routes.");

        var route = routes[0];

        var distanceMeters = route.TryGetProperty("total_distance", out var distanceElement) ? distanceElement.GetDouble() : 0;

        var durationSeconds = route.TryGetProperty("total_duration", out var durationElement) ? durationElement.GetDouble() : 0;

        if (distanceMeters <= 0 || durationSeconds <= 0)
            throw new InvalidOperationException("2GIS returned an invalid route.");

        var trafficUsed = transportMode == TransportMode.Car;

        return new ProviderRouteResult
        {
            Provider = Name,
            TransportMode = TransportModeToString(transportMode),
            DistanceKm = Math.Round(distanceMeters / 1000.0, 3),
            DurationMinutes = Math.Round(durationSeconds / 60.0, 2),
            DurationWithoutTrafficMinutes = null,
            TrafficAvailable = trafficUsed,
            TrafficUsed = trafficUsed
        };
    }

    private async Task<GeoPoint> GeocodeAsync(string address, CancellationToken cancellationToken)
    {
        var url = $"{GeocodeUrl}" +
                  $"?key={Uri.EscapeDataString(_options.ApiKey)}" +
                  $"&q={Uri.EscapeDataString(address)}" +
                  "&fields=items.point";

        using var response = await _httpClient.GetAsync(url, cancellationToken);

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"2GIS Geocoder returned {(int)response.StatusCode}: " + responseContent);

        using var document = JsonDocument.Parse(responseContent);

        if (!document.RootElement.TryGetProperty("result", out var result))
            throw new InvalidOperationException($"2GIS Geocoder returned invalid response: {responseContent}");

        if (!result.TryGetProperty("items", out var items) || items.GetArrayLength() == 0)
            throw new InvalidOperationException($"2GIS could not geocode address: '{address}'");

        var point = items[0].TryGetProperty("point", out var pointElement) ? pointElement : throw new InvalidOperationException($"2GIS geocoder returned no coordinates for '{address}'");

        var lat = point.GetProperty("lat").GetDouble();
        var lon = point.GetProperty("lon").GetDouble();

        return new GeoPoint(lat, lon);
    }

    private static string TransportModeToString(TransportMode mode)
    {
        return mode switch
        {
            TransportMode.Car => "car",
            TransportMode.Bicycle => "bicycle",
            TransportMode.Scooter => "scooter",
            _ => mode.ToString().ToLowerInvariant()
        };
    }

    private sealed record GeoPoint(double Lat, double Lon);
}