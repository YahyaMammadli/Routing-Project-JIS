using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using RoutingService.Application.Interfaces;
using RoutingService.Application.Models;
using RoutingService.Domain.Enums;
using System.Globalization;
using System.Text.Json;
namespace RoutingService.Infrastructure.RoutingProviders.Yandex;
public class YandexRoutingProvider : IRoutingProvider
{
    private const string DistanceMatrixUrl = "https://api.routing.yandex.net/v2/distancematrix";
    private const string TwoGisGeocodeUrl = "https://catalog.api.2gis.com/3.0/items/geocode";

    private readonly HttpClient _httpClient;
    private readonly YandexOptions _options;
    private readonly IConfiguration _configuration;

    public string Name => "yandex";

    public YandexRoutingProvider(
        HttpClient httpClient,
        IOptions<YandexOptions> options,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _configuration = configuration;

        if (string.IsNullOrWhiteSpace(_options.DistanceMatrixApiKey))
        {
            throw new InvalidOperationException(
                "Yandex Distance Matrix API key is not configured.");
        }
    }

    public async Task<ProviderRouteResult> CalculateAsync(
        RouteCalculationRequest request,
        TransportMode transportMode,
        CancellationToken cancellationToken = default)
    {
        var fromPoint = await GeocodeWithTwoGisAsync(
            request.From,
            cancellationToken);

        var toPoint = await GeocodeWithTwoGisAsync(
            request.To,
            cancellationToken);

        var mode = transportMode switch
        {
            TransportMode.Car => "driving",
            TransportMode.Bicycle => "bicycle",
            TransportMode.Scooter => "scooter",

            _ => throw new ArgumentOutOfRangeException(
                nameof(transportMode),
                transportMode,
                null)
        };

        var route = await GetDistanceMatrixAsync(
            fromPoint,
            toPoint,
            mode,
            trafficEnabled: transportMode == TransportMode.Car,
            cancellationToken);

        double? durationWithoutTraffic = null;

        if (transportMode == TransportMode.Car)
        {
            var routeWithoutTraffic =
                await GetDistanceMatrixAsync(
                    fromPoint,
                    toPoint,
                    mode,
                    trafficEnabled: false,
                    cancellationToken);

            durationWithoutTraffic =
                routeWithoutTraffic.DurationMinutes;
        }

        var routeOption = new RouteOption
        {
            RouteNumber = 1,
            DistanceKm = route.DistanceKm,
            DurationMinutes = route.DurationMinutes,
            Algorithm = "Yandex Distance Matrix"
        };

        return new ProviderRouteResult
        {
            Provider = Name,
            TransportMode = TransportModeToString(transportMode),
            DistanceKm = route.DistanceKm,
            DurationMinutes = route.DurationMinutes,
            DurationWithoutTrafficMinutes = durationWithoutTraffic,
            TrafficAvailable = transportMode == TransportMode.Car,
            TrafficUsed = transportMode == TransportMode.Car,
            Routes = [routeOption]
        };
    }

