using CdCSharp.Generic.Cache;

namespace CdCSharp.Generic.UnitTests.Cache;
internal class TestChangeToken : IChangeToken
{
    private readonly CancellationTokenSource _cts;
    private bool _callbackExecuted = false; // Evita múltiples ejecuciones de callbacks

    public TestChangeToken(CancellationTokenSource cts) => _cts = cts;

    public bool ActiveChangeCallbacks => true;
    public bool HasChanged => _cts.Token.IsCancellationRequested;

    public IDisposable RegisterChangeCallback(Action<object?> callback, object? state)
    {
        // Si el token ya está cancelado, ejecuta el callback inmediatamente solo una vez
        if (_cts.Token.IsCancellationRequested && !_callbackExecuted)
        {
            try
            {
                _callbackExecuted = true; // Marcar como ejecutado
                callback(state);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error ejecutando el callback de inmediato: {ex}");
            }
        }

        // Registra el callback para cambios futuros del token
        return _cts.Token.Register(() =>
        {
            if (!_callbackExecuted) // Ejecuta el callback solo si no ha sido ejecutado
            {
                try
                {
                    _callbackExecuted = true; // Marcar como ejecutado
                    callback(state);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en el callback registrado: {ex}");
                }
            }
        });
    }
}

public class ChangeToken_Tests
{
    [Fact]
    public void OnChange_ValidatesArguments()
    {
        Assert.Throws<ArgumentNullException>(() => ChangeToken.OnChange(null!, () => { }));
        Assert.Throws<ArgumentNullException>(() => ChangeToken.OnChange(() => null, null!));
    }

    [Fact]
    public void OnChange_WithState_ValidatesArguments()
    {
        Assert.Throws<ArgumentNullException>(() => ChangeToken.OnChange<object>(null!, _ => { }, null!));
        Assert.Throws<ArgumentNullException>(() => ChangeToken.OnChange<object>(() => null, null!, null!));
    }

    [Fact]
    public void OnChange_InvokesCallbackWhenTokenChanges()
    {
        bool callbackInvoked = false;
        CancellationTokenSource tokenSource = new();

        using (ChangeToken.OnChange(
            () => new TestChangeToken(tokenSource),
            () => callbackInvoked = true))
        {
            Assert.False(callbackInvoked);
            tokenSource.Cancel();
            Assert.True(callbackInvoked);
        }
    }

    [Fact]
    public void OnChange_WithState_InvokesCallbackWithState()
    {
        object state = new();
        object? capturedState = null;
        CancellationTokenSource tokenSource = new();

        using (ChangeToken.OnChange(
            () => new TestChangeToken(tokenSource),
            s => capturedState = s,
            state))
        {
            tokenSource.Cancel();
            Assert.Same(state, capturedState);
        }
    }


    [Fact]
    public void OnChange_DisposingRegistrationStopsCallbacks()
    {
        int callbackCount = 0;
        CancellationTokenSource tokenSource = new();

        IDisposable registration = ChangeToken.OnChange(
            () => new TestChangeToken(tokenSource),
            () => callbackCount++);

        registration.Dispose();
        tokenSource.Cancel();
        Assert.Equal(0, callbackCount);
    }
}
