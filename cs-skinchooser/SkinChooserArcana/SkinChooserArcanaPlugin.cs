using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Extensions;
using CounterStrikeSharp.API.Modules.Utils;
using SkinChooserArcana.IO;
using SkinChooserArcana.Players;
using SkinChooserArcana.Skins;
using SkinChooserArcana.Skins.LupeMG.v1;

namespace SkinChooserArcana
{
    public class SkinChooserArcanaPlugin: BasePlugin, IPluginConfig<SkinChooserArcanaConfig>
    {
        public override string ModuleName => "SkinChooser Arcana";
        public override string ModuleVersion => "1.1.0";
        public override string ModuleAuthor => "Spitice";
        public override string ModuleDescription => "A player model changer plugin for Lupercalia MG server.";

        public required SkinChooserArcanaConfig Config { get; set; }
        private bool _isReloading = false;

#pragma warning disable CS8618
        private Sout _sout;
        private PlayerDatabase _db;
#pragma warning restore CS8618
        private SkinRegistry _skinRegistry = new SkinRegistry();

        private Dictionary<int, string> _playerSlotToOrigModel = new Dictionary<int, string>();

        public readonly FakeConVar<bool> IsModuleEnabled = new("skinchooser_enabled", "SkinChooser is enabled.", true);
        public readonly FakeConVar<float> ConVar_ModelScale = new("skinchooser_modelscale", "The model scale factor for all skins.", 1.0f);
        public readonly FakeConVar<string> ConVar_GameMode = new("skinchooser_gamemode", "The current game mode. Use if skins have alternative skin for the specific game mode.", "");

        public override void Load(bool hotReload)
        {
            _sout = new Sout(Logger, Localizer, "SkinChooser", "SkinChooser", ChatColors.Green);
            _db = new PlayerDatabase(Path.Join(ModuleDirectory, "database_SkinChooserArcana.db"), _sout);

            RegisterListener<Listeners.OnServerPrecacheResources>(OnServerPrecacheResources);
            RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);

            IsModuleEnabled.ValueChanged += (sender, value) =>
            {
                Console.WriteLine($"SkinChooser {value}");
                if (value)
                {
                    EnableSkinInRound();
                }
                else
                {
                    DisableSkinInRound();
                }
            };

            ConVar_ModelScale.ValueChanged += (sender, value) =>
            {
                foreach (var player in Utilities.GetPlayers())
                {
                    if (!player.IsValid)
                        continue;

                    if (!player.PawnIsAlive)
                        continue;

                    var pawn = player.Pawn.Get();

                    if (pawn == null)
                        continue;

                    var skeleton = pawn.CBodyComponent?.SceneNode?.GetSkeletonInstance();

                    if (skeleton == null)
                        continue;

                    skeleton.Scale = value;
                    Utilities.SetStateChanged(pawn, "CBaseEntity", "m_CBodyComponent");
                }
            };

            ConVar_GameMode.ValueChanged += (sender, value) =>
            {
                DisableSkinInRound();
                EnableSkinInRound();
            };
        }

        public override void Unload(bool hotReload)
        {

        }

        public void OnConfigParsed(SkinChooserArcanaConfig config)
        {
            Config = config;
            _skinRegistry = LupeMGv1.Load(Config.SkinsDataFullPath);
        }

        public void OnServerPrecacheResources(ResourceManifest manifest)
        {
            foreach (var skin in _skinRegistry.Skins.Values)
            {
                manifest.AddResource(skin.ModelName);
            }
        }

        private HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
        {
            if (!IsModuleEnabled.Value)
            {
                return HookResult.Continue;
            }

            var player = @event.Userid;
            if (player == null)
            {
                return HookResult.Continue;
            }

            Server.NextFrame(() =>
            {
                ApplySkinForPlayer(player);
            });

            return HookResult.Continue;
        }

        [ConsoleCommand("css_skinreload", "Reloads player skins.")]
        [RequiresPermissions("@css/root")]
        public void OnReloadSkins(CCSPlayerController? player, CommandInfo commandInfo)
        {
            Config.Reload();

            if (_isReloading)
            {
                // Already reloading.
                _sout.ToPlayer(player, "Reload.AlreadyReloading");
                return;
            }

            _isReloading = true;

            Server.NextFrameAsync(async () =>
            {
                var newReg = await LupeMGv1.LoadAsync(Config.SkinsDataFullPath);

                // Summarize the reloaded skin data.
                var diff = SkinRegistry.GenerateSkinRegistryDiff(_skinRegistry, newReg);
                _sout.ToPlayer(player, "Reload.CountDiff", diff.OldSkinCount, diff.NewSkinCount);

                if (diff.AddedSkins.Count > 0)
                {
                    _sout.ToPlayer(player, "Reload.SkinsAdded", String.Join(", ", diff.AddedSkins));
                }
                else
                {
                    _sout.ToPlayer(player, "Reload.NoSkinsAdded");
                }

                if (diff.RemovedSkins.Count > 0)
                {
                    _sout.ToPlayer(player, "Reload.SkinsRemoved", String.Join(", ", diff.RemovedSkins));
                }
                else
                {
                    _sout.ToPlayer(player, "Reload.NoSkinsRemoved");
                }

                if (diff.ModifiedSkins.Count > 0)
                {
                    _sout.ToPlayer(player, "Reload.SkinsModified", String.Join(", ", diff.ModifiedSkins));
                }
                else
                {
                    _sout.ToPlayer(player, "Reload.NoSkinsModified");
                }

                _skinRegistry = newReg;

                _isReloading = false;

                _sout.ToPlayer(player, "Reload.Completed");
                _sout.Log("Skin data has been reloaded.");
            });
        }

