# Balls War

A real-time auto-battle strategy simulation game for Windows.

## Overview

- **Area A (Pinball Arena)**: Balls from multiple factions bounce through a shared pachinko-style arena. Multiplier zones increase ball values, and conversion zones at the bottom trigger effects in Area B.
- **Area B (Battle Grid)**: A large grid battlefield where factions fight for territory. Shotgun pellets, big balls, and shields determine the outcome.

All factions play simultaneously with no player control — only simulation speed can be adjusted.

## Requirements

- Windows 10/11 x64
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (for building)

## Build & Run

```powershell
dotnet run --project src/BallsWar.App
```

## Publish (standalone .exe)

```powershell
dotnet publish src/BallsWar.App/BallsWar.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish
```

The output `publish/BallsWar.App.exe` is a single-file executable with no dependencies.

## Controls

| Key | Action |
|-----|--------|
| `Space` | Pause / Resume |
| `1-6` | Set simulation speed |
| `Q/E` | Decrease / Increase speed |
| `ESC` | Return to setup |
| `R` | Restart after game over |

## Setup Screen

Configure factions, balls, grid size, health, shotgun parameters and more before starting. Settings are persisted to `balls-war-config.json`.

## Tech Stack

- C# / .NET 9
- [Raylib-cs](https://github.com/ChrisDill/Raylib-cs) for rendering
- [Aether.Physics2D](https://github.com/nkast/Aether.Physics2D) for Area A physics

## License

MIT
