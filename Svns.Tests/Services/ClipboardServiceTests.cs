using Xunit;
using Svns.Services;

namespace Svns.Tests.Services;

public class ClipboardServiceTests
{
    [Fact]
    public void SetTextAsync_MethodExists()
    {
        var service = new ClipboardService();
        var methodInfo = typeof(ClipboardService).GetMethod("SetTextAsync");
        Assert.NotNull(methodInfo);
    }

    [Fact]
    public void GetTextAsync_MethodExists()
    {
        var service = new ClipboardService();
        var methodInfo = typeof(ClipboardService).GetMethod("GetTextAsync");
        Assert.NotNull(methodInfo);
    }

    [Fact]
    public void ClearAsync_MethodExists()
    {
        var service = new ClipboardService();
        var methodInfo = typeof(ClipboardService).GetMethod("ClearAsync");
        Assert.NotNull(methodInfo);
    }

    [Fact]
    public void SetTextAsync_ReturnsTaskOfBool()
    {
        var service = new ClipboardService();
        var methodInfo = typeof(ClipboardService).GetMethod("SetTextAsync");
        Assert.NotNull(methodInfo);
        Assert.Equal(typeof(bool), methodInfo.ReturnType.GetGenericArguments()[0]);
    }

    [Fact]
    public void GetTextAsync_ReturnsTaskOfString()
    {
        var service = new ClipboardService();
        var methodInfo = typeof(ClipboardService).GetMethod("GetTextAsync");
        Assert.NotNull(methodInfo);
        Assert.Equal(typeof(string), methodInfo.ReturnType.GetGenericArguments()[0]);
    }

    [Fact]
    public void ClearAsync_ReturnsTask()
    {
        var service = new ClipboardService();
        var methodInfo = typeof(ClipboardService).GetMethod("ClearAsync");
        Assert.NotNull(methodInfo);
        Assert.Equal(typeof(System.Threading.Tasks.Task), methodInfo.ReturnType);
    }

    [Fact]
    public void ClipboardService_HasDefaultConstructor()
    {
        var service = new ClipboardService();
        Assert.NotNull(service);
    }
}
