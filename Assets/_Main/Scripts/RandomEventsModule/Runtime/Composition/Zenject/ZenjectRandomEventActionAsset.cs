using System;
using System.Collections.Generic;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>
    /// Base class for an action asset that resolves its runtime action through Zenject.
    /// A concrete asset only needs to expose its own inspector fields and, if it depends on
    /// values that are not injectable, override <see cref="GetArguments"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="GetArguments"/> receives the container, so an asset can build nested runtime
    /// objects — a <see cref="RandomEventActionPlan"/> for a branch, a
    /// <see cref="RandomEventConditionGroup"/> for a check — and pass them as constructor arguments.
    /// Zenject matches arguments to constructor parameters by type, which a bare <c>null</c> cannot
    /// carry: wrap an argument that may be empty in <see cref="Optional{TArgument}"/> so it keeps
    /// the type of the field it came from.
    /// </remarks>
    [Serializable]
    public abstract class ZenjectRandomEventActionAsset<TAction> : IRandomEventActionAsset
        where TAction : IRandomEventAction
    {
        /// <inheritdoc/>
        public virtual IRandomEventAction Create(DiContainer container) =>
            (TAction)container.InstantiateExplicit(typeof(TAction), CreateArguments(GetArguments(container)));

        /// <summary>Extra constructor arguments Zenject cannot resolve on its own.</summary>
        protected virtual object[] GetArguments(DiContainer container) => Array.Empty<object>();

        /// <summary>Carries an argument that is allowed to be empty, keeping the type Zenject matches it by.</summary>
        protected static object Optional<TArgument>(TArgument value) => InjectUtil.CreateTypePair(value);

        private static List<TypeValuePair> CreateArguments(object[] arguments)
        {
            var argumentList = new List<TypeValuePair>(arguments.Length);

            foreach (var argument in arguments)
            {
                if (argument is TypeValuePair typedArgument)
                {
                    argumentList.Add(typedArgument);

                    continue;
                }

                if (argument == null)
                    throw new InvalidOperationException($"{typeof(TAction).Name} received an untyped null argument. Wrap arguments that may be empty in Optional().");

                argumentList.Add(new TypeValuePair(argument.GetType(), argument));
            }

            return argumentList;
        }
    }
}
