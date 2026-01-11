using Xunit;
using Svns.Services.Svn.Core;

namespace Svns.Tests.Services;

public class SvnCommandTests
{
    [Fact]
    public void Status_CreatesCorrectCommand()
    {
        var command = SvnCommand.Status(@"C:\repo");
        var args = command.BuildArguments();
        Assert.Equal("status", args[0]);
        Assert.Contains(@"C:\repo", args);
    }

    [Fact]
    public void Status_WithVerbose_AddsVerboseFlag()
    {
        var command = SvnCommand.Status(@"C:\repo", verbose: true);
        var args = command.BuildArguments();
        Assert.Contains("-v", args);
    }

    [Fact]
    public void Update_CreatesCorrectCommand()
    {
        var command = SvnCommand.Update(@"C:\repo");
        var args = command.BuildArguments();
        Assert.Equal("update", args[0]);
    }

    [Fact]
    public void Update_WithRevision_AddsRevisionFlag()
    {
        var command = SvnCommand.Update(@"C:\repo", revision: 123);
        var args = command.BuildArguments();
        Assert.Contains("-r123", args);
    }

    [Fact]
    public void Commit_CreatesCorrectCommand()
    {
        var command = SvnCommand.Commit("Test message", new[] { "file.cs" });
        var args = command.BuildArguments();
        Assert.Equal("commit", args[0]);
        Assert.Contains("-m", args);
        Assert.Contains("Test message", args);
        Assert.Contains("file.cs", args);
    }

    [Fact]
    public void Add_CreatesCorrectCommand()
    {
        var command = SvnCommand.Add(@"C:\repo\file.cs");
        var args = command.BuildArguments();
        Assert.Equal("add", args[0]);
        Assert.Contains(@"C:\repo\file.cs", args);
    }

    [Fact]
    public void Add_WithForce_AddsForceFlag()
    {
        var command = SvnCommand.Add(@"C:\repo\file.cs", force: true);
        var args = command.BuildArguments();
        Assert.Contains("--force", args);
    }

    [Fact]
    public void Delete_CreatesCorrectCommand()
    {
        var command = SvnCommand.Delete(@"C:\repo\file.cs");
        var args = command.BuildArguments();
        Assert.Equal("delete", args[0]);
    }

    [Fact]
    public void Revert_CreatesCorrectCommand()
    {
        var command = SvnCommand.Revert(@"C:\repo\file.cs");
        var args = command.BuildArguments();
        Assert.Equal("revert", args[0]);
        Assert.Contains(@"C:\repo\file.cs", args);
    }

    [Fact]
    public void Diff_CreatesCorrectCommand()
    {
        var command = SvnCommand.Diff(@"C:\repo\file.cs");
        var args = command.BuildArguments();
        Assert.Equal("diff", args[0]);
    }

    [Fact]
    public void Diff_WithRevisions_AddsRevisionRange()
    {
        var command = SvnCommand.Diff(@"C:\repo\file.cs", 10, 20);
        var args = command.BuildArguments();
        Assert.Contains("-r10:20", args);
    }

    [Fact]
    public void Log_CreatesCorrectCommand()
    {
        var command = SvnCommand.Log(@"C:\repo");
        var args = command.BuildArguments();
        Assert.Equal("log", args[0]);
    }

    [Fact]
    public void Log_WithLimit_AddsLimitFlag()
    {
        var command = SvnCommand.Log(@"C:\repo", limit: 50);
        var args = command.BuildArguments();
        Assert.Contains("-l50", args);
    }

    [Fact]
    public void Info_CreatesCorrectCommand()
    {
        var command = SvnCommand.Info(@"C:\repo");
        var args = command.BuildArguments();
        Assert.Equal("info", args[0]);
    }

    [Fact]
    public void Cleanup_CreatesCorrectCommand()
    {
        var command = SvnCommand.Cleanup(@"C:\repo");
        var args = command.BuildArguments();
        Assert.Equal("cleanup", args[0]);
    }

    [Fact]
    public void Cleanup_WithRemoveUnversioned_AddsFlag()
    {
        var command = SvnCommand.Cleanup(@"C:\repo", removeUnversioned: true);
        var args = command.BuildArguments();
        Assert.Contains("--remove-unversioned", args);
    }

