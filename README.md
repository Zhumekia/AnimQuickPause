# Anim Pause

A small BepInEx plugin for **Koikatsu CharaStudio** that pauses and resumes character animations with a hotkey, instead of dragging the Speed slider in the Animation Controller.

Press the hotkey and the animation speed goes to 0 instantly. Press it again and each character returns to the speed it had before.

## Features

- Pause or resume the **selected** characters with one key
- Pause or resume **all** characters in the scene with a second key
- Each character remembers its own previous speed
- The Animation Controller's Speed slider is refreshed so it shows the current value
- Both hotkeys can be changed in-game

## Hotkeys

| Action | Default | Config name |
| --- | --- | --- |
| Pause / resume selected characters | Right Ctrl + P | `Pause selected hotkey` |
| Pause / resume all characters | none (set your own) | `Pause all hotkey` |

Change them in-game with **F1 -> Plugin Settings -> Anim Pause**, or edit `BepInEx/config/<plugin GUID>.cfg`.

"Pause all" pauses everyone if any character is still playing. If every character is already paused, it resumes everyone.

## Requirements

- Koikatsu (KK) with CharaStudio
- [BepInEx](https://github.com/BepInEx/BepInEx)
- ConfigurationManager (for rebinding keys with F1)

## Installation

1. Download `AnimPause.dll` from the Releases page.
2. Put it in `BepInEx/plugins` in your Koikatsu folder.
3. Start CharaStudio.

## Building from source

1. Clone this repository.
2. Open the `.sln` in Visual Studio.
3. Make sure the game and BepInEx references resolve (this project is based on [IllusionMods/PluginTemplate](https://github.com/IllusionMods/PluginTemplate)).
4. Build, then copy the DLL to `BepInEx/plugins`.

The whole plugin is a single file: `AnimPausePlugin.cs`.

## Notes

- Tested on: [Koikatsu]
- CharaStudio only.

## License

MIT. See [LICENSE](LICENSE).
