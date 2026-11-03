using Lumen.Audio;
using Lumen.Player;
using UnityEngine;

namespace Lumen.EditorTools.SceneBuilding.Sections
{
    /// <summary>
    /// Geliştirici 2 — oyun sahnesinin sesi: gerilimli arka plan müziği, kutsal bölümde daha gergin parçaya geçiş
    /// ve olaylara bağlı kısa efektler. Müzik ve efektler MusicGenerator ile kodla üretilir.
    /// </summary>
    public sealed class MusicSection : ISceneSection
    {
        private const float MusicVolume = 0.45f;

        public int Order => 41;

        public void Build(SceneBuildContext ctx)
        {
            AudioSource normal = MusicGenerator.AddMusic("Music", MusicGenerator.GameMusic(), MusicVolume);
            AudioSource tense = MusicGenerator.AddMusic("Music_Sanctum", MusicGenerator.SanctumMusic(), 0f);
            normal.gameObject.AddComponent<MusicZone>().Configure(ctx.Get<PlayerController>().transform, normal, tense, SanctumSection.StartZ);

            var sfx = new GameObject("SoundEffects");
            AudioSource source = sfx.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f; // 2D: her yerden aynı duyulur
            sfx.AddComponent<GameAudio>().Configure(MusicGenerator.ChimeSfx(), MusicGenerator.RumbleSfx(), MusicGenerator.HitSfx());
        }
    }
}
