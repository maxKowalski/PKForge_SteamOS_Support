using System.Reflection;
using System.Runtime.InteropServices;
using PKForge.App.Services;

namespace PKForge.App;

/// <summary>
/// Game controllers on desktop through SDL2's game-controller API, which is how Steam Input
/// hands its virtual controller to apps in SteamOS Game Mode. Buttons go to the
/// <see cref="GamepadRouter"/> the way Android's key events do: the d-pad and an unread left
/// stick step once, then repeat after a hold, at the same cadence as on Android.
/// </summary>
internal static partial class DesktopGamepad
{
    private const long HoldDelayMs = 320;
    private const long RepeatMs = 80;
    private const long ShoulderHoldDelayMs = 380;
    private const long ShoulderRepeatMs = 140;
    private const float StickDeadZone = .25f;

    private static IDispatcher? _ui;
    private static PadButton? _held;
    private static long _heldToken;
    private static PadButton? _stickDirection;
    private static readonly float[] Axes = new float[4];

    /// <summary>Starts listening for controllers on a background thread; a missing SDL2 only logs.</summary>
    public static void Start(IDispatcher ui)
    {
        _ui = ui;
        NativeLibrary.SetDllImportResolver(typeof(DesktopGamepad).Assembly, Resolve);
        new Thread(Pump) { IsBackground = true, Name = "SDL gamepad" }.Start();
    }

    /// <summary>The bundled libSDL2-2.0.so first, then the system's.</summary>
    private static IntPtr Resolve(string name, Assembly assembly, DllImportSearchPath? path)
    {
        if (name != Sdl) return IntPtr.Zero;
        foreach (var candidate in new[] { Path.Combine(AppContext.BaseDirectory, "libSDL2-2.0.so"), "libSDL2-2.0.so.0", "libSDL2.so" })
            if (NativeLibrary.TryLoad(candidate, out var handle))
                return handle;
        return IntPtr.Zero;
    }

    private static void Pump()
    {
        try
        {
            // PKForge has no SDL window: without this, SDL drops input whenever it thinks it is unfocused.
            SDL_SetHint("SDL_JOYSTICK_ALLOW_BACKGROUND_EVENTS", "1");
            if (SDL_Init(InitGameController) != 0)
            {
                AppLog.Warn("pad", $"SDL could not start: {Marshal.PtrToStringUTF8(SDL_GetError())}");
                return;
            }
            AppLog.Info("pad", "Listening for game controllers (SDL2)");
            var buffer = new byte[56];
            while (true)
            {
                if (SDL_WaitEventTimeout(buffer, 100) == 0) continue;
                var type = BitConverter.ToUInt32(buffer, 0);
                switch (type)
                {
                    case ControllerDeviceAdded:
                        var index = BitConverter.ToInt32(buffer, 8);
                        if (SDL_GameControllerOpen(index) != IntPtr.Zero)
                            AppLog.Info("pad", $"Controller connected: {Marshal.PtrToStringUTF8(SDL_GameControllerNameForIndex(index)) ?? "unknown"}");
                        break;
                    case ControllerButtonDown when ResolveButton(buffer[12]) is { } pressed:
                        _ui?.Dispatch(() => Press(pressed));
                        break;
                    case ControllerButtonUp when ResolveButton(buffer[12]) is { } released:
                        _ui?.Dispatch(() => Release(released));
                        break;
                    case ControllerAxisMotion when buffer[12] < 4:
                        var axis = buffer[12];
                        var value = BitConverter.ToInt16(buffer, 16) / 32767f;
                        _ui?.Dispatch(() => Axis(axis, value));
                        break;
                }
            }
        }
        catch (Exception error) when (error is DllNotFoundException or EntryPointNotFoundException)
        {
            AppLog.Warn("pad", $"No SDL2 library: game controllers are unavailable ({error.Message})");
        }
    }

    private static GamepadRouter? Router => IPlatformApplication.Current?.Services.GetService<GamepadRouter>();

    private static bool IsDirectional(PadButton button) =>
        button is PadButton.Up or PadButton.Down or PadButton.Left or PadButton.Right;

