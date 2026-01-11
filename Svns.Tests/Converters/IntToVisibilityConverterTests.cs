using Xunit;
using Svns.Converters;
using System.Globalization;

namespace Svns.Tests.Converters;

public class IntToVisibilityConverterTests
{
    private readonly IntToVisibilityConverter _converter = new();

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public void Convert_ReturnsTrue_WhenValueIsGreaterThanZero(int value)
    {
        var result = _converter.Convert(value, typeof(bool), null, CultureInfo.InvariantCulture);

        Assert.Equal(true, result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(int.MinValue)]
    public void Convert_ReturnsFalse_WhenValueIsZeroOrNegative(int value)
    {
        var result = _converter.Convert(value, typeof(bool), null, CultureInfo.InvariantCulture);

        Assert.Equal(false, result);
    }

    [Fact]
    public void Convert_ReturnsFalse_WhenValueIsNull()
    {
        var result = _converter.Convert(null, typeof(bool), null, CultureInfo.InvariantCulture);

        Assert.Equal(false, result);
    }

    [Theory]
    [InlineData("not a number")]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("")]
    public void Convert_ReturnsFalse_WhenValueIsNotInteger(object value)
    {
        var result = _converter.Convert(value, typeof(bool), null, CultureInfo.InvariantCulture);

        Assert.Equal(false, result);
    }

    [Fact]
    public void Convert_ReturnsFalse_WhenValueIsLong()
    {
        var result = _converter.Convert(1L, typeof(bool), null, CultureInfo.InvariantCulture);

        Assert.Equal(false, result);
    }

    [Fact]
    public void Convert_ReturnsFalse_WhenValueIsDouble()
    {
        var result = _converter.Convert(1.5, typeof(bool), null, CultureInfo.InvariantCulture);

        Assert.Equal(false, result);
    }

    [Fact]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        Assert.Throws<NotImplementedException>(() =>
            _converter.ConvertBack(true, typeof(int), null, CultureInfo.InvariantCulture));
    }
}
