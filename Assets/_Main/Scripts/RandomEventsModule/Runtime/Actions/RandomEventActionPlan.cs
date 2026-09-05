using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace RandomEventsModule
{
    /// <summary>A resolved action sequence, ready to run: groups of actions executed one group after another.</summary>
    /// <remarks>
    /// Actions inside one group run in parallel, groups run in order. A group that returns
    /// <c>false</c> stops the plan — that is how a gate step aborts the rest of the sequence.
    /// The same plan type backs an event's own sequence and every nested branch, loop body or
    /// parallel block, so composite actions never re-implement execution.
    /// </remarks>
    public class RandomEventActionPlan
    {
        private readonly IReadOnlyList<IReadOnlyList<IRandomEventAction>> _groups;

        /// <summary>Creates the plan over already-resolved action groups.</summary>
        public RandomEventActionPlan(IReadOnlyList<IReadOnlyList<IRandomEventAction>> groups)
        {
            _groups = groups;
        }

        /// <summary>Whether the plan has nothing to run.</summary>
        public bool IsEmpty => _groups.Count == 0;

        /// <summary>Runs every group in order; returns <c>false</c> when a step gated the sequence.</summary>
        public async UniTask<bool> ExecuteAsync(CancellationToken token)
        {
            foreach (var group in _groups)
            {
                if (group.Count == 1)
                {
                    if (!await group[0].ExecuteAsync(token))
                        return false;

                    continue;
                }

                var results = await UniTask.WhenAll(group.Select(action => action.ExecuteAsync(token)));

                if (results.Any(passed => !passed))
                    return false;
            }

            return true;
        }

        /// <summary>Asks every action in the plan to stop cooperatively.</summary>
        public void RequestStop()
        {
            foreach (var action in _groups.SelectMany(group => group))
                action.RequestStop();
        }
    }
}
