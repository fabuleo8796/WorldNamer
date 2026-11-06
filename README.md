# Fabuleo's World Namer

A **Flipping is Hard** mod by **Fabuleo8796**: give your worlds real names.

Adds a ✏️ pencil next to each world's 🗑️ delete button in the world list. Click it (or select a world and press
**R**) to name the world.

- Names are kept by this mod in `BepInEx/config/FabuleosWorldNamer.json`, keyed by each world's ID. The game's own
  (encrypted) save files are never changed.
- Leave the name empty to go back to the map name.
- In the Rename box: **Enter** saves, **Esc** cancels. Up to 24 characters.

## Download

Get **`FabuleosWorldNamer-v1.0.0.zip`** (or the loose `FabuleosWorldNamer.dll`) from the
[Releases page](../../releases/latest).

## Install

1. You need **BepInEx 6 bleeding edge (IL2CPP)** in your Flipping is Hard folder (if you use other FIH mods, you
   already have it): <https://builds.bepinex.dev/projects/bepinex_be>.
2. Unzip `FabuleosWorldNamer-v1.0.0.zip` into the game folder, so `FabuleosWorldNamer.dll` ends up in
   `BepInEx/plugins/`.
3. Start the game and open the world list.

## Build from source

`dotnet build RenameWorlds.csproj -c Release` (needs the .NET 6+ SDK and a game folder with BepInEx installed, for
references; pass `-p:GameDir="/path/to/Flipping is Hard"`).

Not affiliated with the developers of Flipping is Hard. Code: MIT License (see `LICENSE`).
