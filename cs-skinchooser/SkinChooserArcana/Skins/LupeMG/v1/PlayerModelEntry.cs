namespace SkinChooserArcana.Skins.LupeMG.v1
{
    public class PlayerModelEntry
    {
        public required string name { get; set; }

        public required string ModelCT { get; set; }

        public Dictionary<string, string>? gamemodes { get; set; }

        public string? steamid { get; set; }

        public SkinDescriptor ToSkinDescriptor()
        {
            HashSet<ulong>? allowedPlayerIds = null;
            if (steamid != null && steamid.Length > 0)
            {
                allowedPlayerIds = new HashSet<ulong>();
                foreach (var elem in steamid.Split(","))
                {
                    allowedPlayerIds.Add(ulong.Parse(elem));
                }
            }

            return new SkinDescriptor()
            {
                Id = name,
                ModelName = ModelCT,
                AllowedPlayerIds = allowedPlayerIds,
                GameModes = gamemodes ?? new Dictionary<string, string>(),
            };
        }
    }
}
