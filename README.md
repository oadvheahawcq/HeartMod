# HeartMod — 腐化之心 / The Corrupted Heart

A **Slay the Spire 2** character mod built on [BaseLib](https://github.com/Alchyr/BaseLib).

一个《杀戮尖塔 2》的角色 mod，基于 [BaseLib](https://github.com/Alchyr/BaseLib) 制作。

- **Character / 角色**：腐化之心 (The Corrupted Heart) — 千年前的尖塔主人，使用诅咒的力量进行战斗。
- **Author / 作者**：Meiosis
- **Mod version**：`v0.0.0`
- **Min game version**：`0.111.0`
- **Requires / 前置**：BaseLib `>= 3.4.7`

## Contents / 内容

| Category | Count |
| --- | --- |
| Cards / 卡牌 | 182 localization entries |
| Powers / 能力 | 105 |
| Relics / 遗物 | 25 |
| Potions / 药水 | 6 |
| Languages / 语言 | English, 日本語, 简体中文 |

## Installation / 安装

1. Install [BaseLib](https://github.com/Alchyr/BaseLib) into your `Slay the Spire 2/mods/` folder.
   / 先把 BaseLib 装到游戏的 `mods/` 目录。
2. Download `HeartMod.zip` (or `HeartMod.dll` + `HeartMod.json` + `HeartMod.pck`) from [Releases](../../releases).
   / 从 Releases 下载发布包。
3. Extract into `Slay the Spire 2/mods/HeartMod/` so that `HeartMod.json` sits directly inside that folder.
   / 解压到 `mods/HeartMod/`，确保 `HeartMod.json` 就在该目录下。

## Building from source / 从源码构建

**Requirements / 环境**：.NET 9 SDK, Godot 4.5.1 **mono** (for packaging), a local Slay the Spire 2 install.

```bash
git clone https://github.com/<your-name>/HeartMod.git
cd HeartMod
dotnet build
```

The build copies `HeartMod.dll`, `HeartMod.json` and the `.pdb` into your game's `mods/HeartMod/` folder.
`Sts2PathDiscovery.props` locates the game automatically (registry / Steam library); override with a
`local.props` file if auto-detection fails:

```xml
<Project>
  <PropertyGroup>
    <Sts2Path>D:/SteamLibrary/steamapps/common/Slay the Spire 2</Sts2Path>
  </PropertyGroup>
</Project>
```

`Directory.Build.props` is **not** committed because it holds a machine-specific path to the Godot
executable. Create it next to `HeartMod.csproj` before publishing:

```xml
<Project>
  <PropertyGroup>
    <GodotPath>C:/path/to/Godot_v4.5.1-stable_mono_win64.exe</GodotPath>
  </PropertyGroup>
</Project>
```

Then `dotnet publish` exports `HeartMod.pck` into the mods folder.

> 注意：`Directory.Build.props` 里记录的是本机 Godot 路径，因此没有提交到仓库，需要自己新建。
> 只做 `dotnet build`（编译 dll）时它不影响；要导出 `.pck`（`dotnet publish`）才必须填。

## Project layout / 目录结构

```
HeartModCode/     C# source: Cards, Character, Powers, Relics, Potions, Enchantments, Patches, ...
HeartMod/         Godot assets: images, animations, audio, scenes, localization
build/            exported mod package (git-ignored, published via Releases)
HeartMod.json     mod manifest
project.godot     Godot project file
```

## License / 许可

Not specified yet — add a `LICENSE` file if you want others to reuse the code or art.
/ 尚未声明许可证。若要允许他人复用代码或美术资源，请补一个 `LICENSE` 文件。

Slay the Spire 2 and its assets belong to Mega Crit. This mod is unofficial and not affiliated
with Mega Crit.
