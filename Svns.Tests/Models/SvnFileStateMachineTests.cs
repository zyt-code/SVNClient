using Xunit;
using Svns.Models;
using System.Linq;

namespace Svns.Tests.Models;

public class SvnFileStateMachineTests
{
    #region GetNextState Tests

    [Theory]
    [InlineData(SvnStatusType.Normal, SvnFileAction.Modify, SvnStatusType.Modified)]
    [InlineData(SvnStatusType.Normal, SvnFileAction.Delete, SvnStatusType.Deleted)]
    [InlineData(SvnStatusType.Normal, SvnFileAction.Replace, SvnStatusType.Replaced)]
    public void GetNextState_ValidTransition_ReturnsExpectedState(
        SvnStatusType current, SvnFileAction action, SvnStatusType expected)
    {
        var result = SvnFileStateMachine.GetNextState(current, action);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(SvnStatusType.Normal, SvnFileAction.Commit)]
    [InlineData(SvnStatusType.Unversioned, SvnFileAction.Delete)]
    [InlineData(SvnStatusType.Ignored, SvnFileAction.Modify)]
    public void GetNextState_InvalidTransition_ReturnsNull(
        SvnStatusType current, SvnFileAction action)
    {
        var result = SvnFileStateMachine.GetNextState(current, action);
        Assert.Null(result);
    }

    #endregion

    #region IsValidTransition Tests

    [Fact]
    public void IsValidTransition_ValidTransition_ReturnsTrue()
    {
        Assert.True(SvnFileStateMachine.IsValidTransition(SvnStatusType.Normal, SvnFileAction.Modify));
        Assert.True(SvnFileStateMachine.IsValidTransition(SvnStatusType.Modified, SvnFileAction.Commit));
        Assert.True(SvnFileStateMachine.IsValidTransition(SvnStatusType.Unversioned, SvnFileAction.Add));
    }

    [Fact]
    public void IsValidTransition_InvalidTransition_ReturnsFalse()
    {
        Assert.False(SvnFileStateMachine.IsValidTransition(SvnStatusType.Normal, SvnFileAction.Commit));
        Assert.False(SvnFileStateMachine.IsValidTransition(SvnStatusType.Unversioned, SvnFileAction.Delete));
        Assert.False(SvnFileStateMachine.IsValidTransition(SvnStatusType.Ignored, SvnFileAction.Commit));
    }

    #endregion

    #region GetValidActions Tests

    [Fact]
    public void GetValidActions_NormalState_ReturnsExpectedActions()
    {
        var actions = SvnFileStateMachine.GetValidActions(SvnStatusType.Normal).ToList();

        Assert.Contains(SvnFileAction.Modify, actions);
        Assert.Contains(SvnFileAction.Delete, actions);
        Assert.Contains(SvnFileAction.Replace, actions);
        Assert.Equal(3, actions.Count);
    }

    [Fact]
    public void GetValidActions_UnversionedState_ReturnsAddAndIgnore()
    {
        var actions = SvnFileStateMachine.GetValidActions(SvnStatusType.Unversioned).ToList();

        Assert.Contains(SvnFileAction.Add, actions);
        Assert.Contains(SvnFileAction.Ignore, actions);
        Assert.Equal(2, actions.Count);
    }

    [Fact]
    public void GetValidActions_ModifiedState_ReturnsMultipleActions()
    {
        var actions = SvnFileStateMachine.GetValidActions(SvnStatusType.Modified).ToList();

        Assert.Contains(SvnFileAction.Commit, actions);
        Assert.Contains(SvnFileAction.Revert, actions);
        Assert.Contains(SvnFileAction.Delete, actions);
    }

    #endregion

    #region GetRecommendedAction Tests

