namespace FSMModule
{
    public interface IStateAsset<in TContext>
    {
        IState Create(TContext context);
    }
}