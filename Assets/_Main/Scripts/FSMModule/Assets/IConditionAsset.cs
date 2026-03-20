using System;

namespace AIModule
{
    public interface IConditionAsset<in TContext>
    {
        Func<bool> Create(TContext context);
    }
}