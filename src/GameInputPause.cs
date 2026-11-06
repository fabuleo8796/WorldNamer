using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace RenameWorlds;

/// Switches the game's controls off while the Rename box is open, so typing "E" doesn't flip the page
/// (and Enter/Esc don't also reach the game's menus). The mouse keeps working for the box's buttons.
/// Controls come back once the box is closed and Enter/Esc are let go, so that key press never leaks
/// into the game either.
internal static class GameInputPause
{
    static readonly List<InputAction> paused = new();
    static bool waitingForRelease;

    /// True from opening the box until the closing key is released.
    public static bool Active { get; private set; }

    public static void Begin()
    {
        if (Active && !waitingForRelease) return;
        waitingForRelease = false;
        Active = true;
        try
        {
            var enabled = new Il2CppSystem.Collections.Generic.List<InputAction>();
            InputSystem.ListEnabledActions(enabled);
            for (var i = 0; i < enabled.Count; i++)
            {
                var action = enabled[i];
                if (action == null || IsPointer(action)) continue;
                action.Disable();
                paused.Add(action);
            }
            Plugin.Logger.LogInfo($"Game controls paused while renaming ({paused.Count} actions)");
        }
        catch (Exception e)
        {
            Plugin.Logger.LogWarning($"Couldn't pause game controls: {e.Message}");
        }
    }

    /// Called when the box closes; the controls return in Tick() once the keys are up.
    public static void End()
    {
        if (Active) waitingForRelease = true;
    }

    public static void Tick(Keyboard keyboard)
    {
        if (!waitingForRelease) return;
        if (keyboard != null && (keyboard.enterKey.isPressed || keyboard.numpadEnterKey.isPressed || keyboard.escapeKey.isPressed)) return;
        foreach (var action in paused)
        {
            try { action?.Enable(); } catch { /* the action's owner may be gone */ }
        }
        paused.Clear();
        waitingForRelease = false;
        Active = false;
    }

    /// Pointing and clicking stay on (the box needs the mouse); everything else pauses.
    static bool IsPointer(InputAction action)
    {
        var name = (action.name ?? "").ToLowerInvariant();
        return name.Contains("point") || name.Contains("click") || name.Contains("scroll") || name.Contains("tracked");
    }
}
