using System.Globalization;

namespace Bogoware.Localization;

/// <summary>
/// External DI provider that can localize instances of <typeparamref name="T"/>.
/// No constraint on T — any type can have an external localization provider.
/// </summary>
public interface ILocalizableStringProvider<in T>
{
    string Localize(T value, CultureInfo? culture = null);
}