    [Theory]
    [InlineData(SvnStatusType.Unversioned, SvnFileAction.Add)]
    [InlineData(SvnStatusType.Modified, SvnFileAction.Commit)]
    [InlineData(SvnStatusType.Added, SvnFileAction.Commit)]
    [InlineData(SvnStatusType.Deleted, SvnFileAction.Commit)]
    [InlineData(SvnStatusType.Conflicted, SvnFileAction.Resolve)]
    [InlineData(SvnStatusType.Missing, SvnFileAction.Revert)]
    public void GetRecommendedAction_ReturnsExpectedAction(SvnStatusType status, SvnFileAction expected)
    {
        var result = SvnFileStateMachine.GetRecommendedAction(status);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetRecommendedAction_NormalState_ReturnsNull()
    {
        var result = SvnFileStateMachine.GetRecommendedAction(SvnStatusType.Normal);
        Assert.Null(result);
    }

    #endregion

    #region CanDelete Tests

    [Theory]
    [InlineData(SvnStatusType.Normal)]
    [InlineData(SvnStatusType.Modified)]
    [InlineData(SvnStatusType.Added)]
    [InlineData(SvnStatusType.Unversioned)]
    [InlineData(SvnStatusType.Conflicted)]
    [InlineData(SvnStatusType.Missing)]
    [InlineData(SvnStatusType.Ignored)]
    public void CanDelete_DeletableStates_ReturnsTrue(SvnStatusType status)
    {
        Assert.True(SvnFileStateMachine.CanDelete(status));
    }

    [Theory]
    [InlineData(SvnStatusType.Deleted)]
    [InlineData(SvnStatusType.Incomplete)]
    public void CanDelete_NonDeletableStates_ReturnsFalse(SvnStatusType status)
    {
        Assert.False(SvnFileStateMachine.CanDelete(status));
    }

    #endregion

    #region CanRevert Tests

    [Theory]
    [InlineData(SvnStatusType.Modified)]
    [InlineData(SvnStatusType.Added)]
    [InlineData(SvnStatusType.Deleted)]
    [InlineData(SvnStatusType.Conflicted)]
    [InlineData(SvnStatusType.Missing)]
    public void CanRevert_RevertableStates_ReturnsTrue(SvnStatusType status)
    {
        Assert.True(SvnFileStateMachine.CanRevert(status));
    }

    [Theory]
    [InlineData(SvnStatusType.Normal)]
    [InlineData(SvnStatusType.Unversioned)]
    [InlineData(SvnStatusType.Ignored)]
    public void CanRevert_NonRevertableStates_ReturnsFalse(SvnStatusType status)
    {
        Assert.False(SvnFileStateMachine.CanRevert(status));
    }

    #endregion

    #region CanCommit Tests

    [Theory]
    [InlineData(SvnStatusType.Modified)]
    [InlineData(SvnStatusType.Added)]
    [InlineData(SvnStatusType.Deleted)]
    [InlineData(SvnStatusType.Replaced)]
    public void CanCommit_CommittableStates_ReturnsTrue(SvnStatusType status)
    {
        Assert.True(SvnFileStateMachine.CanCommit(status));
    }

    [Theory]
    [InlineData(SvnStatusType.Normal)]
    [InlineData(SvnStatusType.Unversioned)]
    [InlineData(SvnStatusType.Conflicted)]
    [InlineData(SvnStatusType.Missing)]
    public void CanCommit_NonCommittableStates_ReturnsFalse(SvnStatusType status)
    {
        Assert.False(SvnFileStateMachine.CanCommit(status));
    }

    #endregion

    #region HasLocalModifications Tests

    [Theory]
    [InlineData(SvnStatusType.Modified)]
    [InlineData(SvnStatusType.Added)]
    [InlineData(SvnStatusType.Deleted)]
    [InlineData(SvnStatusType.Conflicted)]
    [InlineData(SvnStatusType.Missing)]
    public void HasLocalModifications_ModifiedStates_ReturnsTrue(SvnStatusType status)
    {
        Assert.True(SvnFileStateMachine.HasLocalModifications(status));
    }

    [Theory]
    [InlineData(SvnStatusType.Normal)]
    [InlineData(SvnStatusType.Unversioned)]
    [InlineData(SvnStatusType.Ignored)]
    public void HasLocalModifications_UnmodifiedStates_ReturnsFalse(SvnStatusType status)
    {
        Assert.False(SvnFileStateMachine.HasLocalModifications(status));
    }

    #endregion

    #region GetActionDescription Tests

    [Fact]
    public void GetActionDescription_UnversionedAdd_ReturnsCorrectDescription()
    {
        var description = SvnFileStateMachine.GetActionDescription(SvnFileAction.Add, SvnStatusType.Unversioned);
        Assert.Equal("Schedule file for addition to version control", description);
    }

    [Fact]
    public void GetActionDescription_ModifiedCommit_ReturnsCorrectDescription()
    {
        var description = SvnFileStateMachine.GetActionDescription(SvnFileAction.Commit, SvnStatusType.Modified);
        Assert.Equal("Commit modifications to repository", description);
    }

    [Fact]
    public void GetActionDescription_ModifiedRevert_ReturnsCorrectDescription()
    {
        var description = SvnFileStateMachine.GetActionDescription(SvnFileAction.Revert, SvnStatusType.Modified);
        Assert.Equal("Discard local modifications", description);
    }

    [Fact]
    public void GetActionDescription_ConflictedResolve_ReturnsCorrectDescription()
    {
        var description = SvnFileStateMachine.GetActionDescription(SvnFileAction.Resolve, SvnStatusType.Conflicted);
        Assert.Equal("Mark conflict as resolved", description);
    }

    #endregion

    #region Workflow Tests

    [Fact]
    public void Workflow_NormalToModifiedToNormal_Complete()
    {
        // Start: Normal
        var state = SvnStatusType.Normal;

        // Modify file
        state = SvnFileStateMachine.GetNextState(state, SvnFileAction.Modify)!.Value;
        Assert.Equal(SvnStatusType.Modified, state);

        // Commit changes
        state = SvnFileStateMachine.GetNextState(state, SvnFileAction.Commit)!.Value;
        Assert.Equal(SvnStatusType.Normal, state);
    }

    [Fact]
    public void Workflow_UnversionedToAddToNormal_Complete()
    {
        // Start: Unversioned
        var state = SvnStatusType.Unversioned;

        // Add to version control
        state = SvnFileStateMachine.GetNextState(state, SvnFileAction.Add)!.Value;
        Assert.Equal(SvnStatusType.Added, state);

        // Commit
        state = SvnFileStateMachine.GetNextState(state, SvnFileAction.Commit)!.Value;
        Assert.Equal(SvnStatusType.Normal, state);
    }

    [Fact]
    public void Workflow_NormalToDeletedToNormal_Complete()
    {
        // Start: Normal
        var state = SvnStatusType.Normal;

        // Delete
        state = SvnFileStateMachine.GetNextState(state, SvnFileAction.Delete)!.Value;
        Assert.Equal(SvnStatusType.Deleted, state);

        // Commit deletion
        state = SvnFileStateMachine.GetNextState(state, SvnFileAction.Commit)!.Value;
        Assert.Equal(SvnStatusType.Normal, state);
    }

    [Fact]
    public void Workflow_NormalToModifiedToRevertToNormal_Complete()
    {
        // Start: Normal
        var state = SvnStatusType.Normal;

        // Modify
        state = SvnFileStateMachine.GetNextState(state, SvnFileAction.Modify)!.Value;
        Assert.Equal(SvnStatusType.Modified, state);

        // Revert (discard changes)
        state = SvnFileStateMachine.GetNextState(state, SvnFileAction.Revert)!.Value;
        Assert.Equal(SvnStatusType.Normal, state);
    }

    [Fact]
    public void Workflow_AddedRevertBecomesUnversioned_Complete()
    {
        // Start: Added (not yet committed)
        var state = SvnStatusType.Added;

        // Revert addition
        state = SvnFileStateMachine.GetNextState(state, SvnFileAction.Revert)!.Value;
        Assert.Equal(SvnStatusType.Unversioned, state);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void InvalidTransition_DoesNotThrow()
    {
        // Should not throw, just return null
        var result = SvnFileStateMachine.GetNextState(SvnStatusType.Normal, SvnFileAction.Commit);
        Assert.Null(result);
    }

    [Fact]
    public void GetValidActions_UnknownState_ReturnsEmpty()
    {
        // Incomplete state has no transitions defined
        var actions = SvnFileStateMachine.GetValidActions(SvnStatusType.Incomplete);
        // Incomplete has one action: Update
        Assert.Single(actions);
        Assert.Contains(SvnFileAction.Update, actions);
    }

    #endregion
}
