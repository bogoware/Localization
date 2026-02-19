using System.Globalization;
using Xunit;

namespace Bogoware.Localization.Tests;

public class JsoncSupportTests
{
    private static readonly CultureInfo EnUs = new("en-US");

    // --- Single-line comments ---

    [Fact]
    public void SingleLineComments_AreParsedSuccessfully()
    {
        var jsonc = """
        {
            // Template for key one
            "Key1": "Value1", // inline comment
            "Key2": "Value2"
            // trailing comment
        }
        """;

        var registry = new JsonLocalizationRegistry();
        registry.LoadFromJson(jsonc, EnUs);

        Assert.True(registry.TryGetTemplate("Key1", EnUs, out var v1));
        Assert.Equal("Value1", v1);
        Assert.True(registry.TryGetTemplate("Key2", EnUs, out var v2));
        Assert.Equal("Value2", v2);
    }

    // --- Block comments ---

    [Fact]
    public void MultiLineComments_AreParsedSuccessfully()
    {
        var jsonc = """
        {
            /* Block comment explaining this section */
            "Key1": "Value1",
            /*
             * Multi-line block comment
             * spanning several lines
             */
            "Key2": "Value2"
        }
        """;

        var registry = new JsonLocalizationRegistry();
        registry.LoadFromJson(jsonc, EnUs);

        Assert.True(registry.TryGetTemplate("Key1", EnUs, out var v1));
        Assert.Equal("Value1", v1);
        Assert.True(registry.TryGetTemplate("Key2", EnUs, out var v2));
        Assert.Equal("Value2", v2);
    }

    // --- Mixed comments ---

    [Fact]
    public void MixedComments_AreParsedSuccessfully()
    {
        var jsonc = """
        {
            // Single-line comment
            "Key1": "Value1", /* inline block */
            /* Block before key */
            "Key2": "Value2" // end-of-line
        }
        """;

        var registry = new JsonLocalizationRegistry();
        registry.LoadFromJson(jsonc, EnUs);

        Assert.True(registry.TryGetTemplate("Key1", EnUs, out var v1));
        Assert.Equal("Value1", v1);
        Assert.True(registry.TryGetTemplate("Key2", EnUs, out var v2));
        Assert.Equal("Value2", v2);
    }

    // --- Trailing commas ---

    [Fact]
    public void TrailingCommas_AreParsedSuccessfully()
    {
        var jsonc = """
        {
            "Key1": "Value1",
            "Key2": "Value2",
        }
        """;

        var registry = new JsonLocalizationRegistry();
        registry.LoadFromJson(jsonc, EnUs);

        Assert.True(registry.TryGetTemplate("Key1", EnUs, out var v1));
        Assert.Equal("Value1", v1);
        Assert.True(registry.TryGetTemplate("Key2", EnUs, out var v2));
        Assert.Equal("Value2", v2);
    }

    // --- Combined JSONC features ---

    [Fact]
    public void CommentsAndTrailingCommas_WorkTogether()
    {
        var jsonc = """
        {
            // Required field
            "Key1": "Value1", /* inline block */
            "Key2": "Value2", // trailing comma follows
        }
        """;

        var registry = new JsonLocalizationRegistry();
        registry.LoadFromJson(jsonc, EnUs);

        Assert.True(registry.TryGetTemplate("Key1", EnUs, out var v1));
        Assert.Equal("Value1", v1);
        Assert.True(registry.TryGetTemplate("Key2", EnUs, out var v2));
        Assert.Equal("Value2", v2);
    }

    // --- Edge cases ---

    [Fact]
    public void EmptyJsoncWithOnlyComments_ReturnsNoTemplates()
    {
        var jsonc = """
        {
            /* nothing here */
            // also nothing
        }
        """;

        var registry = new JsonLocalizationRegistry();
        registry.LoadFromJson(jsonc, EnUs);

        Assert.False(registry.TryGetTemplate("AnyKey", EnUs, out _));
    }

    [Fact]
    public void StandardJson_StillWorksAfterJsoncSupport()
    {
        var json = """{ "Key": "Value" }""";

        var registry = new JsonLocalizationRegistry();
        registry.LoadFromJson(json, EnUs);

        Assert.True(registry.TryGetTemplate("Key", EnUs, out var v));
        Assert.Equal("Value", v);
    }
}
