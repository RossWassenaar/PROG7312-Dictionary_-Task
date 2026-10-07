using System.Globalization;

namespace ProductInventory.Tests;

public class RandFormatTests
{
    [Theory]
    [InlineData("1299.99", "1299.99")]
    [InlineData("1299,99", "1299.99")]
    [InlineData("R1 299.99", "1299.99")]
    [InlineData("1,299.99", "1299.99")]
    [InlineData("r50", "50")]
    public void TryParse_accepts_the_usual_ways_of_writing_rand(string text, string expected)
    {
        Assert.True(RandFormat.TryParse(text, out decimal amount));
        Assert.Equal(decimal.Parse(expected, CultureInfo.InvariantCulture), amount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("-5")]
    public void TryParse_rejects_things_that_are_not_prices(string text)
    {
        Assert.False(RandFormat.TryParse(text, out _));
    }

    [Fact]
    public void Format_uses_space_thousands_and_a_decimal_point()
    {
        Assert.Equal("R5 000.00", RandFormat.Format(5000m));
        Assert.Equal("R349.99", RandFormat.Format(349.99m));
        Assert.Equal("R33 547", RandFormat.FormatWhole(33546.79m));
    }

    [Fact]
    public void Rands_and_cents_split_a_price_for_the_price_tag()
    {
        Assert.Equal("5000", RandFormat.Rands(5000m));
        Assert.Equal("00", RandFormat.Cents(5000m));
        Assert.Equal("349", RandFormat.Rands(349.99m));
        Assert.Equal("99", RandFormat.Cents(349.99m));
    }
}
