using System.Collections;
using Lumen.NPC;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Lumen.Tests
{
    /// <summary>Bitiş ekranı: oyun bitince süre ve güven yazılır, ana menü düğmesi görünür.</summary>
    public class EndScreenTests
    {
        [Test]
        public void FormatTime_WritesMinutesAndSeconds()
        {
            Assert.AreEqual("00:00", EndScreen.FormatTime(0f));
            Assert.AreEqual("01:05", EndScreen.FormatTime(65.9f));
            Assert.AreEqual("12:30", EndScreen.FormatTime(750f));
        }

        [UnityTest]
        public IEnumerator GameEnd_ShowsStatsAndMainMenuButton()
        {
            yield return SceneManager.LoadSceneAsync("Lumen");
            yield return null;
            var gameState = Object.FindAnyObjectByType<GameStateManager>();
            var endScreen = Object.FindAnyObjectByType<EndScreen>();

            gameState.EndGame();
            yield return null;

            Text stats = endScreen.transform.Find("Panel/Stats").GetComponent<Text>();
            StringAssert.StartsWith("Süre:", stats.text);
            StringAssert.Contains("Arven'in güveni", stats.text);
            Assert.IsTrue(endScreen.transform.Find("Panel/MainMenu").gameObject.activeInHierarchy, "Ana menü düğmesi görünmeli.");
        }
    }
}
