namespace SkinChooserArcana.Skins
{
    public class SkinRegistryDiff
    {
        public int OldSkinCount = 0;
        public int NewSkinCount = 0;
        public List<string> AddedSkins = new List<string>();
        public List<string> RemovedSkins = new List<string>();
        public List<string> ModifiedSkins = new List<string>();
    }
}
