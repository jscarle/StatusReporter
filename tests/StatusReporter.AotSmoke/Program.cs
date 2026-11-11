using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace StatusReporter.AotSmoke;

internal static class Program
{
    public static async Task Main()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var cancellationToken = timeout.Token;
        var builder = WebApplication.CreateSlimBuilder();
        builder.Services.AddStatusReporter();

        await using var app = builder.Build();
        app.MapStatus();
        app.MapGroup("/internal").MapStatus("/health")
            .WithName("InternalStatus")
            .AddEndpointFilter((context, next) =>
            {
                context.HttpContext.Response.Headers["X-Status-Smoke"] = "applied";
                return next(context);
            });
        app.Urls.Add("http://127.0.0.1:0");

        await app.StartAsync(cancellationToken);

        try
        {
            var endpoints = ((IEndpointRouteBuilder)app).DataSources.SelectMany(source => source.Endpoints).ToArray();
            if (!endpoints.Any(endpoint => string.Equals(endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName, "GetStatus", StringComparison.Ordinal))
                || !endpoints.Any(endpoint => string.Equals(endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName, "InternalStatus", StringComparison.Ordinal)))
                throw new InvalidOperationException("Default and grouped endpoint name conventions must be preserved.");

            var server = app.Services.GetRequiredService<IServer>();
            var address = server.Features.Get<IServerAddressesFeature>()?.Addresses.Single()
                ?? throw new InvalidOperationException("The smoke test server did not expose its address.");
            using var client = new HttpClient();
            client.BaseAddress = new Uri(address);

            var statusReporter = app.Services.GetRequiredService<IStatusReporter>();
            var applicationStatus = statusReporter.GetStatus();
            if (applicationStatus.VersionId == Guid.Empty
                || applicationStatus.BuiltOn != new DateTimeOffset(2026, 11, 10, 12, 0, 0, TimeSpan.Zero)
                || string.IsNullOrWhiteSpace(applicationStatus.Assembly)
                || string.IsNullOrWhiteSpace(applicationStatus.TargetFramework))
                throw new InvalidOperationException("Entry assembly metadata must remain available in the native executable.");

            await MapStatus_WhenSystemInformationDisabled_ReturnsPublicFieldsAsync(client, applicationStatus, cancellationToken);

            var options = app.Services.GetRequiredService<StatusReporterOptions>();
            options.IncludeSystemInformation = true;

            await MapStatus_WhenSystemInformationEnabled_ReturnsSystemFieldsAsync(client, applicationStatus, cancellationToken);

            Console.WriteLine("Native AOT status endpoint smoke tests passed.");
        }
        finally
        {
            await app.StopAsync(CancellationToken.None);
        }
    }

    private static async Task MapStatus_WhenSystemInformationDisabled_ReturnsPublicFieldsAsync(HttpClient client, ApplicationStatus applicationStatus, CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync("/status", cancellationToken);
        if (response.StatusCode != HttpStatusCode.OK || !string.Equals(response.Content.Headers.ContentType?.MediaType, "application/json", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("The default status endpoint must return HTTP 200 with JSON content.");

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var document = JsonDocument.Parse(json);
        var status = document.RootElement;
        string[] expectedFields = ["versionId", "version", "builtOn", "startedOn", "current", "uptime"];
        var actualFields = status.EnumerateObject().Select(property => property.Name).ToArray();
        if (!actualFields.Order(StringComparer.Ordinal).SequenceEqual(expectedFields.Order(StringComparer.Ordinal), StringComparer.Ordinal))
            throw new InvalidOperationException("The default response must contain exactly the public version and timing fields.");

        if (status.GetProperty("versionId").GetGuid() != applicationStatus.VersionId
            || !string.Equals(status.GetProperty("version").GetString(), applicationStatus.Version, StringComparison.Ordinal)
            || status.GetProperty("builtOn").GetDateTimeOffset() != applicationStatus.BuiltOn)
            throw new InvalidOperationException("The default response must preserve the entry assembly version and build timestamp.");
    }

    private static async Task MapStatus_WhenSystemInformationEnabled_ReturnsSystemFieldsAsync(HttpClient client, ApplicationStatus applicationStatus, CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync("/internal/health", cancellationToken);
        if (response.StatusCode != HttpStatusCode.OK || !string.Equals(response.Content.Headers.ContentType?.MediaType, "application/json", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("The grouped status endpoint must return HTTP 200 with JSON content.");

        if (!response.Headers.TryGetValues("X-Status-Smoke", out var filterHeaderValues)
            || !filterHeaderValues.Contains("applied", StringComparer.Ordinal))
            throw new InvalidOperationException("Endpoint filters chained onto MapStatus must execute.");

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var document = JsonDocument.Parse(json);
        var status = document.RootElement;
        if (status.EnumerateObject().Count() != 13
            || !string.Equals(status.GetProperty("assembly").GetString(), applicationStatus.Assembly, StringComparison.Ordinal)
            || !string.Equals(status.GetProperty("targetFramework").GetString(), applicationStatus.TargetFramework, StringComparison.Ordinal)
            || !string.Equals(status.GetProperty("framework").GetString(), applicationStatus.Framework, StringComparison.Ordinal)
            || !string.Equals(status.GetProperty("hostname").GetString(), applicationStatus.Hostname, StringComparison.Ordinal)
            || !string.Equals(status.GetProperty("runtimeIdentifier").GetString(), applicationStatus.RuntimeIdentifier, StringComparison.Ordinal)
            || !string.Equals(status.GetProperty("operatingSystem").GetString(), applicationStatus.OperatingSystem, StringComparison.Ordinal)
            || !string.Equals(status.GetProperty("environment").GetString(), applicationStatus.Environment, StringComparison.Ordinal))
            throw new InvalidOperationException("Enabling system information must include every system field in the grouped response.");
    }
}
