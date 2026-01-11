using Xunit;
using Svns.Models;

namespace Svns.Tests.Models;

public class StatusFilterTypeTests
{
    [Fact]
    public void All_IsDefaultValue()
    {
        var filter = default(StatusFilterType);
        Assert.Equal(StatusFilterType.All, filter);
    }

    [Fact]
    public void AllEnumValues_AreUnique()
    {
        var values = Enum.GetValues<StatusFilterType>();
        var uniqueValues = values.Distinct().ToArray();
        Assert.Equal(values.Length, uniqueValues.Length);
    }

    [Theory]
    [InlineData(StatusFilterType.All)]
    [InlineData(StatusFilterType.Modified)]
    [InlineData(StatusFilterType.Added)]
    [InlineData(StatusFilterType.Deleted)]
    [InlineData(StatusFilterType.Conflicted)]
    [InlineData(StatusFilterType.Unversioned)]
    [InlineData(StatusFilterType.LocalChanges)]
    public void EnumValues_CanBeCast(StatusFilterType filterType)
    {
        var intValue = (int)filterType;
        var backToEnum = (StatusFilterType)intValue;
        Assert.Equal(filterType, backToEnum);
    }
}
