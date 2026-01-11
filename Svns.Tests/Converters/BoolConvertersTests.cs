using Xunit;
using Svns.Converters;
using Avalonia.Media;
using System.Globalization;

namespace Svns.Tests.Converters;

public class BoolConvertersTests
{
    [Fact]
    public void BoolToStringConverter_ReturnsTrue_WhenBoolIsTrue()
    {
        var converter = new BoolToStringConverter();
        var result = converter.Convert(true, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal("True", result);
    }

    [Fact]
    public void BoolToStringConverter_ReturnsFalse_WhenBoolIsFalse()
    {
        var converter = new BoolToStringConverter();
        var result = converter.Convert(false, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal("False", result);
    }

    [Fact]
    public void BoolToStringConverter_UsesCustomValues()
    {
        var converter = new BoolToStringConverter
        {
            TrueValue = "Yes",
            FalseValue = "No"
        };

        Assert.Equal("Yes", converter.Convert(true, typeof(string), null, CultureInfo.InvariantCulture));
        Assert.Equal("No", converter.Convert(false, typeof(string), null, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void BoolToStringConverter_UsesParameterFormat()
    {
        var converter = new BoolToStringConverter();
        var result = converter.Convert(true, typeof(string), "Active;Inactive", CultureInfo.InvariantCulture);

        Assert.Equal("Active", result);

        var resultFalse = converter.Convert(false, typeof(string), "Active;Inactive", CultureInfo.InvariantCulture);
        Assert.Equal("Inactive", resultFalse);
    }

    [Fact]
    public void BoolToStringConverter_ReturnsFalseValue_WhenValueIsNotBool()
    {
        var converter = new BoolToStringConverter { FalseValue = "N/A" };
        var result = converter.Convert("string", typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal("N/A", result);
    }

    [Fact]
    public void BoolToStringConverter_ReturnsFalseValue_WhenValueIsNull()
    {
        var converter = new BoolToStringConverter { FalseValue = "N/A" };
        var result = converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal("N/A", result);
    }

    [Fact]
    public void BoolToBrushConverter_ReturnsTrueBrush_WhenTrue()
    {
        var converter = new BoolToBrushConverter
        {
            TrueBrush = Brushes.Blue,
            FalseBrush = Brushes.Red
        };

        var result = converter.Convert(true, typeof(IBrush), null, CultureInfo.InvariantCulture);

        Assert.Same(Brushes.Blue, result);
    }

    [Fact]
    public void BoolToBrushConverter_ReturnsFalseBrush_WhenFalse()
    {
        var converter = new BoolToBrushConverter
        {
            TrueBrush = Brushes.Blue,
            FalseBrush = Brushes.Red
        };

        var result = converter.Convert(false, typeof(IBrush), null, CultureInfo.InvariantCulture);

        Assert.Same(Brushes.Red, result);
    }

    [Fact]
    public void BoolToBrushConverter_UsesParameterColors_WhenValid()
    {
        var converter = new BoolToBrushConverter();
        var result = converter.Convert(true, typeof(IBrush), "#00FF00;#FF0000", CultureInfo.InvariantCulture);

        Assert.NotNull(result);
        Assert.IsType<SolidColorBrush>(result);
    }

    [Fact]
    public void BoolToBrushConverter_ReturnsFalseBrush_WhenValueIsNull()
    {
        var converter = new BoolToBrushConverter { FalseBrush = Brushes.Gray };
        var result = converter.Convert(null, typeof(IBrush), null, CultureInfo.InvariantCulture);

        Assert.Same(Brushes.Gray, result);
    }

    [Fact]
    public void BoolToBrushConverter_ReturnsFalseBrush_WhenValueIsNotBool()
    {
        var converter = new BoolToBrushConverter { FalseBrush = Brushes.Gray };
        var result = converter.Convert("not a bool", typeof(IBrush), null, CultureInfo.InvariantCulture);

        Assert.Same(Brushes.Gray, result);
    }
}
