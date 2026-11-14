namespace PKForge.App;

/// <summary>
/// Desktop stand-in for MAUI's <c>MainThread</c>, which the Avalonia backend cannot implement
/// (it throws there). Every file lives in a PKForge.App namespace, so this type wins name lookup
/// over the global using and the existing call sites compile unchanged. Calls go through the
/// application dispatcher, which is the UI thread on every platform.
/// </summary>
public static class MainThread
{
    private static IDispatcher Dispatcher =>
        Application.Current?.Dispatcher
        ?? Microsoft.Maui.Dispatching.Dispatcher.GetForCurrentThread()
        ?? throw new InvalidOperationException("No UI dispatcher is available yet.");

    public static bool IsMainThread => !Dispatcher.IsDispatchRequired;

    public static void BeginInvokeOnMainThread(Action action)
    {
        if (IsMainThread) action();
        else Dispatcher.Dispatch(action);
    }

    public static Task InvokeOnMainThreadAsync(Action action) => Dispatcher.DispatchAsync(action);

    public static Task<T> InvokeOnMainThreadAsync<T>(Func<T> function) => Dispatcher.DispatchAsync(function);

    public static Task InvokeOnMainThreadAsync(Func<Task> function) => Dispatcher.DispatchAsync(function);

    public static Task<T> InvokeOnMainThreadAsync<T>(Func<Task<T>> function) => Dispatcher.DispatchAsync(function);
}