    [Fact]
    public void UseXml_AddsXmlFlag()
    {
        var command = SvnCommand.Status(@"C:\repo");
        command.UseXml = true;
        var args = command.BuildArguments();
        Assert.Contains("--xml", args);
    }

    [Fact]
    public void Checkout_CreatesCorrectCommand()
    {
        var command = SvnCommand.Checkout("http://svn/repo", @"C:\local");
        var args = command.BuildArguments();
        Assert.Equal("checkout", args[0]);
        Assert.Contains("http://svn/repo", args);
        Assert.Contains(@"C:\local", args);
    }

    [Fact]
    public void Blame_CreatesCorrectCommand()
    {
        var command = SvnCommand.Blame(@"C:\repo\file.cs");
        var args = command.BuildArguments();
        Assert.Equal("blame", args[0]);
    }

    [Fact]
    public void List_CreatesCorrectCommand()
    {
        var command = SvnCommand.List("http://svn/repo");
        var args = command.BuildArguments();
        Assert.Equal("list", args[0]);
    }

    [Fact]
    public void Resolve_CreatesCorrectCommand()
    {
        var command = SvnCommand.Resolve(@"C:\repo\file.cs", "working");
        var args = command.BuildArguments();
        Assert.Equal("resolve", args[0]);
        Assert.Contains("--accept=working", args);
    }

    [Fact]
    public void Move_CreatesCorrectCommand()
    {
        var command = SvnCommand.Move(@"C:\repo\old.cs", @"C:\repo\new.cs");
        var args = command.BuildArguments();
        Assert.Equal("move", args[0]);
        Assert.Contains(@"C:\repo\old.cs", args);
        Assert.Contains(@"C:\repo\new.cs", args);
    }

    [Fact]
    public void Lock_CreatesCorrectCommand()
    {
        var command = SvnCommand.Lock(@"C:\repo\file.cs", "Lock message");
        var args = command.BuildArguments();
        Assert.Equal("lock", args[0]);
        Assert.Contains(@"C:\repo\file.cs", args);
        Assert.Contains("-m", args);
        Assert.Contains("Lock message", args);
    }

    [Fact]
    public void Lock_WithForce_AddsForceFlag()
    {
        var command = SvnCommand.Lock(@"C:\repo\file.cs", "Lock message", force: true);
        var args = command.BuildArguments();
        Assert.Contains("--force", args);
    }

    [Fact]
    public void Unlock_CreatesCorrectCommand()
    {
        var command = SvnCommand.Unlock(@"C:\repo\file.cs");
        var args = command.BuildArguments();
        Assert.Equal("unlock", args[0]);
        Assert.Contains(@"C:\repo\file.cs", args);
    }

    [Fact]
    public void Unlock_WithForce_AddsForceFlag()
    {
        var command = SvnCommand.Unlock(@"C:\repo\file.cs", force: true);
        var args = command.BuildArguments();
        Assert.Contains("--force", args);
    }

    [Fact]
    public void PropList_CreatesCorrectCommand()
    {
        var command = SvnCommand.PropList(@"C:\repo\file.cs");
        var args = command.BuildArguments();
        Assert.Equal("proplist", args[0]);
        Assert.Contains(@"C:\repo\file.cs", args);
    }

    [Fact]
    public void PropList_WithRecursive_AddsFlag()
    {
        var command = SvnCommand.PropList(@"C:\repo", recursive: true);
        var args = command.BuildArguments();
        Assert.Contains("-R", args);
    }

    [Fact]
    public void PropGet_CreatesCorrectCommand()
    {
        var command = SvnCommand.PropGet("svn:ignore", @"C:\repo");
        var args = command.BuildArguments();
        Assert.Equal("propget", args[0]);
        Assert.Contains("svn:ignore", args);
        Assert.Contains(@"C:\repo", args);
    }

