using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>
    /// A flat, ordered list of action steps: by default each step runs after the previous one,
    /// a step marked <c>runInParallel</c> runs alongside it.
    /// </summary>
    /// <remarks>
    /// Nest one sequence inside another through composite actions — the inspector draws every
    /// sequence, however deep, with the same step list.
    /// </remarks>
    [Serializable]
    public class RandomEventActionSequence
    {
        [SerializeField] private List<RandomEventActionStep> steps = new();

        /// <summary>Resolves the steps into a runnable plan.</summary>
        public RandomEventActionPlan Create(DiContainer container)
        {
            var groups = new List<List<IRandomEventAction>>();

            foreach (var step in steps)
            {
                var action = step?.Create(container);

                if (action == null)
                    continue;

                if (step.RunInParallel && groups.Count > 0)
                    groups[^1].Add(action);
                else
                    groups.Add(new List<IRandomEventAction> { action });
            }

            return new RandomEventActionPlan(groups);
        }
    }
}
