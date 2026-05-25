using Stateless;

namespace BugPro;

public sealed class Bug
{
    public enum State
    {
        New,
        Triaged,
        Fixing,
        NeedMoreInfo,
        NotABug,
        WontFix,
        Duplicate,
        CannotReproduce,
        Closed
    }

    public enum Trigger
    {
        StartAnalysis,
        AcceptForFix,
        RequestMoreInfo,
        ProvideMoreInfo,
        ResolveAsNotABug,
        ResolveAsWontFix,
        ResolveAsDuplicate,
        ResolveAsCannotReproduce,
        ConfirmByTester,
        ReturnToTriage,
        CompleteFix,
        Reopen
    }

    private readonly StateMachine<State, Trigger> _stateMachine;

    public Bug()
    {
        _stateMachine = new StateMachine<State, Trigger>(State.New);

        _stateMachine.Configure(State.New)
            .Permit(Trigger.StartAnalysis, State.Triaged);

        _stateMachine.Configure(State.Triaged)
            .Permit(Trigger.AcceptForFix, State.Fixing)
            .Permit(Trigger.RequestMoreInfo, State.NeedMoreInfo)
            .Permit(Trigger.ResolveAsNotABug, State.NotABug)
            .Permit(Trigger.ResolveAsWontFix, State.WontFix)
            .Permit(Trigger.ResolveAsDuplicate, State.Duplicate)
            .Permit(Trigger.ResolveAsCannotReproduce, State.CannotReproduce);

        _stateMachine.Configure(State.Fixing)
            .Permit(Trigger.RequestMoreInfo, State.NeedMoreInfo)
            .Permit(Trigger.CompleteFix, State.Closed)
            .Permit(Trigger.ReturnToTriage, State.Triaged);

        _stateMachine.Configure(State.NeedMoreInfo)
            .Permit(Trigger.ProvideMoreInfo, State.Fixing);

        _stateMachine.Configure(State.NotABug)
            .Permit(Trigger.ConfirmByTester, State.Closed)
            .Permit(Trigger.ReturnToTriage, State.Triaged);

        _stateMachine.Configure(State.WontFix)
            .Permit(Trigger.ConfirmByTester, State.Closed)
            .Permit(Trigger.ReturnToTriage, State.Triaged);

        _stateMachine.Configure(State.Duplicate)
            .Permit(Trigger.ConfirmByTester, State.Closed)
            .Permit(Trigger.ReturnToTriage, State.Triaged);

        _stateMachine.Configure(State.CannotReproduce)
            .Permit(Trigger.ConfirmByTester, State.Closed)
            .Permit(Trigger.ReturnToTriage, State.Triaged);

        _stateMachine.Configure(State.Closed)
            .Permit(Trigger.Reopen, State.Triaged);
    }

    public State CurrentState => _stateMachine.State;

    public void Fire(Trigger trigger) => _stateMachine.Fire(trigger);

    public IReadOnlyCollection<Trigger> GetPermittedTriggers() => _stateMachine.PermittedTriggers.ToArray();
}

internal static class Program
{
    private static void Main()
    {
        var bug = new Bug();

        Console.WriteLine("Bug workflow demo");
        PrintState(bug, "Создан новый дефект");

        bug.Fire(Bug.Trigger.StartAnalysis);
        PrintState(bug, "Продуктовая команда взяла дефект в разбор");

        bug.Fire(Bug.Trigger.AcceptForFix);
        PrintState(bug, "Разработчик начал исправление");

        bug.Fire(Bug.Trigger.CompleteFix);
        PrintState(bug, "Исправление завершено");

        bug.Fire(Bug.Trigger.Reopen);
        PrintState(bug, "Тестировщик переоткрыл дефект");

        Console.WriteLine("Доступные переходы: " + string.Join(", ", bug.GetPermittedTriggers()));
    }

    private static void PrintState(Bug bug, string action)
    {
        Console.WriteLine($"{action}: {bug.CurrentState}");
    }
}