    private async Task<GeoPoint> GeocodeWithTwoGisAsync(
        string address,
        CancellationToken cancellationToken)
    {
        var twoGisApiKey =
            _configuration["RoutingProviders:TwoGis:ApiKey"];

        if (string.IsNullOrWhiteSpace(twoGisApiKey))
        {
            throw new InvalidOperationException(
                "2GIS API key is not configured. " +
                "Yandex Distance Matrix requires coordinates, " +
                "so the address must first be geocoded.");
        }

        var url =
            $"{TwoGisGeocodeUrl}" +
            $"?key={Uri.EscapeDataString(twoGisApiKey)}" +
            $"&q={Uri.EscapeDataString(address)}" +
            "&fields=items.point";

        using var response =
            await _httpClient.GetAsync(
                url,
                cancellationToken);

        var responseContent =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"2GIS Geocoder returned " +
                $"{(int)response.StatusCode}: " +
            responseContent);
        }

        using var document =
            JsonDocument.Parse(responseContent);

        if (!document.RootElement.TryGetProperty(
                "result",
                out var result))
        {
            throw new InvalidOperationException(
                "2GIS Geocoder returned invalid response.");
        }

        if (!result.TryGetProperty(
                "items",
                out var items) ||
            items.GetArrayLength() == 0)
        {
            throw new InvalidOperationException(
                $"2GIS could not geocode address: '{address}'");
        }

        var point =
            items[0].GetProperty("point");

        var latitude =
            point.GetProperty("lat").GetDouble();

        var longitude =
            point.GetProperty("lon").GetDouble();

        return new GeoPoint(
            latitude,
            longitude);
    }

    private async Task<RouteData> GetDistanceMatrixAsync(GeoPoint from,GeoPoint to,string mode,bool trafficEnabled,CancellationToken cancellationToken)
    {
        var origins =
            string.Create(
                CultureInfo.InvariantCulture,
                $"{from.Lat},{from.Lon}");

        var destinations =
            string.Create(
                CultureInfo.InvariantCulture,
                $"{to.Lat},{to.Lon}");

        var url =
            $"{DistanceMatrixUrl}" +
            $"?apikey={Uri.EscapeDataString(_options.DistanceMatrixApiKey)}" +
            $"&origins={Uri.EscapeDataString(origins)}" +
            $"&destinations={Uri.EscapeDataString(destinations)}" +
            $"&mode={Uri.EscapeDataString(mode)}";

        if (!trafficEnabled && mode == "driving")
        {
            url += "&traffic=disabled";
        }

        using var response =
            await _httpClient.GetAsync(
                url,
                cancellationToken);

        var responseContent =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Yandex Distance Matrix API returned " +
                $"{(int)response.StatusCode}: " +
                responseContent);
        }

        using var document =
            JsonDocument.Parse(responseContent);

        if (document.RootElement.TryGetProperty(
                "errors",
                out var errors))
        {
            throw new InvalidOperationException(
                $"Yandex Distance Matrix API returned an error: " +
                errors);
        }

        if (!document.RootElement.TryGetProperty(
                "rows",
                out var rows) ||
            rows.GetArrayLength() == 0)
        {
            throw new InvalidOperationException(
                "Yandex Distance Matrix returned no rows.");
        }

        var row = rows[0];

        if (!row.TryGetProperty(
                "elements",
                out var elements) ||
            elements.GetArrayLength() == 0)
        {
            throw new InvalidOperationException(
                "Yandex Distance Matrix returned no elements.");
        }

        var element = elements[0];

        var status =
            element.TryGetProperty(
                "status",
                out var statusElement)
                ? statusElement.GetString()
                : null;

        if (!string.Equals(
                status,
                "OK",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Yandex Distance Matrix route failed. " +
                $"Status: {status}");
        }

        if (!element.TryGetProperty(
                "distance",
                out var distanceElement))
        {
            throw new InvalidOperationException(
                "Yandex response does not contain distance.");
        }

        if (!element.TryGetProperty(
                "duration",
                out var durationElement))
        {
            throw new InvalidOperationException(
                "Yandex response does not contain duration.");
        }

        var distanceMeters =
            distanceElement
                .GetProperty("value")
                .GetDouble();

        var durationSeconds =
            durationElement
                .GetProperty("value")
                .GetDouble(); if (distanceMeters <= 0 ||
        durationSeconds <= 0)
        {
            throw new InvalidOperationException(
                "Yandex returned an invalid route.");
        }

        return new RouteData
        {
            DistanceKm =
                Math.Round(
                    distanceMeters / 1000.0,
                    3),

            DurationMinutes =
                Math.Round(
                    durationSeconds / 60.0,
                    2)
        };
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

    private sealed record GeoPoint(
        double Lat,
        double Lon);

    private sealed class RouteData
    {
        public double DistanceKm { get; init; }

        public double DurationMinutes { get; init; }
    }
}