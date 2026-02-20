using Bogoware.Localization;

namespace Bogoware.Localization.Sample.Api.Models;

/// <summary>
/// Represents an order status that can be localized per culture.
/// Template: "Order #{OrderNumber} is {Status}"
/// </summary>
public record OrderStatus(int OrderNumber, string Status) : ILocalizable;

/// <summary>
/// Represents a validation error with a localized message.
/// Template: "'{FieldName}' is required"
/// </summary>
public record RequiredFieldError(string FieldName) : ILocalizable;

/// <summary>
/// Represents an invalid email validation error.
/// Template: "'{FieldName}' is not a valid email address"
/// </summary>
public record InvalidEmailError(string FieldName) : ILocalizable;

/// <summary>
/// Represents a max-length validation error.
/// Template: "'{FieldName}' must not exceed {MaxLength} characters"
/// </summary>
public record MaxLengthError(string FieldName, int MaxLength) : ILocalizable;

/// <summary>
/// A response DTO that contains both plain and localizable properties.
/// </summary>
public record OrderResponse(int Id, OrderStatus Status, decimal Total);

/// <summary>
/// A response DTO containing a list of validation errors.
/// </summary>
public record ValidationErrorsResponse(List<ILocalizable> Errors);
