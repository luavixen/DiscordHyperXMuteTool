using System;

namespace DiscordHyperXMuteTool
{
    internal abstract class Reactive<TSelf> where TSelf : Reactive<TSelf>
    {
        private readonly object _lock = new object();

        private event Action<TSelf> _updated;
        private bool _updating;

        public void Update()
        {
            lock (_lock)
            {
                if (_updating) return;
                try
                {
                    _updating = true;
                    _updated?.Invoke((TSelf)this);
                }
                finally
                {
                    _updating = false;
                }
            }
        }

        public IDisposable Subscribe(Action<TSelf> action)
        {
            lock (_lock)
            {
                _updated += action;
                return new Subscription(this, action);
            }
        }
        public IDisposable SubscribeComputed(Action<TSelf> action)
        {
            lock (_lock)
            {
                action((TSelf)this);
                return Subscribe(action);
            }
        }

        private sealed class Subscription : IDisposable
        {
            private readonly Reactive<TSelf> _reactive;
            private readonly Action<TSelf> _action;
            private bool _disposed;

            public Subscription(Reactive<TSelf> reactive, Action<TSelf> action)
            {
                _reactive = reactive;
                _action = action;
            }

            public void Dispose()
            {
                lock (this)
                {
                    if (!_disposed)
                    {
                        lock (_reactive._lock)
                        {
                            _reactive._updated -= _action;
                        }
                        _disposed = true;
                    }
                }
            }
        }
    }
}
