using System;
using EHS;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RenameWorlds;

/// Hooks the world list: shows custom names, adds the pencil, remembers the selected slot (for the R key).
internal static class SlotPatches
{
    const string PencilName = "RenameWorldsPencil";
    internal static SaveSlotButton Selected;

    /// SaveSlotView.Init(SaveSlotMetaData) fills a slot's labels: put the custom name over the map name.
    [HarmonyPatch(typeof(SaveSlotView), nameof(SaveSlotView.Init))]
    [HarmonyPostfix]
    static void ViewInit(SaveSlotView __instance, SaveSlotMetaData __0)
    {
        try { ApplyName(__instance, __0); }
        catch (Exception e) { Plugin.Logger.LogError($"name: {e}"); }
    }

    /// SaveSlotButton.Init(StartGameManager, UIPanelManager, SaveSlotMetaData) sets up one world button.
    [HarmonyPatch(typeof(SaveSlotButton), nameof(SaveSlotButton.Init))]
    [HarmonyPostfix]
    static void ButtonInit(SaveSlotButton __instance, SaveSlotMetaData __2)
    {
        try { AddPencil(__instance, __2); }
        catch (Exception e) { Plugin.Logger.LogError($"pencil: {e}"); }
    }

    [HarmonyPatch(typeof(SaveSlotButton), nameof(SaveSlotButton.SetSelectedVisual))]
    [HarmonyPostfix]
    static void SelectedVisual(SaveSlotButton __instance, bool __0)
    {
        if (__0) Selected = __instance;
        else if (Selected != null && Selected.Pointer == __instance.Pointer) Selected = null;
    }

    /// While the Rename box is open the game's menus don't react to keys (E, Esc, arrows…).
    [HarmonyPatch(typeof(SaveSlotPageController), nameof(SaveSlotPageController.Update))]
    [HarmonyPrefix]
    static bool PageUpdate() => !GameInputPause.Active;

    [HarmonyPatch(typeof(UIPanelManager), nameof(UIPanelManager.Update))]
    [HarmonyPrefix]
    static bool PanelUpdate() => !GameInputPause.Active;

    internal static bool IsRenamable(SaveSlotMetaData data) =>
        data != null && !data.IsEmptySlot && !string.IsNullOrEmpty(data.WorldGuid);

    internal static void ApplyName(SaveSlotView view, SaveSlotMetaData data)
    {
        if (view == null || view.mapNameText == null || !IsRenamable(data)) return;
        view.mapNameText.text = WorldNames.TryGet(data.WorldGuid, out var name) ? name : data.MapDisplayName;
        PlacePencil(view);
    }

    /// The pencil sits right after the world's name ("TEST WORLD ✏"), inside the name label itself, so
    /// it never covers the slot's other icons and follows the name when it changes.
    static void AddPencil(SaveSlotButton slot, SaveSlotMetaData data)
    {
        var view = slot.view;
        if (view == null || view.mapNameText == null) return;
        var existing = view.mapNameText.transform.Find(PencilName);
        if (!IsRenamable(data))
        {
            // Slots are reused: an empty slot has nothing to rename.
            if (existing != null) existing.gameObject.SetActive(false);
            return;
        }
        if (existing == null) BuildPencil(view.mapNameText.rectTransform, slot);
        else existing.gameObject.SetActive(true);
        PlacePencil(view);
    }

    static void PlacePencil(SaveSlotView view)
    {
        var text = view.mapNameText;
        var pencil = text.transform.Find(PencilName);
        if (pencil == null) return;
        var rect = pencil.GetComponent<RectTransform>();
        var area = text.rectTransform.rect;
        // Where the letters really are (not the label's whole box).
        text.ForceMeshUpdate();
        var bounds = text.textBounds;
        var hasText = bounds.size.x > 0.5f && bounds.size.y > 0.5f;
        var letters = hasText ? bounds.size.y : text.fontSize * 0.75f;
        var size = Mathf.Max(40f, letters * 1.55f);
        var right = hasText ? bounds.max.x - area.xMin : Mathf.Min(text.GetPreferredValues(text.text).x, area.width);
        var middle = hasText ? bounds.center.y - area.center.y : 0f;
        rect.sizeDelta = new Vector2(size, size);
        rect.anchoredPosition = new Vector2(Mathf.Min(right, area.width) + size * 0.3f, middle);
    }

    static GameObject BuildPencil(RectTransform nameRect, SaveSlotButton slot)
    {
        var go = new GameObject(PencilName);
        go.layer = nameRect.gameObject.layer;
        var rect = go.AddComponent<RectTransform>();
        rect.SetParent(nameRect, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0f, 0.5f);
        rect.pivot = new Vector2(0f, 0.5f);

        // A soft rounded button, like the game's STORY / HARD tags, with the pencil inside.
        var background = go.AddComponent<Image>();
        background.sprite = Assets.Rounded;
        background.type = Image.Type.Sliced;
        background.pixelsPerUnitMultiplier = 1.6f;
        background.color = new Color(1f, 1f, 1f, 0.32f);
        var iconGo = new GameObject("Icon");
        iconGo.layer = go.layer;
        var iconRect = iconGo.AddComponent<RectTransform>();
        iconRect.SetParent(rect, false);
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.anchorMin = new Vector2(0.14f, 0.14f);
        iconRect.anchorMax = new Vector2(0.86f, 0.86f);
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;
        var icon = iconGo.AddComponent<Image>();
        icon.sprite = Assets.Pencil;
        icon.preserveAspect = true;
        icon.raycastTarget = false;

        var button = go.AddComponent<Button>();
        button.targetGraphic = background;
        var colors = button.colors;
        // Brighter pill on hover, darker when pressed (the tint multiplies the pill's colour).
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 1f, 1.7f);
        colors.pressedColor = new Color(0.55f, 0.55f, 0.55f, 1.4f);
        colors.selectedColor = Color.white;
        colors.colorMultiplier = 1f;
        button.colors = colors;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        var slotRef = slot;
        button.onClick.AddListener((UnityAction)new Action(() => RenameBox.Open(slotRef)));
        return go;
    }
}
