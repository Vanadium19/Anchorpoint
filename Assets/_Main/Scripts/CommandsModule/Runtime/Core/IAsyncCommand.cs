using Cysharp.Threading.Tasks;

namespace CommandsModule
{
    public interface IAsyncCommand
    {
        UniTask<TaskResult> Execute();
    }

    public interface IAsyncCommand<TArgument>
    {
        UniTask<TaskResult> Execute(TArgument argument1);
    }

    public interface IAsyncCommand<TArgument1, TArgument2>
    {
        UniTask<TaskResult> Execute(TArgument1 argument1, TArgument2 argument2);
    }

    public interface IAsyncCommand<TArgument1, TArgument2, TArgument3>
    {
        UniTask<TaskResult> Execute(TArgument1 argument1, TArgument2 argument2, TArgument3 argument3);
    }

    public interface IAsyncCommand<TArgument1, TArgument2, TArgument3, TArgument4>
    {
        UniTask<TaskResult> Execute(TArgument1 argument1, TArgument2 argument2, TArgument3 argument3, TArgument4 argument4);
    }

    public interface IAsyncCommand<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5>
    {
        UniTask<TaskResult> Execute(TArgument1 argument1, TArgument2 argument2, TArgument3 argument3, TArgument4 argument4, TArgument5 argument5);
    }
}