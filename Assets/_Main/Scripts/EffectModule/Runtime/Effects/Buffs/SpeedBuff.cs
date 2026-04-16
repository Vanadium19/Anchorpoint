using ComponentsModule;
using UnityEngine;

namespace EffectModule
{
    public class SpeedBuff : BuffBase
    {
        private readonly float _speedMultiplier;
        private IMoveComponent _moveComponent;

        public override string EffectId => "speed";
        public override string BuffId => "speed";

        public SpeedBuff(float duration, float speedMultiplier) : base(duration)
        {
            _speedMultiplier = speedMultiplier;
        }

        protected override void OnApply()
        {
            if (Target == null)
                return;

            if (!Target.TryGet(out _moveComponent))
                return;

            _moveComponent.SetSpeedMultiplier(_speedMultiplier);
        }

        protected override void OnExpire()
        {
            ResetSpeed();
        }

        protected override void OnCancel()
        {
            ResetSpeed();
        }

        private void ResetSpeed()
        {
            if (_moveComponent != null)
                _moveComponent.ResetSpeedMultiplier();

            _moveComponent = null;
        }
    }
}
