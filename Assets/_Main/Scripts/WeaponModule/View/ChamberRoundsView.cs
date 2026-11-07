using UnityEngine;

namespace WeaponModule
{
    /// <summary>
    /// Shows the loaded rounds of a weapon (cylinder chambers, magazine top, tube) and the casings
    /// that replace them during a reload.
    /// </summary>
    public class ChamberRoundsView : WeaponViewPart
    {
        [Tooltip("Rounds in feed order, starting with the one fired next.")]
        [SerializeField] private GameObject[] rounds;
        [Tooltip("Casing of each round, same order as rounds. Optional.")]
        [SerializeField] private GameObject[] casings;
        [Tooltip("Animation Event id after which the remaining rounds move into place.")]
        [SerializeField] private string roundsAdvancedEventId = WeaponEventIds.ActionCycleCompleted;
        [Tooltip("Animation Event id at which the reloaded rounds become visible.")]
        [SerializeField] private string roundsLoadedEventId = WeaponEventIds.RoundsLoaded;

        private int _loadedCount;
        private bool _isReloading;

        private void LateUpdate()
        {
            if (_isReloading)
                HideRoundsReplacedByCasings();
        }

        public override void OnLoadedRoundsChanged(int count)
        {
            _loadedCount = Mathf.Clamp(count, 0, rounds.Length);

            if (!_isReloading)
                ShowLoadedRounds();
        }

        public override void OnFired()
        {
            if (_loadedCount == 0)
                return;

            rounds[0].SetActive(false);
            _loadedCount--;
        }

        public override void OnReloadStarted(bool isEmptyReload)
        {
            MatchCasingsToRounds();
            _isReloading = true;
        }

        public override void OnReloadStateChanged(bool isReloading)
        {
            if (!isReloading)
                CompleteReload();
        }

        public override void OnAnimationEvent(string eventId)
        {
            if (eventId == roundsLoadedEventId)
                CompleteReload();
            else if (eventId == roundsAdvancedEventId)
                ShowLoadedRounds();
        }

        private void CompleteReload()
        {
            _isReloading = false;
            ShowLoadedRounds();
        }

        private void ShowLoadedRounds()
        {
            if (_isReloading)
                return;

            for (var i = 0; i < rounds.Length; i++)
                rounds[i].SetActive(i < _loadedCount);
        }

        private void HideRoundsReplacedByCasings()
        {
            var count = Mathf.Min(rounds.Length, casings.Length);

            for (var i = 0; i < count; i++)
            {
                if (rounds[i].activeSelf && casings[i].activeInHierarchy)
                    rounds[i].SetActive(false);
            }
        }

        private void MatchCasingsToRounds()
        {
            var count = Mathf.Min(rounds.Length, casings.Length);

            for (var i = 0; i < count; i++)
                casings[i].SetActive(rounds[i].activeSelf);
        }
    }
}