    [Fact]
    public void PropSet_CreatesCorrectCommand()
    {
        var command = SvnCommand.PropSet("svn:eol-style", "native", @"C:\repo\file.cs");
        var args = command.BuildArguments();
        Assert.Equal("propset", args[0]);
        Assert.Contains("svn:eol-style", args);
        Assert.Contains("native", args);
        Assert.Contains(@"C:\repo\file.cs", args);
    }

    [Fact]
    public void PropDelete_CreatesCorrectCommand()
    {
        var command = SvnCommand.PropDelete("svn:ignore", @"C:\repo");
        var args = command.BuildArguments();
        Assert.Equal("propdel", args[0]);
        Assert.Contains("svn:ignore", args);
        Assert.Contains(@"C:\repo", args);
    }

    [Fact]
    public void Copy_CreatesCorrectCommand()
    {
        var command = SvnCommand.Copy("http://svn/trunk", "http://svn/branches/feature", "Create branch");
        var args = command.BuildArguments();
        Assert.Equal("copy", args[0]);
        Assert.Contains("http://svn/trunk", args);
        Assert.Contains("http://svn/branches/feature", args);
        Assert.Contains("-m", args);
        Assert.Contains("Create branch", args);
    }

    [Fact]
    public void Switch_CreatesCorrectCommand()
    {
        var command = SvnCommand.Switch(@"C:\repo", "http://svn/branches/feature");
        var args = command.BuildArguments();
        Assert.Equal("switch", args[0]);
        Assert.Contains(@"C:\repo", args);
        Assert.Contains("http://svn/branches/feature", args);
    }

    [Fact]
    public void Switch_WithRevision_AddsRevisionFlag()
    {
        var command = SvnCommand.Switch(@"C:\repo", "http://svn/branches/feature", revision: 100);
        var args = command.BuildArguments();
        Assert.Contains("-r100", args);
    }

    [Fact]
    public void Merge_CreatesCorrectCommand()
    {
        var command = SvnCommand.Merge("http://svn/branches/feature", 10, 20, @"C:\repo");
        var args = command.BuildArguments();
        Assert.Equal("merge", args[0]);
        Assert.Contains("http://svn/branches/feature", args);
        Assert.Contains("-r10:20", args);
        Assert.Contains(@"C:\repo", args);
    }

    [Fact]
    public void Merge_WithDryRun_AddsDryRunFlag()
    {
        var command = SvnCommand.Merge("http://svn/branches/feature", 10, 20, @"C:\repo", dryRun: true);
        var args = command.BuildArguments();
        Assert.Contains("--dry-run", args);
    }

    [Fact]
    public void Merge_WithAccept_AddsAcceptFlag()
    {
        var command = SvnCommand.Merge("http://svn/branches/feature", 10, 20, @"C:\repo", dryRun: false, accept: "postpone");
        var args = command.BuildArguments();
        Assert.Contains("--accept=postpone", args);
    }

    [Fact]
    public void Merge_WithAcceptMineConflict_AddsAcceptFlag()
    {
        var command = SvnCommand.Merge("http://svn/branches/feature", 10, 20, @"C:\repo", accept: "mine-conflict");
        var args = command.BuildArguments();
        Assert.Contains("--accept=mine-conflict", args);
    }

    [Fact]
    public void Merge_WithAcceptTheirsConflict_AddsAcceptFlag()
    {
        var command = SvnCommand.Merge("http://svn/branches/feature", 10, 20, @"C:\repo", accept: "theirs-conflict");
        var args = command.BuildArguments();
        Assert.Contains("--accept=theirs-conflict", args);
    }

    [Fact]
    public void Merge_WithAcceptMineFull_AddsAcceptFlag()
    {
        var command = SvnCommand.Merge("http://svn/branches/feature", 10, 20, @"C:\repo", accept: "mine-full");
        var args = command.BuildArguments();
        Assert.Contains("--accept=mine-full", args);
    }

    [Fact]
    public void Merge_WithAcceptTheirsFull_AddsAcceptFlag()
    {
        var command = SvnCommand.Merge("http://svn/branches/feature", 10, 20, @"C:\repo", accept: "theirs-full");
        var args = command.BuildArguments();
        Assert.Contains("--accept=theirs-full", args);
    }

