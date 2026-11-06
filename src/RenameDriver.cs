using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace RenameWorlds;

/// Keys: R renames the selected world; in the Rename box Enter saves and Esc cancels.
public class RenameDriver : MonoBehaviour
{
    public RenameDriver(IntPtr ptr) : base(ptr) { }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        try
        {
            GameInputPause.Tick(keyboard);
            if (RenameBox.IsOpen)
            {
                RenameBox.UpdateCounter();
                if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame) RenameBox.Save();
                else if (keyboard.escapeKey.wasPressedThisFrame) RenameBox.Close();
                return;
            }
            if (keyboard.rKey.wasPressedThisFrame && !TypingSomewhere())
            {
                var slot = SlotPatches.Selected;
                if (slot != null && slot.gameObject.activeInHierarchy) RenameBox.Open(slot);
            }
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"keys: {e.Message}");
        }
    }

    /// Never steal R while the player types in one of the game's own text fields.
    static bool TypingSomewhere()
    {
        var selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        return selected != null && selected.GetComponent<TMP_InputField>() != null;
    }
}
