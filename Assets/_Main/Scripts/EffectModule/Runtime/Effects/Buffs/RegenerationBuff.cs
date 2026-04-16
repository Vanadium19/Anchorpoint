using ComponentsModule;

namespace EffectModule
{
    public class RegenerationBuff : BuffBase
    {
        private readonly float _healPerTick;
        private readonly float _tickInterval;
        private float _tickTimer;
        private IHealthComponent _health;

        public override string EffectId => "regeneration";
        public override string BuffId => "regeneration";

        public RegenerationBuff(float duration, float healPerTick, float tickInterval) : base(duration)
        {
            _healPerTick = healPerTick;
            _tickInterval = tickInterval;
        }

        protected override void OnApply()
        {
            Target?.TryGet(out _health);
            _tickTimer = 0f;
        }

        protected override void OnTick(float deltaTime)
        {
            if (_health == null)
                return;

            _tickTimer += deltaTime;

            while (_tickTimer >= _tickInterval)
            {
                _tickTimer -= _tickInterval;
                _health.Heal(_healPerTick);
            }
        }

        protected override void OnCancel()
        {
            _health = null;
            _tickTimer = 0f;
        }
    }
}
