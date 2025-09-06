using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace SkinChooserArcana
{
    public class SkinChooserArcanaConfig: BasePluginConfig
    {
        [JsonPropertyName("SkinsDataPath")]
        public string SkinsDataPath { get; set; } = "PlayerSkins.json";

        [JsonPropertyName("DefaultSkin")]
        public string DefaultSkin { get; set; } = "";

        [JsonIgnore]
        public string SkinsDataFullPath {
            get
            {
                return Path.Combine(Server.GameDirectory, "csgo", "addons", "counterstrikesharp", "configs", "plugins", "SkinChooserArcana", SkinsDataPath);
            }
        }
    }
}
