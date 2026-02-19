using System.Globalization;

namespace Bogoware.Localization.Tests.Helpers;

/// <summary>
/// Simple marker type implementing ILocalizable for testing.
/// </summary>
public class TestLocalizable : ILocalizable;

/// <summary>
/// Self-localizing type for testing ILocalizableProvider.
/// </summary>
public class TestSelfProvider(string value) : ILocalizationProvider
{
    public string Localize(CultureInfo? culture = null) => value;
}

/// <summary>
/// DI provider for TestLocalizable.
/// </summary>
public class TestDiProvider : ILocalizationProvider<TestLocalizable>
{
    public string Localize(TestLocalizable value, CultureInfo? culture = null) => "from DI provider";
}


/// <summary>
/// Simulates a validation error with a FieldName property for template substitution tests.
/// Template placeholder: {FieldName}
/// </summary>
public class TestRequiredFieldError(string fieldName) : ILocalizable
{
    public string FieldName { get; } = fieldName;
}

/// <summary>
/// Simulates a validation error with FieldName and MaxLength properties.
/// Template placeholders: {FieldName}, {MaxLength}
/// </summary>
public class TestMaxLengthError(string fieldName, int maxLength) : ILocalizable
{
    public string FieldName { get; } = fieldName;
    public int MaxLength { get; } = maxLength;
}

/// <summary>
/// Simulates a validation error with FieldName, Min, and Max properties.
/// Template placeholders: {FieldName}, {Min}, {Max}
/// </summary>
public class TestOutOfRangeError(string fieldName, object? fieldValue, object min, object max) : ILocalizable
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
public class TestMustBeGreaterThanError(string fieldName, object min) : ILocalizable
{
    public string FieldName { get; } = fieldName;
    public object Min { get; } = min;
}

/// <summary>
/// Simulates a validation error with FieldName and ExpectedFormat properties.
/// Template placeholders: {FieldName}, {ExpectedFormat}
/// </summary>
public class TestInvalidFormatError(string fieldName, string expectedFormat) : ILocalizable
{
    public string FieldName { get; } = fieldName;
    public string ExpectedFormat { get; } = expectedFormat;
}

/// <summary>
/// Simulates a simple validation error with only FieldName (for email, fiscal code, etc.).
/// Template placeholder: {FieldName}
/// </summary>
public class TestInvalidEmailError(string fieldName) : ILocalizable
{
    public string FieldName { get; } = fieldName;
}

/// <summary>
/// Simulates a type with declared-only property for BuildFallback test.
/// Has a FieldName inherited-style property excluded from DeclaredOnly, and MaxLength as its own property.
/// </summary>
public class TestFallbackError : ILocalizable
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

/// <summary>
/// Non-ILocalizable complex type with a custom ToString() for exhaustive mode fallback testing.
/// </summary>
public class NonLocalizableAddress
{
    public string Street { get; set; } = "";
    public string City { get; set; } = "";

    public override string ToString() => $"{Street}, {City}";
}
