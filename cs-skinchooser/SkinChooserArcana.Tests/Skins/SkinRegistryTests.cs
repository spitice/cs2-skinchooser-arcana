using SkinChooserArcana.Skins;

namespace SkinChooserArcana.Tests.Skins
{
    public class SkinRegistryTests
    {
        [Fact]
        public void ShouldGenerateRegistryDiff()
        {
            var oldReg = new SkinRegistry();
            var newReg = new SkinRegistry();

            oldReg.Skins["foo"] = new SkinDescriptor() { Id = "foo" };
            oldReg.Skins["bar"] = new SkinDescriptor() { Id = "bar" };
            oldReg.Skins["baz"] = new SkinDescriptor() { Id = "baz" };

            newReg.Skins["foo"] = new SkinDescriptor() { Id = "foo", ModelName = "foofoo" };
            newReg.Skins["bar"] = new SkinDescriptor() { Id = "bar" };
            newReg.Skins["qux"] = new SkinDescriptor() { Id = "qux" };
            newReg.Skins["quux"] = new SkinDescriptor() { Id = "quux" };

            var diff = SkinRegistry.GenerateSkinRegistryDiff(oldReg, newReg);

            Assert.Equal(3, diff.OldSkinCount);
            Assert.Equal(4, diff.NewSkinCount);
            Assert.True(diff.AddedSkins.SequenceEqual(["quux", "qux"]));
            Assert.True(diff.RemovedSkins.SequenceEqual(["baz"]));
            Assert.True(diff.ModifiedSkins.SequenceEqual(["foo"]));
        }
    }
}