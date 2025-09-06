namespace SkinChooserArcana.Skins
{
    public class SkinRegistry
    {
        public Dictionary<string, SkinDescriptor> Skins { get; set; } = new Dictionary<string, SkinDescriptor>();

        public static SkinRegistryDiff GenerateSkinRegistryDiff(SkinRegistry oldReg, SkinRegistry newReg)
        {
            var diff = new SkinRegistryDiff();
            diff.OldSkinCount = oldReg.Skins.Count;
            diff.NewSkinCount = newReg.Skins.Count;
            diff.AddedSkins = newReg.Skins.Keys.Except(oldReg.Skins.Keys).Order().ToList();
            diff.RemovedSkins = oldReg.Skins.Keys.Except(newReg.Skins.Keys).Order().ToList();
            diff.ModifiedSkins = newReg.Skins.Keys.Intersect(oldReg.Skins.Keys)
                .Where(key => !newReg.Skins[key].Equals(oldReg.Skins[key]))
                .Order()
                .ToList();

            return diff;
        }
    }
}
