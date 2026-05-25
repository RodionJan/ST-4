using BugPro;

namespace BugTests;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void NewBug_HasNewState()
    {
        var bug = new Bug();

        Assert.AreEqual(Bug.State.New, bug.CurrentState);
    }

    [TestMethod]
    public void StartAnalysis_FromNew_MovesToTriaged()
    {
        var bug = new Bug();

        bug.Fire(Bug.Trigger.StartAnalysis);

        Assert.AreEqual(Bug.State.Triaged, bug.CurrentState);
    }

    [TestMethod]
    public void AcceptForFix_FromTriaged_MovesToFixing()
    {
        var bug = CreateTriagedBug();

        bug.Fire(Bug.Trigger.AcceptForFix);

        Assert.AreEqual(Bug.State.Fixing, bug.CurrentState);
    }

    [TestMethod]
    public void RequestMoreInfo_FromTriaged_MovesToNeedMoreInfo()
    {
        var bug = CreateTriagedBug();

        bug.Fire(Bug.Trigger.RequestMoreInfo);

        Assert.AreEqual(Bug.State.NeedMoreInfo, bug.CurrentState);
    }

    [TestMethod]
    public void ProvideMoreInfo_FromNeedMoreInfo_MovesToFixing()
    {
        var bug = CreateNeedMoreInfoBug();

        bug.Fire(Bug.Trigger.ProvideMoreInfo);

        Assert.AreEqual(Bug.State.Fixing, bug.CurrentState);
    }

    [TestMethod]
    public void RequestMoreInfo_FromFixing_MovesToNeedMoreInfo()
    {
        var bug = CreateFixingBug();

        bug.Fire(Bug.Trigger.RequestMoreInfo);

        Assert.AreEqual(Bug.State.NeedMoreInfo, bug.CurrentState);
    }

    [TestMethod]
    public void ResolveAsNotABug_FromTriaged_MovesToNotABug()
    {
        var bug = CreateTriagedBug();

        bug.Fire(Bug.Trigger.ResolveAsNotABug);

        Assert.AreEqual(Bug.State.NotABug, bug.CurrentState);
    }

    [TestMethod]
    public void ResolveAsWontFix_FromTriaged_MovesToWontFix()
    {
        var bug = CreateTriagedBug();

        bug.Fire(Bug.Trigger.ResolveAsWontFix);

        Assert.AreEqual(Bug.State.WontFix, bug.CurrentState);
    }

    [TestMethod]
    public void ResolveAsDuplicate_FromTriaged_MovesToDuplicate()
    {
        var bug = CreateTriagedBug();

        bug.Fire(Bug.Trigger.ResolveAsDuplicate);

        Assert.AreEqual(Bug.State.Duplicate, bug.CurrentState);
    }

    [TestMethod]
    public void ResolveAsCannotReproduce_FromTriaged_MovesToCannotReproduce()
    {
        var bug = CreateTriagedBug();

        bug.Fire(Bug.Trigger.ResolveAsCannotReproduce);

        Assert.AreEqual(Bug.State.CannotReproduce, bug.CurrentState);
    }

    [TestMethod]
    public void ConfirmByTester_FromNotABug_MovesToClosed()
    {
        var bug = CreateResolvedBug(Bug.Trigger.ResolveAsNotABug);

        bug.Fire(Bug.Trigger.ConfirmByTester);

        Assert.AreEqual(Bug.State.Closed, bug.CurrentState);
    }

    [TestMethod]
    public void ConfirmByTester_FromWontFix_MovesToClosed()
    {
        var bug = CreateResolvedBug(Bug.Trigger.ResolveAsWontFix);

        bug.Fire(Bug.Trigger.ConfirmByTester);

        Assert.AreEqual(Bug.State.Closed, bug.CurrentState);
    }

    [TestMethod]
    public void ConfirmByTester_FromDuplicate_MovesToClosed()
    {
        var bug = CreateResolvedBug(Bug.Trigger.ResolveAsDuplicate);

        bug.Fire(Bug.Trigger.ConfirmByTester);

        Assert.AreEqual(Bug.State.Closed, bug.CurrentState);
    }

    [TestMethod]
    public void ConfirmByTester_FromCannotReproduce_MovesToClosed()
    {
        var bug = CreateResolvedBug(Bug.Trigger.ResolveAsCannotReproduce);

        bug.Fire(Bug.Trigger.ConfirmByTester);

        Assert.AreEqual(Bug.State.Closed, bug.CurrentState);
    }

    [TestMethod]
    public void ReturnToTriage_FromCannotReproduce_MovesToTriaged()
    {
        var bug = CreateResolvedBug(Bug.Trigger.ResolveAsCannotReproduce);

        bug.Fire(Bug.Trigger.ReturnToTriage);

        Assert.AreEqual(Bug.State.Triaged, bug.CurrentState);
    }

    [TestMethod]
    public void ReturnToTriage_FromNotABug_MovesToTriaged()
    {
        var bug = CreateResolvedBug(Bug.Trigger.ResolveAsNotABug);

        bug.Fire(Bug.Trigger.ReturnToTriage);

        Assert.AreEqual(Bug.State.Triaged, bug.CurrentState);
    }

    [TestMethod]
    public void CompleteFix_FromFixing_MovesToClosed()
    {
        var bug = CreateFixingBug();

        bug.Fire(Bug.Trigger.CompleteFix);

        Assert.AreEqual(Bug.State.Closed, bug.CurrentState);
    }

    [TestMethod]
    public void ReturnToTriage_FromFixing_MovesToTriaged()
    {
        var bug = CreateFixingBug();

        bug.Fire(Bug.Trigger.ReturnToTriage);

        Assert.AreEqual(Bug.State.Triaged, bug.CurrentState);
    }

    [TestMethod]
    public void Reopen_FromClosed_MovesToTriaged()
    {
        var bug = CreateClosedBugThroughFix();

        bug.Fire(Bug.Trigger.Reopen);

        Assert.AreEqual(Bug.State.Triaged, bug.CurrentState);
    }

    [TestMethod]
    public void GetPermittedTriggers_FromTriaged_ReturnsExpectedTriggers()
    {
        var bug = CreateTriagedBug();

        var triggers = bug.GetPermittedTriggers();

        CollectionAssert.AreEquivalent(
            new[]
            {
                Bug.Trigger.AcceptForFix,
                Bug.Trigger.RequestMoreInfo,
                Bug.Trigger.ResolveAsNotABug,
                Bug.Trigger.ResolveAsWontFix,
                Bug.Trigger.ResolveAsDuplicate,
                Bug.Trigger.ResolveAsCannotReproduce
            },
            triggers.ToArray());
    }

    [TestMethod]
    public void AcceptForFix_FromNew_ThrowsInvalidOperationException()
    {
        var bug = new Bug();

        Assert.ThrowsException<InvalidOperationException>(() => bug.Fire(Bug.Trigger.AcceptForFix));
    }

    [TestMethod]
    public void ConfirmByTester_FromFixing_ThrowsInvalidOperationException()
    {
        var bug = CreateFixingBug();

        Assert.ThrowsException<InvalidOperationException>(() => bug.Fire(Bug.Trigger.ConfirmByTester));
    }

    [TestMethod]
    public void ProvideMoreInfo_FromTriaged_ThrowsInvalidOperationException()
    {
        var bug = CreateTriagedBug();

        Assert.ThrowsException<InvalidOperationException>(() => bug.Fire(Bug.Trigger.ProvideMoreInfo));
    }

    [TestMethod]
    public void Reopen_FromTriaged_ThrowsInvalidOperationException()
    {
        var bug = CreateTriagedBug();

        Assert.ThrowsException<InvalidOperationException>(() => bug.Fire(Bug.Trigger.Reopen));
    }

    [TestMethod]
    public void CompleteFix_FromClosed_ThrowsInvalidOperationException()
    {
        var bug = CreateClosedBugThroughFix();

        Assert.ThrowsException<InvalidOperationException>(() => bug.Fire(Bug.Trigger.CompleteFix));
    }

    private static Bug CreateTriagedBug()
    {
        var bug = new Bug();
        bug.Fire(Bug.Trigger.StartAnalysis);
        return bug;
    }

    private static Bug CreateFixingBug()
    {
        var bug = CreateTriagedBug();
        bug.Fire(Bug.Trigger.AcceptForFix);
        return bug;
    }

    private static Bug CreateNeedMoreInfoBug()
    {
        var bug = CreateTriagedBug();
        bug.Fire(Bug.Trigger.RequestMoreInfo);
        return bug;
    }

    private static Bug CreateResolvedBug(Bug.Trigger resolutionTrigger)
    {
        var bug = CreateTriagedBug();
        bug.Fire(resolutionTrigger);
        return bug;
    }

    private static Bug CreateClosedBugThroughFix()
    {
        var bug = CreateFixingBug();
        bug.Fire(Bug.Trigger.CompleteFix);
        return bug;
    }
}
