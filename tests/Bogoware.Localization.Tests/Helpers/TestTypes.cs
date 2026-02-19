using System.Globalization;

namespace Bogoware.Localization.Tests.Helpers;

/// <summary>
/// Simple marker type implementing ILocalizableString for testing.
/// </summary>
public class TestLocalizableString : ILocalizableString;

/// <summary>
/// Self-localizing type for testing ILocalizableStringProvider.
/// </summary>
public class TestSelfProvider(string value) : ILocalizableStringProvider
{
    public string Localize(CultureInfo? culture = null) => value;
}

/// <summary>
/// DI provider for TestLocalizableString.
/// </summary>
public class TestDiProvider : ILocalizableStringProvider<TestLocalizableString>
{
    public string Localize(TestLocalizableString value, CultureInfo? culture = null) => "from DI provider";
}

/// <summary>
/// LocalizedMessage subclass for testing fallback behavior.
/// </summary>
public class TestLocalizedMessage(string fallback) : LocalizedMessage(fallback);

/// <summary>
/// Simulates a validation error with a FieldName property for template substitution tests.
/// Template placeholder: {FieldName}
/// </summary>
public class TestRequiredFieldError(string fieldName) : ILocalizableString
{
    public string FieldName { get; } = fieldName;
}

/// <summary>
/// Simulates a validation error with FieldName and MaxLength properties.
/// Template placeholders: {FieldName}, {MaxLength}
/// </summary>
public class TestMaxLengthError(string fieldName, int maxLength) : ILocalizableString
{
    public string FieldName { get; } = fieldName;
    public int MaxLength { get; } = maxLength;
}

/// <summary>
/// Simulates a validation error with FieldName, Min, and Max properties.
/// Template placeholders: {FieldName}, {Min}, {Max}
/// </summary>
public class TestOutOfRangeError(string fieldName, object? fieldValue, object min, object max) : ILocalizableString
{
    public string FieldName { get; } = fieldName;
    public object? FieldValue { get; } = fieldValue;
    public object Min { get; } = min;
    public object Max { get; } = max;
}

/// <summary>
/// Simulates a validation error with FieldName and Min properties.
/// Template placeholders: {FieldName}, {Min}
/// </summary>
public class TestMustBeGreaterThanError(string fieldName, object min) : ILocalizableString
{
    public string FieldName { get; } = fieldName;
    public object Min { get; } = min;
}

/// <summary>
/// Simulates a validation error with FieldName and ExpectedFormat properties.
/// Template placeholders: {FieldName}, {ExpectedFormat}
/// </summary>
public class TestInvalidFormatError(string fieldName, string expectedFormat) : ILocalizableString
{
    public string FieldName { get; } = fieldName;
    public string ExpectedFormat { get; } = expectedFormat;
}

/// <summary>
/// Simulates a simple validation error with only FieldName (for email, fiscal code, etc.).
/// Template placeholder: {FieldName}
/// </summary>
public class TestInvalidEmailError(string fieldName) : ILocalizableString
{
    public string FieldName { get; } = fieldName;
}

/// <summary>
/// Simulates a type with declared-only property for BuildFallback test.
/// Has a FieldName inherited-style property excluded from DeclaredOnly, and MaxLength as its own property.
/// </summary>
public class TestFallbackError : ILocalizableString
{
    // These are NOT declared-only properties, simulating inheritance
    public string FieldName { get; }
    public object? FieldValue { get; }

    // This IS a declared property on this type
    public int MaxLength { get; }

    public TestFallbackError(string fieldName, object? fieldValue, int maxLength)
    {
        FieldName = fieldName;
        FieldValue = fieldValue;
        MaxLength = maxLength;
    }
}
