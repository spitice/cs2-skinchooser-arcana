using System.Text.Json;

namespace SkinChooserArcana.Skins.LupeMG.v1
{
    public static class LupeMGv1
    {

        public static SkinRegistry Load(string filePath)
        {
            using FileStream openStream = File.OpenRead(filePath);
            var playerSkins = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, PlayerModelEntry>>>(openStream);
            return PlayerSkinsToSkinRegistry(playerSkins);
        }

        public static async Task<SkinRegistry> LoadAsync(string filePath)
        {
            using FileStream openStream = File.OpenRead(filePath);
            var playerSkins = await JsonSerializer.DeserializeAsync<Dictionary<string, Dictionary<string, PlayerModelEntry>>>(openStream);
            return PlayerSkinsToSkinRegistry(playerSkins);
        }

        private static SkinRegistry PlayerSkinsToSkinRegistry(Dictionary<string, Dictionary<string, PlayerModelEntry>>? playerSkins)
        {
            var skinReg = new SkinRegistry();

            if (playerSkins == null)
            {
                return skinReg;
            }

            foreach (var skinClass in playerSkins.Values)
            {
                foreach (var modelEntry in skinClass.Values)
                {
                    var skinDesc = modelEntry.ToSkinDescriptor();
                    skinReg.Skins[modelEntry.name] = skinDesc;
                }
            }

            return skinReg;
        }
    }
}