    [Fact]
    public void Merge_WithDryRunAndAccept_AddsBothFlags()
    {
        var command = SvnCommand.Merge("http://svn/branches/feature", 10, 20, @"C:\repo", dryRun: true, accept: "postpone");
        var args = command.BuildArguments();
        Assert.Contains("--dry-run", args);
        Assert.Contains("--accept=postpone", args);
    }

    [Fact]
    public void Mkdir_CreatesCorrectCommand()
    {
        var command = SvnCommand.Mkdir(@"C:\repo\newfolder");
        var args = command.BuildArguments();
        Assert.Equal("mkdir", args[0]);
        Assert.Contains(@"C:\repo\newfolder", args);
    }

    [Fact]
    public void Mkdir_WithMessage_AddsMessageFlag()
    {
        var command = SvnCommand.Mkdir("http://svn/trunk/newfolder", "Create new folder");
        var args = command.BuildArguments();
        Assert.Equal("mkdir", args[0]);
        Assert.Contains("-m", args);
        Assert.Contains("Create new folder", args);
    }

    [Fact]
    public void Mkdir_WithParents_AddsParentsFlag()
    {
        var command = SvnCommand.Mkdir(@"C:\repo\new\nested\folder", parents: true);
        var args = command.BuildArguments();
        Assert.Contains("--parents", args);
    }

    [Fact]
    public void Cat_CreatesCorrectCommand()
    {
        var command = SvnCommand.Cat("http://svn/trunk/file.txt");
        var args = command.BuildArguments();
        Assert.Equal("cat", args[0]);
        Assert.Contains("http://svn/trunk/file.txt", args);
    }

    [Fact]
    public void Cat_WithRevision_AddsRevisionFlag()
    {
        var command = SvnCommand.Cat("http://svn/trunk/file.txt", revision: 123);
        var args = command.BuildArguments();
        Assert.Equal("cat", args[0]);
        Assert.Contains("-r123", args);
    }

    [Fact]
    public void Import_CreatesCorrectCommand()
    {
        var command = SvnCommand.Import(@"C:\local\folder", "http://svn/trunk", "Import files");
        var args = command.BuildArguments();
        Assert.Equal("import", args[0]);
        Assert.Contains(@"C:\local\folder", args);
        Assert.Contains("http://svn/trunk", args);
        Assert.Contains("-m", args);
        Assert.Contains("Import files", args);
    }

    [Fact]
    public void Import_WithNoIgnore_AddsNoIgnoreFlag()
    {
        var command = SvnCommand.Import(@"C:\local\folder", "http://svn/trunk", "Import files", noIgnore: true);
        var args = command.BuildArguments();
        Assert.Contains("--no-ignore", args);
    }

    [Fact]
    public void Relocate_CreatesCorrectCommand()
    {
        var command = SvnCommand.Relocate("http://old-svn/repo", "http://new-svn/repo");
        var args = command.BuildArguments();
        Assert.Equal("relocate", args[0]);
        Assert.Contains("http://old-svn/repo", args);
        Assert.Contains("http://new-svn/repo", args);
    }

    [Fact]
    public void Relocate_WithPath_AddsPath()
    {
        var command = SvnCommand.Relocate("http://old-svn/repo", "http://new-svn/repo", @"C:\repo");
        var args = command.BuildArguments();
        Assert.Contains(@"C:\repo", args);
    }

    [Fact]
    public void Export_CreatesCorrectCommand()
    {
        var command = SvnCommand.Export("http://svn/trunk", @"C:\export");
        var args = command.BuildArguments();
        Assert.Equal("export", args[0]);
        Assert.Contains("http://svn/trunk", args);
        Assert.Contains(@"C:\export", args);
    }

    [Fact]
    public void Export_WithRevision_AddsRevisionFlag()
    {
        var command = SvnCommand.Export("http://svn/trunk", @"C:\export", revision: 50);
        var args = command.BuildArguments();
        Assert.Contains("-r50", args);
    }

    [Fact]
    public void Export_WithForce_AddsForceFlag()
    {
        var command = SvnCommand.Export("http://svn/trunk", @"C:\export", force: true);
        var args = command.BuildArguments();
        Assert.Contains("--force", args);
    }
}
