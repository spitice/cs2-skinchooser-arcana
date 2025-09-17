# CS2 SkinChooser Arcana

Skin (custom player model) selector plugin for Lupercalia MG server written in CSSharp.

Before you use this plugin, see [Related projects](#related-projects) section first.

## Installation

- PREREQUISITES:
	- [CounterStrikeSharp](https://docs.cssharp.dev/index.html)
- Download the [latest SkinChooser Arcana release](https://github.com/spitice/cs2-skinchooser-arcana/releases)
- Copy/move `addons` folder to the server's `csgo` directory

## Defining custom skins

- Rename `addons/counterstrikesharp/configs/plugins/SkinChooserArcana/PlayerSkins.example.json` to `PlayerSkins.json`
- Edit `PlayerSkins.json` file to add your custom skins
	- NOTE: The plugin uses `ModelCT` for both sides (i.e., CT and T). You cannot specify which team can use the skin.
- You can specify the default skin in `SkinChooserArcana.json`
	- See `SkinChooserArcana.example.json` for reference
	- If it's not specified or just an empty string, the plugin won't change the player skin by default.

## Commands and ConVars

- `!skin <skin name>`
	- Set skins
- `!skin`
	- Revert to the default skin
- `!skins`
	- Shows all skin names available in the server.
	- RED🔴: Requires permission to use the skin.
	- GOLD🟡: Requires permission to use the skin, and you have the permission.

Admins (`@css/root`) only

- `!skinreload`
	- Reloads `PlayerSkins.json` on the fly

ConVars

- `skinchooser_enabled 1`
	- Enables/disables SkinChooser
	- You can change this ConVar during the round so the player models will be immediately swapped.
- `skinchooser_modelscale 1.0`
	- Changed player model scale

## Database

Player's skin preferences will be saved to `addons/counterstrikesharp/plugins/SkinChooserArcana/database_SkinChooserArcana.db` as a SQLite database.

## Acknowledgements

This plugin is meant to be a port of SkinChooser feature implemented in [Zeisen's CS2Fixes fork](https://github.com/LupercaliaMG/CS2Fixes).
Massive thanks to Zeisen for making and maintaining the fork for Lupercalia MG server all the time.

## Related projects

If you want a more sophisticated plugin for custom skins, I recommend the following plugin:

- [CS2-PlayerModelChanger](https://github.com/samyycX/CS2-PlayerModelChanger) by samyycX

## Changelogs

#### v1.0.0 (25-09-17)

- Initial release