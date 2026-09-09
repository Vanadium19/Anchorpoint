using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>Queues notification messages and shows them one at a time on the HUD view.</summary>
    public class RandomEventNotificationPresenter : IInitializable, IDisposable
    {
        private readonly IRandomEventNotifier _notifier;
        private readonly RandomEventNotificationView _view;
        private readonly Queue<RandomEventNotificationMessage> _messageQueue = new();

        private CancellationTokenSource _tokenSource;
        private bool _isShowing;

        /// <summary>Creates the presenter for one notifier/view pair.</summary>
        public RandomEventNotificationPresenter(IRandomEventNotifier notifier, RandomEventNotificationView view)
        {
            _notifier = notifier;
            _view = view;
        }

        /// <summary>Hides the view and subscribes to the notifier.</summary>
        public void Initialize()
        {
            _view.Hide();

            _tokenSource = new();
            _notifier.MessageRequested += OnMessageRequested;
        }

        /// <summary>Unsubscribes from the notifier and cancels any pending display.</summary>
        public void Dispose()
        {
            _notifier.MessageRequested -= OnMessageRequested;

            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
        }

        private void OnMessageRequested(RandomEventNotificationMessage message)
        {
            _messageQueue.Enqueue(message);

            if (_isShowing)
                return;

            _isShowing = true;
            ShowQueueAsync(_tokenSource.Token).Forget();
        }

        private async UniTaskVoid ShowQueueAsync(CancellationToken token)
        {
            try
            {
                while (_messageQueue.Count > 0)
                {
                    var message = _messageQueue.Dequeue();
                    _view.Show(message.Text);

                    var canceled = await UniTask.Delay(TimeSpan.FromSeconds(message.DurationSeconds), cancellationToken: token)
                        .SuppressCancellationThrow();

                    if (canceled)
                        return;
                }
            }
            finally
            {
                _view.Hide();
                _isShowing = false;
            }
        }
    }
}
