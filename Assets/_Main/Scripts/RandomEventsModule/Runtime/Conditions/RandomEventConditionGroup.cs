using System.Collections.Generic;
using System.Linq;

namespace RandomEventsModule
{
    /// <summary>Runtime form of a <see cref="RandomEventConditionSet"/>: resolved conditions plus the mode that combines them.</summary>
    /// <remarks>An empty group is met, so an unfilled condition set never blocks anything.</remarks>
    public class RandomEventConditionGroup
    {
        private readonly IReadOnlyList<IRandomEventCondition> _conditions;
        private readonly ConditionMatchType _match;

        /// <summary>Creates the group over already-resolved conditions.</summary>
        public RandomEventConditionGroup(IReadOnlyList<IRandomEventCondition> conditions, ConditionMatchType match)
        {
            _conditions = conditions;
            _match = match;
        }

        /// <summary>Evaluates the conditions, short-circuiting on the first decisive one.</summary>
        public bool IsMet()
        {
            if (_conditions == null || _conditions.Count == 0)
                return true;

            return _match == ConditionMatchType.All
                ? _conditions.All(condition => condition.IsMet())
                : _conditions.Any(condition => condition.IsMet());
        }
    }
}
