using CounterStrikeSharp.API.Modules.Utils;

namespace SkinChooserArcana.Skins
{
    public class SkinDescriptor: IEquatable<SkinDescriptor>
    {
        public required string Id { get; set; }
        public string ModelName { get; set; } = "";

        public HashSet<ulong>? AllowedPlayerIds { get; set; } = null;

        public bool IsPlayerAllowedToUse(ulong id)
        {
            if (AllowedPlayerIds == null)
            {
                // It is a public skin if no player ids are specified
                return true;
            }

            return AllowedPlayerIds.Contains(id);
        }

        public string GetColoredId(ulong id)
        {
            if (AllowedPlayerIds != null)
            {
                if (AllowedPlayerIds.Contains(id))
                {
                    return $"{ChatColors.Gold}{Id}{ChatColors.Default}";
                }
                else
                {

                    return $"{ChatColors.LightRed}{Id}{ChatColors.Default}";
                }
            }
            return Id;
        }

        public bool Equals(SkinDescriptor? other)
        {
            if (other == null)
                return false;

            if (Id != other.Id)
                return false;

            if (ModelName != other.ModelName)
                return false;

            if (AllowedPlayerIds == null)
                return other.AllowedPlayerIds == null;

            if (other.AllowedPlayerIds == null)
                return false;

            if (!AllowedPlayerIds.SetEquals(other.AllowedPlayerIds))
                return false;

            return true;
        }
    }
}
