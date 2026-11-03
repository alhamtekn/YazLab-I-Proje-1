using System.Collections;
using Lumen.Audio;
using Lumen.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Lumen.Tests
{
    /// <summary>Müzik ve efektler: kutsal bölüme girince müzik gergin parçaya geçer, efekt bileşeni sahnede hazırdır.</summary>
    public class MusicTests
    {
        [UnityTest]
        public IEnumerator EnteringSanctum_CrossfadesToTenseMusic_AndBack()
        {
            yield return SceneManager.LoadSceneAsync("Lumen");
            yield return null;
            MusicZone zone = Object.FindAnyObjectByType<MusicZone>();
            PlayerController player = Object.FindAnyObjectByType<PlayerController>();
            Assert.IsNotNull(zone, "Sahnede müzik geçişi yok.");
            Assert.AreEqual(0f, zone.Blend, "Oyun normal müzikle başlamalı.");

            player.Teleport(new Vector3(0f, 0.05f, 62f)); // kutsal bölüm
            yield return new WaitForSeconds(3.5f);
            Assert.AreEqual(1f, zone.Blend, 0.01f, "Kutsal bölümde gergin müzik çalmalı.");

            player.Teleport(new Vector3(0f, 0.05f, 45f)); // Arven'in bölgesi
            yield return new WaitForSeconds(3.5f);
            Assert.AreEqual(0f, zone.Blend, 0.01f, "Bölümden çıkınca normal müziğe dönmeli.");
        }

        [UnityTest]
        public IEnumerator SoundEffects_AreReady()
        {
            yield return SceneManager.LoadSceneAsync("Lumen");
            GameAudio audio = Object.FindAnyObjectByType<GameAudio>();
            Assert.IsNotNull(audio, "Sahnede ses efektleri bileşeni yok.");
            Assert.DoesNotThrow(audio.PlayChime);
            Assert.DoesNotThrow(audio.PlayRumble);
            Assert.DoesNotThrow(audio.PlayHit);
        }
    }
}
