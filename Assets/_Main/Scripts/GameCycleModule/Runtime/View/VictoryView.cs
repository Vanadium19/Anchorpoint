using UnityEngine;

namespace GameCycleModule
{
    public class VictoryView : MonoBehaviour
    {
        [SerializeField] private GameObject panel;

        private GameObject _target => panel != null ? panel : gameObject;

        public void Show() => _target.SetActive(true);

        public void Hide() => _target.SetActive(false);
    }
}