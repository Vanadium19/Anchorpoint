using System.Collections.Generic;

namespace WeaponModule
{
    /// <summary>
    /// The result of <see cref="WeaponViewValidator.Validate"/> for one weapon view: every problem found and the size
    /// of the space of builds the view was checked against.
    /// </summary>
    public sealed class WeaponViewReport
    {
        private readonly List<string> _issues = new();

        /// <summary>
        /// Every problem found, one readable line each, without duplicates.
        /// </summary>
        public IReadOnlyList<string> Issues => _issues;

        /// <summary>
        /// Number of modifications whose socket type the view has.
        /// </summary>
        public int CompatibleCount { get; internal set; }

        /// <summary>
        /// Number of different builds the view can show: for every socket type of the view, one modification or none.
        /// </summary>
        public long BuildSpace { get; internal set; }

        /// <summary>
        /// Number of builds that were applied to the view. Smaller than <see cref="BuildSpace"/> when sampled.
        /// </summary>
        public int EvaluatedBuilds { get; internal set; }

        /// <summary>
        /// Number of applied builds that showed no problem and contain no modification with a problem.
        /// </summary>
        public int ValidBuilds { get; internal set; }

        /// <summary>
        /// True when only part of the build space was applied, because it exceeded the limit.
        /// </summary>
        public bool IsSampled => EvaluatedBuilds < BuildSpace;

        /// <summary>
        /// Share of the applied builds that were valid, from 0 to 1.
        /// </summary>
        public double ValidShare => EvaluatedBuilds > 0 ? (double)ValidBuilds / EvaluatedBuilds : 0d;

        internal void AddIssue(string issue)
        {
            if (!_issues.Contains(issue))
                _issues.Add(issue);
        }
    }
}