        [ConsoleCommand("css_skin", "Change player skin.")]
        public void OnSkin(CCSPlayerController? player, CommandInfo commandInfo)
        {
            if (player == null)
            {
                _sout.ToPlayer(player, "Skin.ClientOnly");
                return;
            }

            if (commandInfo.ArgCount == 1)
            {
                var currentSkinName = _db.GetSkinForPlayer(player.SteamID);
                if (currentSkinName == "")
                {
                    _sout.ToPlayer(player, "Skin.CurrentSkinDefalt");
                }
                else
                {
                    _sout.ToPlayer(player, "Skin.CurrentSkin", currentSkinName);
                    _sout.ToPlayer(player, "Skin.HowToReset");
                }
                return;
            }

            var skinName = commandInfo.GetArg(1).ToLower();
            if (skinName == "default")
            {
                skinName = "";
            }

            SetPlayerSkin(player, skinName);
        }

        [ConsoleCommand("css_skins", "View the list of skins.")]
        public void OnSkins(CCSPlayerController? player, CommandInfo commandInfo)
        {
            if (player == null)
            {
                return;
            }

            _sout.ToPlayer(player, "SkinList.AllSkins", "");

            var skinNames = _skinRegistry.Skins.Values
                .OrderBy(skin => skin.Id)
                .Select(skin => skin.GetColoredId(player.SteamID))
                .ToList();

            var separator = $" {ChatColors.Grey} / {ChatColors.Default} ";
            var step = 20;

            for (var i = 0; i < skinNames.Count; i += step)
            {
                var endIndex = Math.Min(i + step, skinNames.Count);
                var range = new Range(i, endIndex);
                var partialSkinNames = skinNames.Take(range);
                var text = String.Join(separator, partialSkinNames);
                if (endIndex != skinNames.Count)
                {
                    text += separator;
                }
                _sout.ToPlayerWithoutPrefix(player, text);
            }
        }

        private void SetPlayerSkin(CCSPlayerController player, string skinName)
        {
            if (skinName == "")
            {
                skinName = Config.DefaultSkin;
            }

            var skin = _skinRegistry.Skins.GetValueOrDefault(skinName);
            if (skin == null)
            {
                _sout.ToPlayer(player, "Skin.NotFound", skinName);
                return;
            }

            if (!skin.IsPlayerAllowedToUse(player.SteamID))
            {
                _sout.ToPlayer(player, "Skin.NotAllowed", skinName);
                return;
            }

            _sout.ToPlayer(player, "Skin.Updated", skinName);

            _db.SetSkinForPlayer(player.SteamID, skinName);

            ApplyPlayerSkinModelToPawn(player, skinName);
        }

        private string? ApplyPlayerSkinModelToPawn(CCSPlayerController player, string skinName, bool checkGameMode = true)
        {
            if (skinName == "")
            {
                skinName = Config.DefaultSkin;
            }

            var skin = _skinRegistry.Skins.GetValueOrDefault(skinName);
            if (skin == null)
            {
                return null;
            }

            // Check if the skin has an alternative for the current game mode
            if (checkGameMode)
            {
                var alternativeSkinName = "";
                if (skin.GameModes.TryGetValue(ConVar_GameMode.Value, out alternativeSkinName))
                {
                    _sout.ToPlayer(player, "Skin.UsingAlternative", skinName, alternativeSkinName);
                    return ApplyPlayerSkinModelToPawn(player, alternativeSkinName, false);
                }
            }

            var pawn = player.PlayerPawn.Get();
            if (pawn == null)
            {
                return null;
            }

            var skeleton = pawn.CBodyComponent?.SceneNode?.GetSkeletonInstance();
            if (skeleton == null)
            {
                return null;
            }

            if (skeleton.Scale != ConVar_ModelScale.Value)
            {
                skeleton.Scale = ConVar_ModelScale.Value;
            }

            var oldModelName = skeleton.ModelState.ModelName;
            if (oldModelName == skin.ModelName)
            {
                // Not changed
                oldModelName = null;
            }
            else
            { 
                pawn.SetModel(skin.ModelName);
            }

            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_CBodyComponent");
            

            return oldModelName;
        }

        private void ApplySkinForPlayer(CCSPlayerController player)
        {
            var skinName = _db.GetSkinForPlayer(player.SteamID);
            var oldModelName = ApplyPlayerSkinModelToPawn(player, skinName);
            if (oldModelName != null)
            {
                _playerSlotToOrigModel[player.Slot] = oldModelName;
            }
        }

        private void EnableSkinInRound()
        {
            foreach (var player in Utilities.GetPlayers())
            {
                ApplySkinForPlayer(player);
            }
        }

        private void DisableSkinInRound()
        {
            foreach (var player in Utilities.GetPlayers())
            {
                var oldModelName = _playerSlotToOrigModel.GetValueOrDefault(player.Slot);
                if (oldModelName == null)
                {
                    continue;
                }

                player.PlayerPawn.Get()?.SetModel(oldModelName);
            }
        }
    }
}
