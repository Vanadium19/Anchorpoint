using System;
using System.Collections.Generic;
using EvacuationModule;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>Default <see cref="IEvacuationBlocker"/>, and the <see cref="IEvacuationGate"/> the evacuation service asks.</summary>
    /// <remarks>
    /// Refusing evacuation shows the message of the newest hold, so the player is told about the event
    /// happening now. Holds left behind by an interrupted event are dropped when the last random event
    /// finishes, which keeps a missing <c>Unblock</c> step from locking the scene for good.
    /// </remarks>
    public class EvacuationBlocker : IEvacuationBlocker, IEvacuationGate, IInitializable, IDisposable
    {
        private readonly IRandomEventService _service;
        private readonly IRandomEventNotifier _notifier;
        private readonly List<BlockedMessage> _holds = new();

        /// <summary>Creates the blocker over the scene event service and notifier.</summary>
        public EvacuationBlocker(IRandomEventService service, IRandomEventNotifier notifier)
        {
            _service = service;
            _notifier = notifier;
        }

        /// <inheritdoc/>
        public bool IsBlocked => _holds.Count > 0;

        /// <summary>Starts watching for finished events to drop abandoned holds.</summary>
        public void Initialize() => _service.EventFinished += OnEventFinished;

        /// <inheritdoc/>
        public void Block(string blockedMessageKey, float messageDurationSeconds)
        {
            Unblock(blockedMessageKey);

            _holds.Add(new BlockedMessage(blockedMessageKey, messageDurationSeconds));
        }

        /// <inheritdoc/>
        public void Unblock(string blockedMessageKey)
        {
            if (string.IsNullOrEmpty(blockedMessageKey))
            {
                _holds.Clear();

                return;
            }

            _holds.RemoveAll(hold => hold.LocalizationKey == blockedMessageKey);
        }

        /// <inheritdoc/>
        public bool TryEnterEvacuation()
        {
            if (_holds.Count == 0)
                return true;

            var message = _holds[^1];

            if (!string.IsNullOrEmpty(message.LocalizationKey))
                _notifier.Show(message.LocalizationKey, message.DurationSeconds);

            return false;
        }

        /// <summary>Stops watching finished events.</summary>
        public void Dispose() => _service.EventFinished -= OnEventFinished;

        private void OnEventFinished(RandomEventInfo info)
        {
            if (!_service.IsAnyEventActive)
                _holds.Clear();
        }

        private readonly struct BlockedMessage
        {
            public BlockedMessage(string localizationKey, float durationSeconds)
            {
                LocalizationKey = localizationKey;
                DurationSeconds = durationSeconds;
            }

            public string LocalizationKey { get; }

            public float DurationSeconds { get; }
        }
    }
}
