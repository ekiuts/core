# KenshiCore

A .NET 10 library for reading, writing, and reverse-engineering [Kenshi](https://kenshi.fandom.com/) `.mod` files.

This repository hosts the core shared dependency used by the other Kenshi tooling projects:
[KenshiFixer](https://github.com/Kakrain/KenshiFixer), [KenshiPatcher](https://github.com/Kakrain/KenshiPatcher), and
[KenshiUtilities](https://github.com/Kakrain/KenshiUtilities).

## Projects

The repository contains two projects:

| Project         | Target           | Purpose                                                                                                                  |
| --------------- | ---------------- | ------------------------------------------------------------------------------------------------------------------------ |
| `KenshiCore`    | `net10.0`         | UI-free core: `.mod` file parsing/writing, Ogre mesh & skeleton readers, mod/record models, repositories, and utilities. |
| `KenshiCore.UI` | `net10.0-windows` | WinForms layer: shared base form, log window, theming, progress, and mod icons. Depends on `KenshiCore`.                 |

Splitting the UI into a separate project keeps the core platform-agnostic, so it can be used headless and unit-tested without WinForms.

## Features

- Read and write `.mod` and `.base` files (supports file types 16 & 17).
- Parse and mutate mod records: bool, float, int, string, filename, vec3/vec4, extra-data, and instance fields.
- Resolve mod dependencies/references and load order.
- Ogre `.mesh` and `.skeleton` readers (vertex data, skeleton links, bone influence, intersection tests).
- Detect and manage Steam Workshop vs. game-directory mods.
- Configuration via `kenshiAppConfig.json`.

## Requirements

- .NET 10 SDK (or Desktop Runtime, x64, to run the WinForms UI).
- Microsoft Visual C++ 20xx Redistributable (x64) for GUI applications.

## Building

```sh
dotnet build KenshiCore.sln
```

To build the Windows-only UI/consumer projects on a non-Windows host:

```sh
dotnet build KenshiCore.sln -p:EnableWindowsTargeting=true
```

## Usage

Create the composition root (`KenshiServices`) once at startup and inject it into your forms/consumers:

```csharp
using KenshiCore;

var services = new KenshiServices();   // resolves Kenshi install paths, wires repositories
services.LoadMods();

// parse a mod
var re = new ReverseEngineer();
re.LoadModFile("path/to/mod.mod");
```

`ModManager`, `ModRepository`, `ReverseEngineerRepository`, and `FileAnalyzer` are instance-based
and available through `KenshiServices`. A `Current` service-locator bridge is provided for helper
classes that are not wired for constructor injection.

## Credits

**KenshiCore** was originally created and developed by **Kakrain** ([@Kakrain](https://github.com/Kakrain)).

It is maintained by **ekiuts** ([@ekiuts](https://github.com/ekiuts)), who restructured the codebase into
the current split-core / WinForms-UI layout.

## License

Released under the [MIT License](LICENSE.md). Copyright © 2026 **Ekiuts**.
