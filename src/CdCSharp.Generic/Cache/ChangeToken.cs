namespace CdCSharp.Generic.Cache;
/// <summary>
/// Propagates notifications that a change has occurred.
/// </summary>
public static class ChangeToken
{
    /// <summary>
    /// Registers the <paramref name="changeTokenConsumer"/> action to be called whenever the token produced changes.
    /// </summary>
    /// <param name="changeTokenProducer">Produces the change token.</param>
    /// <param name="changeTokenConsumer">Action called when the token changes.</param>
    /// <returns></returns>
    public static IDisposable OnChange(Func<IChangeToken?> changeTokenProducer, Action changeTokenConsumer)
    {
        if (changeTokenProducer is null)
        {
            throw new ArgumentNullException(nameof(changeTokenProducer));
        }
        if (changeTokenConsumer is null)
        {
            throw new ArgumentNullException(nameof(changeTokenConsumer));
        }

        return new ChangeTokenRegistration<Action>(changeTokenProducer, callback => callback(), changeTokenConsumer);
    }

    /// <summary>
    /// Registers the <paramref name="changeTokenConsumer"/> action to be called whenever the token produced changes.
    /// </summary>
    /// <param name="changeTokenProducer">Produces the change token.</param>
    /// <param name="changeTokenConsumer">Action called when the token changes.</param>
    /// <param name="state">state for the consumer.</param>
    /// <returns></returns>
    public static IDisposable OnChange<TState>(Func<IChangeToken?> changeTokenProducer, Action<TState> changeTokenConsumer, TState state)
    {
        if (changeTokenProducer is null)
        {
            throw new ArgumentNullException(nameof(changeTokenProducer));
        }
        if (changeTokenConsumer is null)
        {
            throw new ArgumentNullException(nameof(changeTokenConsumer));
        }

        return new ChangeTokenRegistration<TState>(changeTokenProducer, changeTokenConsumer, state);
    }

    private sealed class ChangeTokenRegistration<TState> : IDisposable
    {
        private readonly Func<IChangeToken?> _changeTokenProducer;
        private readonly Action<TState> _changeTokenConsumer;
        private readonly TState _state;
        private IDisposable? _disposable;

        private static readonly NoopDisposable _disposedSentinel = new();

        public ChangeTokenRegistration(Func<IChangeToken?> changeTokenProducer, Action<TState> changeTokenConsumer, TState state)
        {
            _changeTokenProducer = changeTokenProducer;
            _changeTokenConsumer = changeTokenConsumer;
            _state = state;

            IChangeToken? token = changeTokenProducer();

            RegisterChangeTokenCallback(token);
        }

        private void OnChangeTokenFired()
        {
            // Prevenir reentradas si ya se procesó
            if (_hasProcessedChange)
            {
                return;
            }

            // Obtenemos el siguiente token de cambio
            IChangeToken? token = _changeTokenProducer();

            try
            {
                _changeTokenConsumer(_state);
            }
            finally
            {
                // Solo registrar si el nuevo token no ha cambiado aún
                if (token != null && !token.HasChanged)
                {
                    RegisterChangeTokenCallback(token);
                }
            }
        }

        private bool _hasProcessedChange = false; // Nueva bandera para evitar reentradas innecesarias

        private void RegisterChangeTokenCallback(IChangeToken? token)
        {
            if (token is null || _hasProcessedChange)
            {
                // Si el token es nulo o ya se procesó, no registrar más callbacks
                return;
            }

            IDisposable registraton = token.RegisterChangeCallback(
                s => ((ChangeTokenRegistration<TState>?)s)!.OnChangeTokenFired(),
                this);

            // Si el token ya ha cambiado, no necesitamos registrarlo de nuevo
            if (token.HasChanged && token.ActiveChangeCallbacks)
            {
                registraton.Dispose();
                _hasProcessedChange = true; // Marcar como procesado
                return;
            }

            SetDisposable(registraton);
        }

        private void SetDisposable(IDisposable disposable)
        {
            // We don't want to transition from _disposedSentinel => anything since it's terminal
            // but we want to allow going from previously assigned disposable, to another
            // disposable.
            IDisposable? current = Volatile.Read(ref _disposable);

            // If Dispose was called, then immediately dispose the disposable
            if (current == _disposedSentinel)
            {
                disposable.Dispose();
                return;
            }

            // Otherwise, try to update the disposable
            IDisposable? previous = Interlocked.CompareExchange(ref _disposable, disposable, current);

            if (previous == _disposedSentinel)
            {
                // The subscription was disposed so we dispose immediately and return
                disposable.Dispose();
            }
            else if (previous == current)
            {
                // We successfully assigned the _disposable field to disposable
            }
            else
            {
                // Sets can never overlap with other SetDisposable calls so we should never get into this situation
                throw new InvalidOperationException("Somebody else set the _disposable field");
            }
        }

        public void Dispose() =>
            // If the previous value is disposable then dispose it, otherwise,
            // now we've set the disposed sentinel
            Interlocked.Exchange(ref _disposable, _disposedSentinel)?.Dispose();

        private sealed class NoopDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
