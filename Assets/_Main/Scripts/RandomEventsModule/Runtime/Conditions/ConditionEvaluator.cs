using System.Collections.Generic;
using System.Linq;

namespace RandomEventsModule
{
    internal static class ConditionEvaluator
    {
        public static bool IsMet(IReadOnlyList<IRandomEventCondition> conditions, ConditionMatchType conditionMatch)
        {
            if (conditions.Count == 0)
                return true;

            return conditionMatch == ConditionMatchType.All
                ? conditions.All(condition => condition.IsMet())
                : conditions.Any(condition => condition.IsMet());
        }
    }
}
