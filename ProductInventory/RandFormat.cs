using System.Globalization;

namespace ProductInventory;

public static class RandFormat
{
    private static readonly NumberFormatInfo Separators = new()
    {
        NumberGroupSeparator = " ",
        NumberDecimalSeparator = ".",
        NumberGroupSizes = new[] { 3 }
    };

    public static string Format(decimal amount) => "R" + amount.ToString("#,0.00", Separators);

    public static string FormatWhole(decimal amount) => "R" + Math.Round(amount).ToString("#,0", Separators);

    public static bool TryParse(string? text, out decimal amount)
    {
        amount = 0;
        if (string.IsNullOrWhiteSpace(text))
            return false;

        string cleaned = text.Trim().TrimStart('R', 'r').Replace(" ", "").Replace("\u00A0", "");
        cleaned = cleaned.Contains('.') ? cleaned.Replace(",", "") : cleaned.Replace(',', '.');

        return decimal.TryParse(cleaned, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out amount);
    }

    public static string Rands(decimal amount) => decimal.Truncate(amount).ToString(CultureInfo.InvariantCulture);

    public static string Cents(decimal amount) =>
        ((int)((amount - decimal.Truncate(amount)) * 100)).ToString("00", CultureInfo.InvariantCulture);
}
