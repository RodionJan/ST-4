using System;
using BugPro;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BugTests
{
    [TestClass]
    public class BugWorkflowTests
    {
        private static Bug FreshBug() => new Bug();

        [TestMethod]
        public void NewBug_StartsInOpen()
        {
            var bug = FreshBug();
            Assert.AreEqual(State.Open, bug.CurrentState);
        }

        [TestMethod]
        public void NewBug_HasZeroReopens()
        {
            var bug = FreshBug();
            Assert.AreEqual(0, bug.ReopenCount);
        }

        [TestMethod]
        public void StartAnalysis_MovesOpenToAnalysis()
        {
            var bug = FreshBug();
            bug.StartAnalysis();
            Assert.AreEqual(State.InAnalysis, bug.CurrentState);
        }

        [TestMethod]
        public void AssignToTeam_MovesAnalysisToInProgress()
        {
            var bug = FreshBug();
            bug.StartAnalysis();
            bug.AssignToTeam();
            Assert.AreEqual(State.InProgress, bug.CurrentState);
        }

        [TestMethod]
        public void MarkAsResolved_MovesInProgressToMarkAsResolvedd()
        {
            var bug = FreshBug();
            bug.StartAnalysis();
            bug.AssignToTeam();
            bug.MarkAsResolved();
            Assert.AreEqual(State.MarkAsResolvedd, bug.CurrentState);
        }

        [TestMethod]
        public void Confirm_MovesMarkAsResolveddToClosed()
        {
            var bug = FreshBug();
            bug.StartAnalysis();
            bug.AssignToTeam();
            bug.MarkAsResolved();
            bug.Confirm();
            Assert.AreEqual(State.Closed, bug.CurrentState);
        }

        [TestMethod]
        public void Reject_MovesAnalysisToRejected()
        {
            var bug = FreshBug();
            bug.StartAnalysis();
            bug.Reject();
            Assert.AreEqual(State.Rejected, bug.CurrentState);
        }

        [TestMethod]
        public void Defer_MovesAnalysisToDeferred()
        {
            var bug = FreshBug();
            bug.StartAnalysis();
            bug.Defer();
            Assert.AreEqual(State.Deferred, bug.CurrentState);
        }

        [TestMethod]
        public void Resume_MovesDeferredBackToAnalysis()
        {
            var bug = FreshBug();
            bug.StartAnalysis();
            bug.Defer();
            bug.Resume();
            Assert.AreEqual(State.InAnalysis, bug.CurrentState);
        }

        [TestMethod]
        public void ReStartAnalysis_FromInProgress_ReturnsToAnalysis()
        {
            var bug = FreshBug();
            bug.StartAnalysis();
            bug.AssignToTeam();
            bug.ReStartAnalysis();
            Assert.AreEqual(State.InAnalysis, bug.CurrentState);
        }

        [TestMethod]
        public void ReStartAnalysis_FromMarkAsResolvedd_ReturnsToAnalysis()
        {
            var bug = FreshBug();
            bug.StartAnalysis();
            bug.AssignToTeam();
            bug.MarkAsResolved();
            bug.ReStartAnalysis();
            Assert.AreEqual(State.InAnalysis, bug.CurrentState);
        }

        [TestMethod]
        public void Reopen_FromClosed_MovesToReopened()
        {
            var bug = FreshBug();
            bug.StartAnalysis();
            bug.AssignToTeam();
            bug.MarkAsResolved();
            bug.Confirm();
            bug.Reopen();
            Assert.AreEqual(State.Reopened, bug.CurrentState);
        }

        [TestMethod]
        public void Reopen_FromRejected_MovesToReopened()
        {
            var bug = FreshBug();
            bug.StartAnalysis();
            bug.Reject();
            bug.Reopen();
            Assert.AreEqual(State.Reopened, bug.CurrentState);
        }

        [TestMethod]
        public void Reopen_IncrementsReopenCount()
        {
            var bug = FreshBug();
            bug.StartAnalysis();
            bug.Reject();
            bug.Reopen();
            Assert.AreEqual(1, bug.ReopenCount);
        }

        [TestMethod]
        public void Reopened_StartAnalysis_ReturnsToAnalysis()
        {
            var bug = FreshBug();
            bug.StartAnalysis();
            bug.Reject();
            bug.Reopen();
            bug.StartAnalysis();
            Assert.AreEqual(State.InAnalysis, bug.CurrentState);
        }

        [TestMethod]
        public void CanExecute_StartAnalysis_TrueInOpen()
        {
            var bug = FreshBug();
            Assert.IsTrue(bug.CanExecute(Trigger.StartAnalysis));
        }

        [TestMethod]
        public void CanExecute_AssignToTeam_FalseInOpen()
        {
            var bug = FreshBug();
            Assert.IsFalse(bug.CanExecute(Trigger.AssignToTeam));
        }

        [TestMethod]
        public void AssignToTeam_FromOpen_Throws()
        {
            var bug = FreshBug();
            Assert.ThrowsException<InvalidOperationException>(() => bug.AssignToTeam());
        }

        [TestMethod]
        public void Confirm_FromOpen_Throws()
        {
            var bug = FreshBug();
            Assert.ThrowsException<InvalidOperationException>(() => bug.Confirm());
        }

        [TestMethod]
        public void MarkAsResolved_FromAnalysis_Throws()
        {
            var bug = FreshBug();
            bug.StartAnalysis();
            Assert.ThrowsException<InvalidOperationException>(() => bug.MarkAsResolved());
        }

        [TestMethod]
        public void Reopen_FromOpen_Throws()
        {
            var bug = FreshBug();
            Assert.ThrowsException<InvalidOperationException>(() => bug.Reopen());
        }

        [TestMethod]
        public void Resume_FromClosed_Throws()
        {
            var bug = FreshBug();
            bug.StartAnalysis();
            bug.AssignToTeam();
            bug.MarkAsResolved();
            bug.Confirm();
            Assert.ThrowsException<InvalidOperationException>(() => bug.Resume());
        }

        [TestMethod]
        public void Reopen_BeyondLimit_Throws()
        {
            var bug = FreshBug();
            bug.StartAnalysis();
            bug.Reject();

            for (int i = 0; i < Bug.MaxReopen; i++)
            {
                bug.Reopen();
                bug.StartAnalysis();
                bug.Reject();
            }

            Assert.AreEqual(Bug.MaxReopen, bug.ReopenCount);
            Assert.IsFalse(bug.CanExecute(Trigger.Reopen));
            Assert.ThrowsException<InvalidOperationException>(() => bug.Reopen());
        }

        [TestMethod]
        public void DoubleStartAnalysis_Throws()
        {
            var bug = FreshBug();
            bug.StartAnalysis();
            Assert.ThrowsException<InvalidOperationException>(() => bug.StartAnalysis());
        }
    }
}
