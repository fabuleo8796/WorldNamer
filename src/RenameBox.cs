using System;
using EHS;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RenameWorlds;

/// The "RENAME WORLD" box: dark rounded panel, the game's fonts, a paper-cut name field, SAVE / CANCEL.
/// Built once in code, shown on top of everything. Enter saves, Esc cancels.
internal static class RenameBox
{
    static GameObject root;
    static TMP_InputField input;
    static TextMeshProUGUI message, counter;
    static SaveSlotButton slot;
    static string worldGuid;

    public static bool IsOpen => root != null && root.activeSelf;

    public static void Open(SaveSlotButton target)
    {
        var data = target != null ? target.Data : null;
        if (!SlotPatches.IsRenamable(data)) return;
        if (root == null) Build();
        slot = target;
        worldGuid = data.WorldGuid;
        var current = WorldNames.TryGet(worldGuid, out var name) ? name : data.MapDisplayName;
        message.text = $"Give \"{current}\" a new name";
        input.text = WorldNames.Clean(current);
        root.SetActive(true);
        GameInputPause.Begin();
        UpdateCounter();
        EventSystem.current?.SetSelectedGameObject(input.gameObject);
        input.ActivateInputField();
        input.MoveTextEnd(false);
        input.selectionAnchorPosition = 0; // select all: typing replaces the old name
    }

    public static void Save()
    {
        if (!IsOpen) return;
        WorldNames.Set(worldGuid, input.text);
        if (slot != null && slot.view != null) SlotPatches.ApplyName(slot.view, slot.Data);
        Plugin.Logger.LogInfo($"Renamed world {worldGuid} to \"{WorldNames.Clean(input.text)}\"");
        Close();
    }

    public static void Close()
    {
        if (root == null) return;
        input.DeactivateInputField();
        root.SetActive(false);
        GameInputPause.End();
        // Hand focus back to the world that was renamed.
        if (slot != null && slot.gameObject.activeInHierarchy) EventSystem.current?.SetSelectedGameObject(slot.gameObject);
        slot = null;
    }

    public static void UpdateCounter()
    {
        if (counter != null && input != null) counter.text = $"{input.text.Length} / {WorldNames.MaxLength}";
    }

    // ---------------------------------------------------------------- building

    static void Build()
    {
        root = new GameObject("RenameWorldsBox");
        UnityEngine.Object.DontDestroyOnLoad(root);
        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 30000;
        var scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        root.AddComponent<GraphicRaycaster>();

        // Dim the game behind and catch clicks outside the box.
        var dim = Panel(root.transform, "Dim", new Color(0, 0, 0, 0.55f), null);
        Stretch(dim);

        var box = Panel(root.transform, "Box", new Color(28 / 255f, 28 / 255f, 30 / 255f, 0.95f), Assets.Rounded);
        box.sizeDelta = new Vector2(840, 520);

        Label(box, "RENAME WORLD", Assets.Title, 54, Color.white, new Vector2(0, 190), new Vector2(780, 70));
        message = Label(box, "", Assets.Body, 40, new Color(1, 1, 1, 0.85f), new Vector2(0, 128), new Vector2(780, 50));

        input = Field(box, new Vector2(0, 30), new Vector2(740, 128));
        counter = Label(box, "", Assets.Body, 32, new Color(1, 1, 1, 0.7f), new Vector2(0, -52), new Vector2(720, 40));
        counter.alignment = TextAlignmentOptions.Right;

        var close = Button(box, "X", new Vector2(370, 210), false, new Vector2(64, 64), 30);
        close.onClick.AddListener((UnityAction)new Action(Close));

        var save = Button(box, "SAVE", new Vector2(-170, -160), true);
        save.onClick.AddListener((UnityAction)new Action(Save));
        var cancel = Button(box, "CANCEL", new Vector2(170, -160), false);
        cancel.onClick.AddListener((UnityAction)new Action(Close));

        root.SetActive(false);
    }

    static RectTransform Panel(Transform parent, string name, Color color, Sprite sprite)
    {
        var go = new GameObject(name);
        var rect = go.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        var image = go.AddComponent<Image>();
        image.color = color;
        if (sprite != null) { image.sprite = sprite; image.type = Image.Type.Sliced; }
        return rect;
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
    }

    static TextMeshProUGUI Label(RectTransform parent, string text, TMP_FontAsset font, float size, Color color,
                                 Vector2 position, Vector2 box)
    {
        var go = new GameObject("Label");
        var rect = go.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.sizeDelta = box;
        rect.anchoredPosition = position;
        var label = go.AddComponent<TextMeshProUGUI>();
        if (font != null) label.font = font;
        label.text = text;
        label.fontSize = size;
        label.color = color;
        label.alignment = TextAlignmentOptions.Center;
        label.enableWordWrapping = false;
        label.overflowMode = TextOverflowModes.Ellipsis;
        label.raycastTarget = false;
        return label;
    }

    static TMP_InputField Field(RectTransform parent, Vector2 position, Vector2 size)
    {
        var go = new GameObject("NameField");
        go.SetActive(false); // TMP_InputField needs its parts before it is enabled
        var rect = go.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        var background = go.AddComponent<Image>();
        background.sprite = Assets.Wobbly;
        background.type = Assets.Wobbly == Assets.Rounded ? Image.Type.Sliced : Image.Type.Simple;
        background.color = Color.white;

        var area = new GameObject("Text Area");
        var areaRect = area.AddComponent<RectTransform>();
        areaRect.SetParent(rect, false);
        Stretch(areaRect);
        areaRect.offsetMin = new Vector2(48, 14);
        areaRect.offsetMax = new Vector2(-48, -14);
        area.AddComponent<RectMask2D>();

        var text = Label(areaRect, "", Assets.Title, 46, new Color(0.07f, 0.07f, 0.07f), Vector2.zero, Vector2.zero);
        Stretch(text.rectTransform);
        text.overflowMode = TextOverflowModes.Overflow;
        var placeholder = Label(areaRect, "WORLD NAME", Assets.Title, 46, new Color(0, 0, 0, 0.3f), Vector2.zero, Vector2.zero);
        Stretch(placeholder.rectTransform);

        var field = go.AddComponent<TMP_InputField>();
        field.textViewport = areaRect;
        field.textComponent = text;
        field.placeholder = placeholder;
        field.characterLimit = WorldNames.MaxLength;
        field.lineType = TMP_InputField.LineType.SingleLine;
        field.customCaretColor = true;
        field.caretColor = new Color(0.07f, 0.07f, 0.07f);
        field.caretWidth = 4;
        field.selectionColor = new Color(0.55f, 0.75f, 1f, 0.55f);
        field.targetGraphic = background;
        go.SetActive(true);
        return field;
    }

    static Button Button(RectTransform parent, string label, Vector2 position, bool primary, Vector2? size = null, float fontSize = 34)
    {
        var rect = Panel(parent, label, primary ? new Color(0.29f, 0.29f, 0.31f) : new Color(0.23f, 0.23f, 0.24f), Assets.Rounded);
        rect.sizeDelta = size ?? new Vector2(300, 96);
        rect.anchoredPosition = position;
        if (primary)
        {
            // White frame around the default button, like the game's selected button.
            var frame = rect.gameObject.AddComponent<Outline>();
            frame.effectColor = Color.white;
            frame.effectDistance = new Vector2(4, -4);
        }
        var text = Label(rect, label, Assets.Title, fontSize, Color.white, Vector2.zero, Vector2.zero);
        Stretch(text.rectTransform);
        var button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = rect.GetComponent<Image>();
        return button;
    }
}
