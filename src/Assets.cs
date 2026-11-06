using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

namespace RenameWorlds;

/// The pencil (embedded), a rounded panel shape (generated), and the game's own fonts and button art,
/// looked up by name so the Rename box looks like the rest of Flipping is Hard.
internal static class Assets
{
    static Sprite pencil, rounded, wobbly;
    static TMP_FontAsset bungee, jersey;

    public static Sprite Pencil => pencil ??= LoadPencil();
    public static Sprite Rounded => rounded ??= MakeRounded(64, 18);
    /// The game's white "paper cut" button (Menu Button Full), or the rounded shape if it isn't loaded.
    public static Sprite Wobbly => wobbly ??= FindSprite("Menu Button Full") ?? Rounded;
    public static TMP_FontAsset Title => bungee ??= FindFont("Bungee-Regular SDF");
    public static TMP_FontAsset Body => jersey ??= FindFont("Jersey10-Regular SDF") ?? Title;

    static Sprite LoadPencil()
    {
        const int size = 256;
        using var stream = typeof(Assets).Assembly.GetManifestResourceStream("pencil.rgba");
        var bytes = new byte[size * size * 4];
        using (var ms = new MemoryStream(bytes)) stream!.CopyTo(ms);
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color32[size * size];
        for (var i = 0; i < pixels.Length; i++)
            pixels[i] = new Color32(bytes[i * 4], bytes[i * 4 + 1], bytes[i * 4 + 2], bytes[i * 4 + 3]);
        tex.SetPixels32(pixels);
        tex.Apply(false, true);
        tex.filterMode = FilterMode.Bilinear;
        tex.hideFlags = HideFlags.HideAndDontSave;
        var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        sprite.hideFlags = HideFlags.HideAndDontSave;
        return sprite;
    }

    /// A white rounded square, 9-sliced, for the dark panel and the buttons.
    static Sprite MakeRounded(int size, int radius)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color32[size * size];
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
        {
            float cx = Mathf.Clamp(x + 0.5f, radius, size - radius), cy = Mathf.Clamp(y + 0.5f, radius, size - radius);
            var d = Mathf.Sqrt((x + 0.5f - cx) * (x + 0.5f - cx) + (y + 0.5f - cy) * (y + 0.5f - cy));
            var a = (byte)(Mathf.Clamp01(radius - d + 0.5f) * 255);
            pixels[y * size + x] = new Color32(255, 255, 255, a);
        }
        tex.SetPixels32(pixels);
        tex.Apply(false, true);
        tex.hideFlags = HideFlags.HideAndDontSave;
        var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
                                   SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        sprite.hideFlags = HideFlags.HideAndDontSave;
        return sprite;
    }

    static Sprite FindSprite(string name) =>
        Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(s => s != null && s.name == name);

    static TMP_FontAsset FindFont(string name)
    {
        var all = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
        return all.FirstOrDefault(f => f != null && f.name == name)
            ?? all.FirstOrDefault(f => f != null && f.name.StartsWith(name.Split(' ')[0]));
    }
}
