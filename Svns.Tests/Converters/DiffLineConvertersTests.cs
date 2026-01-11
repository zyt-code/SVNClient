using Xunit;
using Svns.Converters;
using Svns.ViewModels;
using Avalonia.Media;
using System.Globalization;

namespace Svns.Tests.Converters;

public class DiffLineConvertersTests
{
    [Theory]
    [InlineData(DiffLineType.Added)]
    [InlineData(DiffLineType.Removed)]
    [InlineData(DiffLineType.Header)]
    [InlineData(DiffLineType.FileHeader)]
    [InlineData(DiffLineType.Context)]
    public void DiffLineTypeToBackgroundConverter_ReturnsBrush(DiffLineType type)
    {
        var converter = new DiffLineTypeToBackgroundConverter();
        var result = converter.Convert(type, typeof(IBrush), null, CultureInfo.InvariantCulture);
        Assert.IsAssignableFrom<IBrush>(result);
    }

    [Fact]
    public void DiffLineTypeToBackgroundConverter_ReturnsTransparent_ForContext()
    {
        var converter = new DiffLineTypeToBackgroundConverter();
        var result = converter.Convert(DiffLineType.Context, typeof(IBrush), null, CultureInfo.InvariantCulture);
        Assert.Equal(Brushes.Transparent, result);
    }

    [Fact]
    public void DiffLineTypeToBackgroundConverter_ReturnsTransparent_ForInvalidInput()
    {
        var converter = new DiffLineTypeToBackgroundConverter();
        var result = converter.Convert("invalid", typeof(IBrush), null, CultureInfo.InvariantCulture);
        Assert.Equal(Brushes.Transparent, result);
    }

    [Theory]
    [InlineData(DiffLineType.Added)]
    [InlineData(DiffLineType.Removed)]
    [InlineData(DiffLineType.Header)]
    [InlineData(DiffLineType.FileHeader)]
    [InlineData(DiffLineType.Context)]
    public void DiffLineTypeToForegroundConverter_ReturnsBrush(DiffLineType type)
    {
        var converter = new DiffLineTypeToForegroundConverter();
        var result = converter.Convert(type, typeof(IBrush), null, CultureInfo.InvariantCulture);
        Assert.IsAssignableFrom<IBrush>(result);
    }

    [Fact]
    public void DiffLineTypeToForegroundConverter_ReturnsBrush_ForInvalidInput()
    {
        var converter = new DiffLineTypeToForegroundConverter();
        var result = converter.Convert("invalid", typeof(IBrush), null, CultureInfo.InvariantCulture);
        Assert.IsAssignableFrom<IBrush>(result);
    }

    [Fact]
    public void DiffLineTypeToBackgroundConverter_ConvertBack_ThrowsNotImplementedException()
    {
        var converter = new DiffLineTypeToBackgroundConverter();
        Assert.Throws<NotImplementedException>(() =>
            converter.ConvertBack(null, typeof(DiffLineType), null, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void DiffLineTypeToForegroundConverter_ConvertBack_ThrowsNotImplementedException()
    {
        var converter = new DiffLineTypeToForegroundConverter();
        Assert.Throws<NotImplementedException>(() =>
            converter.ConvertBack(null, typeof(DiffLineType), null, CultureInfo.InvariantCulture));
    }
}
