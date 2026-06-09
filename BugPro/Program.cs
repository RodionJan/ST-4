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

        private readonly StateMachine<State, Trigger> _stateMachine;
        private int _reopenCounter;

        public Bug()
        {
            _stateMachine = new StateMachine<State, Trigger>(State.Open);

            _stateMachine.Configure(State.Open)
                .Permit(Trigger.StartAnalysis, State.InAnalysis);

            _stateMachine.Configure(State.InAnalysis)
                .Permit(Trigger.AssignToTeam, State.InProgress)
                .Permit(Trigger.Defer, State.Deferred)
                .Permit(Trigger.Reject, State.Rejected);

            _stateMachine.Configure(State.InProgress)
                .Permit(Trigger.MarkAsResolved, State.MarkAsResolvedd)
                .Permit(Trigger.ReStartAnalysis, State.InAnalysis);

            _stateMachine.Configure(State.MarkAsResolvedd)
                .Permit(Trigger.Confirm, State.Closed)
                .Permit(Trigger.ReStartAnalysis, State.InAnalysis);

            _stateMachine.Configure(State.Deferred)
                .Permit(Trigger.Resume, State.InAnalysis);

            _stateMachine.Configure(State.Closed)
                .PermitIf(Trigger.Reopen, State.Reopened, () => _reopenCounter < MaxReopen);

            _stateMachine.Configure(State.Rejected)
                .PermitIf(Trigger.Reopen, State.Reopened, () => _reopenCounter < MaxReopen);

            _stateMachine.Configure(State.Reopened)
                .OnEntry(() => _reopenCounter++)
                .Permit(Trigger.StartAnalysis, State.InAnalysis);
        }

        public State CurrentState => _stateMachine.State;

        public int ReopenCount => _reopenCounter;

        public bool CanExecute(Trigger trigger) => _stateMachine.CanExecute(trigger);

        public void StartAnalysis() => _stateMachine.Fire(Trigger.StartAnalysis);
        public void AssignToTeam() => _stateMachine.Fire(Trigger.AssignToTeam);
        public void MarkAsResolved() => _stateMachine.Fire(Trigger.MarkAsResolved);
        public void Confirm() => _stateMachine.Fire(Trigger.Confirm);
        public void ReStartAnalysis() => _stateMachine.Fire(Trigger.ReStartAnalysis);
        public void Reject() => _stateMachine.Fire(Trigger.Reject);
        public void Defer() => _stateMachine.Fire(Trigger.Defer);
        public void Resume() => _stateMachine.Fire(Trigger.Resume);
        public void Reopen() => _stateMachine.Fire(Trigger.Reopen);
    }

    public static class Program
    {
        public static void Main()
        {
            var bugInstance = new Bug();
            Console.WriteLine($"Start: {bugInstance.CurrentState}");

            bugInstance.StartAnalysis();
            Console.WriteLine($"After StartAnalysis: {bugInstance.CurrentState}");

            bugInstance.AssignToTeam();
            Console.WriteLine($"After AssignToTeam: {bugInstance.CurrentState}");

            bugInstance.MarkAsResolved();
            Console.WriteLine($"After MarkAsResolved: {bugInstance.CurrentState}");

            bugInstance.Confirm();
            Console.WriteLine($"After Confirm: {bugInstance.CurrentState}");

            bugInstance.Reopen();
            Console.WriteLine($"After Reopen: {bugInstance.CurrentState} " +
                              $"(reopened {bugInstance.ReopenCount} time(s))");

            bugInstance.StartAnalysis();
            Console.WriteLine($"After StartAnalysis: {bugInstance.CurrentState}");
        }
    }
}