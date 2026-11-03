namespace Lumen.EditorTools.SceneBuilding.Sections
{
    /// <summary>Geliştirici 2 — oyun sahnesinin arka plan müziği (gerilimli, döngüde). Müzik MusicGenerator ile kodla üretilir.</summary>
    public sealed class MusicSection : ISceneSection
    {
        public int Order => 41;

        public void Build(SceneBuildContext ctx)
        {
            MusicGenerator.AddMusic("Music", MusicGenerator.GameMusic(), 0.45f);
        }
    }
}
