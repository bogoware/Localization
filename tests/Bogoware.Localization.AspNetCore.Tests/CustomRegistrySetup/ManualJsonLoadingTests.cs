using System.Text.Json;
using AwesomeAssertions;
using Bogoware.Localization.AspNetCore.Tests.Fixtures;

namespace Bogoware.Localization.AspNetCore.Tests.CustomRegistrySetup;

public sealed class ManualJsonLoadingTests(
    CustomRegistrySetupFixture factory,
    ITestOutputHelper output) : IClassFixture<CustomRegistrySetupFixture>
{
    [Fact]
    public async Task Custom_Templates_From_File_Are_Used()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/orders/99");
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        output.WriteLine($"Custom registry [invariant]: {body}");

        using var doc = JsonDocument.Parse(body);
        var status = doc.RootElement.GetProperty("status").GetString();

        // Should use the custom template: "Custom: Order #{OrderNumber} — {Status}"
        status.Should().Be("Custom: Order #99 — Shipped");
    }
}
