using System;
using Stateless;

namespace BugPro
{
    public enum State
    {
        Open,
        InAnalysis,
        InProgress,
        MarkAsResolvedd,
        Closed,
        Reopened,
        Rejected,
        Deferred
    }

    public enum Trigger
    {
        StartAnalysis,
        AssignToTeam,
        MarkAsResolved,
        Confirm,
        ReStartAnalysis,
        Reject,
        Defer,
        Resume,
        Reopen
    }

    public class Bug
    {
        public const int MaxReopen = 3;

        private readonly StateMachine<State, Trigger> _machine;
        private int _reopenCount;

        public Bug()
        {
            _machine = new StateMachine<State, Trigger>(State.Open);

            _machine.Configure(State.Open)
                .Permit(Trigger.StartAnalysis, State.InAnalysis);

            _machine.Configure(State.InAnalysis)
                .Permit(Trigger.AssignToTeam, State.InProgress)
                .Permit(Trigger.Defer, State.Deferred)
                .Permit(Trigger.Reject, State.Rejected);

            _machine.Configure(State.InProgress)
                .Permit(Trigger.MarkAsResolved, State.MarkAsResolvedd)
                .Permit(Trigger.ReStartAnalysis, State.InAnalysis);

            _machine.Configure(State.MarkAsResolvedd)
                .Permit(Trigger.Confirm, State.Closed)
                .Permit(Trigger.ReStartAnalysis, State.InAnalysis);

            _machine.Configure(State.Deferred)
                .Permit(Trigger.Resume, State.InAnalysis);

            _machine.Configure(State.Closed)
                .PermitIf(Trigger.Reopen, State.Reopened, () => _reopenCount < MaxReopen);

            _machine.Configure(State.Rejected)
                .PermitIf(Trigger.Reopen, State.Reopened, () => _reopenCount < MaxReopen);

            _machine.Configure(State.Reopened)
                .OnEntry(() => _reopenCount++)
                .Permit(Trigger.StartAnalysis, State.InAnalysis);
        }

        public State CurrentState => _machine.State;

        public int ReopenCount => _reopenCount;

        public bool CanExecute(Trigger trigger) => _machine.CanExecute(trigger);

        public void StartAnalysis() => _machine.Fire(Trigger.StartAnalysis);
        public void AssignToTeam() => _machine.Fire(Trigger.AssignToTeam);
        public void MarkAsResolved() => _machine.Fire(Trigger.MarkAsResolved);
        public void Confirm() => _machine.Fire(Trigger.Confirm);
        public void ReStartAnalysis() => _machine.Fire(Trigger.ReStartAnalysis);
        public void Reject() => _machine.Fire(Trigger.Reject);
        public void Defer() => _machine.Fire(Trigger.Defer);
        public void Resume() => _machine.Fire(Trigger.Resume);
        public void Reopen() => _machine.Fire(Trigger.Reopen);
    }

    public static class Program
    {
        public static void Main()
        {
            var bug = new Bug();
            Console.WriteLine($"Start: {bug.CurrentState}");

            bug.StartAnalysis();
            Console.WriteLine($"After StartAnalysis: {bug.CurrentState}");

            bug.AssignToTeam();
            Console.WriteLine($"After AssignToTeam: {bug.CurrentState}");

            bug.MarkAsResolved();
            Console.WriteLine($"After MarkAsResolved: {bug.CurrentState}");

            bug.Confirm();
            Console.WriteLine($"After Confirm: {bug.CurrentState}");

            bug.Reopen();
            Console.WriteLine($"After Reopen: {bug.CurrentState} " +
                              $"(reopened {bug.ReopenCount} time(s))");

            bug.StartAnalysis();
            Console.WriteLine($"After StartAnalysis: {bug.CurrentState}");
        }
    }
}