    private static void Press(PadButton button)
    {
        var router = Router;
        // Asked before dispatching: the press itself may open another screen.
        var repeats = IsDirectional(button) || (button is PadButton.L or PadButton.R && router?.TopPagesWithShoulders == true);
        if (router?.Dispatch(button) == true && repeats)
            Hold(button);
    }

    private static void Release(PadButton button)
    {
        if (_held == button) _held = null;
        Router?.DispatchRelease(button);
    }

    /// <summary>Repeats a held button until it is released or another one takes over.</summary>
    private static void Hold(PadButton button)
    {
        _held = button;
        var token = ++_heldToken;
        var directional = IsDirectional(button);
        void Step()
        {
            if (_held != button || token != _heldToken) return;
            var router = Router;
            if (!directional && router?.TopPagesWithShoulders != true) { _held = null; return; }
            if (router?.Dispatch(button) != true) { _held = null; return; }
            _ui?.DispatchDelayed(TimeSpan.FromMilliseconds(directional ? RepeatMs : ShoulderRepeatMs), Step);
        }
        _ui?.DispatchDelayed(TimeSpan.FromMilliseconds(directional ? HoldDelayMs : ShoulderHoldDelayMs), Step);
    }

    /// <summary>
    /// The sticks go to a screen that reads them (the Poképark's camera); otherwise the left
    /// stick steps like the d-pad, as Android's own joystick navigation does.
    /// </summary>
    private static void Axis(int axis, float value)
    {
        var size = Math.Abs(value);
        Axes[axis] = size <= StickDeadZone ? 0 : Math.Sign(value) * Math.Min(1, (size - StickDeadZone) / (1 - StickDeadZone));
        if (Router?.DispatchStick(Axes[0], Axes[1], Axes[2], Axes[3]) == true) return;
        var x = Axes[0];
        var y = Axes[1];
        PadButton? direction = Math.Abs(x) < .5f && Math.Abs(y) < .5f ? null
            : Math.Abs(y) >= Math.Abs(x) ? (y < 0 ? PadButton.Up : PadButton.Down)
            : (x < 0 ? PadButton.Left : PadButton.Right);
        if (direction == _stickDirection) return;
        if (_stickDirection is { } previous) Release(previous);
        _stickDirection = direction;
        if (direction is { } pressed) Press(pressed);
    }

    // SDL's buttons are positional (A is the bottom face button), like Android's.
    private static PadButton? ResolveButton(byte button) => button switch
    {
        0 => PadButton.A,
        1 => PadButton.B,
        2 => PadButton.X,
        3 => PadButton.Y,
        4 => PadButton.Select,
        6 => PadButton.Start,
        9 => PadButton.L,
        10 => PadButton.R,
        11 => PadButton.Up,
        12 => PadButton.Down,
        13 => PadButton.Left,
        14 => PadButton.Right,
        _ => null,
    };

    private const string Sdl = "SDL2-2.0";
    private const uint InitGameController = 0x2000;
    private const uint ControllerAxisMotion = 0x650;
    private const uint ControllerButtonDown = 0x651;
    private const uint ControllerButtonUp = 0x652;
    private const uint ControllerDeviceAdded = 0x653;

    [LibraryImport(Sdl, StringMarshalling = StringMarshalling.Utf8)]
    private static partial int SDL_SetHint(string name, string value);

    [LibraryImport(Sdl)]
    private static partial int SDL_Init(uint flags);

    [LibraryImport(Sdl)]
    private static partial IntPtr SDL_GetError();

    /// <summary>SDL_Event is a 56-byte union; the fields read here sit at fixed offsets.</summary>
    [LibraryImport(Sdl)]
    private static partial int SDL_WaitEventTimeout([Out] byte[] sdlEvent, int timeout);

    [LibraryImport(Sdl)]
    private static partial IntPtr SDL_GameControllerOpen(int joystickIndex);

    [LibraryImport(Sdl)]
    private static partial IntPtr SDL_GameControllerNameForIndex(int joystickIndex);
}
