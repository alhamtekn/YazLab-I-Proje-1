using System.Collections;
using Lumen.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Lumen.Tests
{
    /// <summary>Nişangah etkileşilebilir nesne yakındayken parlar; duraklatma menüsündeki "Yeniden Başla" oyunu baştan yükler.</summary>
    public class CrosshairAndRestartTests
    {
        [UnityTest]
        public IEnumerator Crosshair_HighlightsNearInteractable()
        {
            yield return SceneManager.LoadSceneAsync("Lumen");
            yield return null;
            var crosshair = Object.FindAnyObjectByType<CrosshairView>();
            var player = Object.FindAnyObjectByType<PlayerController>();
            Assert.IsNotNull(crosshair, "Sahnede nişangah yok.");

            player.Teleport(new Vector3(0f, 0.05f, 45f)); // Arven'den uzak, etrafta etkileşim yok
            yield return null;
            yield return null;
            Assert.IsFalse(crosshair.IsHighlighted);

            player.Teleport(new Vector3(-3f, 0.05f, 47.8f)); // Arven'in önü
            yield return null;
            yield return null;
            Assert.IsTrue(crosshair.IsHighlighted, "Arven'in yanında nişangah parlamalı.");
        }

        [UnityTest]
        public IEnumerator RestartButton_ReloadsTheGame()
        {
            yield return SceneManager.LoadSceneAsync("Lumen");
            yield return null;
            var player = Object.FindAnyObjectByType<PlayerController>();
            Vector3 start = player.transform.position;
            player.Teleport(new Vector3(0f, 0.05f, 30f));
            yield return null;

            Button restart = GameObject.Find("PauseMenu").transform.Find("Dim/Panel/Restart").GetComponent<Button>();
            restart.onClick.Invoke();
            yield return null;
            yield return null;

            player = Object.FindAnyObjectByType<PlayerController>();
            Assert.Less(Vector3.Distance(start, player.transform.position), 0.5f, "Yeniden başlayınca oyuncu başlangıçta olmalı.");
            Assert.AreEqual(1f, Time.timeScale, "Yeniden başlayınca oyun duraklatılmış kalmamalı.");
        }
    }
}
