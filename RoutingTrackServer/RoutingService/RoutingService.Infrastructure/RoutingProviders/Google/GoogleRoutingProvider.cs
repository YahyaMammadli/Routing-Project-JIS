using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RoutingService.Application.Interfaces;
using RoutingService.Application.Models;
using RoutingService.Domain.Enums;

namespace RoutingService.Infrastructure.RoutingProviders.Google;

public class GoogleRoutingProvider : IRoutingProvider
{
    private const string RoutesUrl = "https://routes.googleapis.com/directions/v2:computeRoutes";

    private readonly HttpClient _httpClient;
    private readonly GoogleOptions _options;

    public string Name => "google";

    public GoogleRoutingProvider(HttpClient httpClient, IOptions<GoogleOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new InvalidOperationException("Google Routing API key is not configured.");
        
    }

    public async Task<ProviderRouteResult> CalculateAsync(RouteCalculationRequest request, TransportMode transportMode, CancellationToken cancellationToken = default)
    {
        if (transportMode == TransportMode.Scooter)
            return new ProviderRouteResult
            {
                Provider = Name,
                TransportMode = "scooter",
                Error = "Google Routes API does not provide an electric-scooter travel mode. " + "TWO_WHEELER represents motorized two-wheelers such as motorcycles."
            };
        

        var travelMode = transportMode switch
        {
            TransportMode.Car => "DRIVE",
            TransportMode.Bicycle => "BICYCLE", _ => throw new ArgumentOutOfRangeException(nameof(transportMode), transportMode, null)
        };



        var requestBody = new Dictionary<string, object>
        {
            ["origin"] = new
            {
                address = request.From
            },

            ["destination"] = new
            {
                address = request.To
            },

            ["travelMode"] = travelMode,

            ["languageCode"] = "en-US",

            ["units"] = "METRIC"
        };



        if (transportMode == TransportMode.Car)
            requestBody["routingPreference"] = "TRAFFIC_AWARE";
        

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, RoutesUrl);

        httpRequest.Headers.TryAddWithoutValidation("X-Goog-Api-Key", _options.ApiKey);

        httpRequest.Headers.TryAddWithoutValidation("X-Goog-FieldMask", "routes.distanceMeters,routes.duration,routes.staticDuration");

        httpRequest.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Google Routes API returned {(int)response.StatusCode}: " + responseContent);
        

        using var document = JsonDocument.Parse(responseContent);

        if (!document.RootElement.TryGetProperty("routes", out var routes) || routes.GetArrayLength() == 0)
            throw new InvalidOperationException(
                "Google Routes API returned no routes.");
        

        var route = routes[0];

        var distanceMeters = route.TryGetProperty("distanceMeters", out var distanceElement) ? distanceElement.GetDouble() : 0;

        var durationSeconds = ParseDurationSeconds(route, "duration");

        double? staticDurationMinutes = null;

        if (route.TryGetProperty("staticDuration", out _))
            staticDurationMinutes = ParseDurationSeconds(route, "staticDuration") / 60.0;
        

        var trafficUsed = transportMode == TransportMode.Car;

        return new ProviderRouteResult
        {
            Provider = Name,
            TransportMode = TransportModeToString(transportMode),
            DistanceKm = Math.Round(distanceMeters / 1000.0, 3),
            DurationMinutes = Math.Round(durationSeconds / 60.0, 2),
            DurationWithoutTrafficMinutes = staticDurationMinutes.HasValue ? Math.Round(staticDurationMinutes.Value, 2) : null,
            TrafficAvailable = trafficUsed,
            TrafficUsed = trafficUsed
        };

    }



    private static double ParseDurationSeconds(JsonElement route, string propertyName)
    {
        if (!route.TryGetProperty(propertyName, out var durationElement))
            throw new InvalidOperationException($"Google route response does not contain '{propertyName}'.");
        

        var durationText = durationElement.GetString();

        if (string.IsNullOrWhiteSpace(durationText))
            throw new InvalidOperationException(
                $"Google returned empty '{propertyName}'.");
        

        if (!durationText.EndsWith("s", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Unexpected Google duration format: {durationText}");
        

        var number = durationText[..^1];

        if (!double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds))
            throw new InvalidOperationException(
                $"Unable to parse Google duration: {durationText}");
        

        return seconds;
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


}