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

/// <summary>
/// Struct implementing ILocalizable for testing struct support (including Nullable&lt;T&gt;).
/// Template placeholder: {FieldName}
/// </summary>
public readonly record struct StructRequiredFieldError(string FieldName) : ILocalizable;

/// <summary>
/// Struct implementing ILocalizationProvider (self-localizing) for testing struct self-provider support.
/// </summary>
public readonly record struct StructSelfProvider(string Value) : ILocalizationProvider
{
    public string Localize(CultureInfo? culture = null) => Value;
}

/// <summary>
/// Record class implementing ILocalizable for testing record class support.
/// Template placeholder: {FieldName}
/// </summary>
public record RecordRequiredFieldError(string FieldName) : ILocalizable;

/// <summary>
/// Record class implementing ILocalizationProvider (self-localizing) for testing record class self-provider support.
/// </summary>
public record RecordSelfProvider(string Value) : ILocalizationProvider
{
    public string Localize(CultureInfo? culture = null) => Value;
}

/// <summary>
/// A DI provider that throws during Localize, for testing exception wrapping in TryFormatViaDiProvider.
/// </summary>
public class ThrowingDiProvider : ILocalizationProvider<TestLocalizable>
{
    public string Localize(TestLocalizable value, CultureInfo? culture = null)
        => throw new InvalidOperationException("Provider blew up");
}

/// <summary>
/// A non-ILocalizable type with a DI provider, for testing Format&lt;T&gt; with DI resolution.
/// </summary>
public class NonLocalizableOrder
{
    public string OrderId { get; set; } = "";
}

/// <summary>
/// DI provider for NonLocalizableOrder.
/// </summary>
public class NonLocalizableOrderProvider : ILocalizationProvider<NonLocalizableOrder>
{
    public string Localize(NonLocalizableOrder value, CultureInfo? culture = null)
        => $"Order #{value.OrderId}";
}

/// <summary>
/// Generic localizable type for testing generic FQDN key resolution.
/// Template placeholders: {Value}, {Code}
/// </summary>
public class GenericResult<T>(T value, int code) : ILocalizable
{
    public T Value { get; } = value;
    public int Code { get; } = code;
}

/// <summary>
/// ILocalizable with a nullable property, for testing FormatTemplate null property handling.
/// Template placeholder: {FieldName}, {Details}
/// </summary>
public class TestNullablePropertyError(string fieldName, string? details) : ILocalizable
{
    public string FieldName { get; } = fieldName;
    public string? Details { get; } = details;
}

/// <summary>
/// Inherits from a base class to test BuildFallback DeclaredOnly behavior.
/// Only DeclaredOnly properties (Code) should appear in fallback, not inherited ones (FieldName).
/// </summary>
public class TestBaseError : ILocalizable
{
    public string FieldName { get; }

    public TestBaseError(string fieldName)
    {
        FieldName = fieldName;
    }
}

/// <summary>
/// Derived error that declares its own Code property.
/// BuildFallback with DeclaredOnly should show Code but not FieldName.
/// </summary>
public class TestDerivedError : TestBaseError
{
    public int Code { get; }

    public TestDerivedError(string fieldName, int code) : base(fieldName)
    {
        Code = code;
    }
}

/// <summary>
/// Simulates an invalid currency error for nested localization testing.
/// Template placeholder: {CurrencyCode}
/// </summary>
public class TestInvalidCurrencyError(string currencyCode) : ILocalizable
{
    public string CurrencyCode { get; } = currencyCode;
}

/// <summary>
/// Simulates a payment error containing a nested ILocalizable property for recursive localization testing.
/// Template placeholders: {FieldName}, {Detail}
/// </summary>
public class TestPaymentError(string fieldName, TestInvalidCurrencyError detail) : ILocalizable
{
    public string FieldName { get; } = fieldName;
    public TestInvalidCurrencyError Detail { get; } = detail;
}

/// <summary>
/// Simulates a payment error with a nullable nested ILocalizable property for null-nested testing.
/// Template placeholders: {FieldName}, {Detail}
/// </summary>
public class TestNullablePaymentError(string fieldName, TestInvalidCurrencyError? detail) : ILocalizable
{
    public string FieldName { get; } = fieldName;
    public TestInvalidCurrencyError? Detail { get; } = detail;
}

/// <summary>
/// Simulates a 3-level deep nested error: Transaction → Payment → Currency.
/// Template placeholders: {TransactionId}, {Payment}
/// </summary>
public class TestTransactionError(string transactionId, TestPaymentError payment) : ILocalizable
{
    public string TransactionId { get; } = transactionId;
    public TestPaymentError Payment { get; } = payment;
}
