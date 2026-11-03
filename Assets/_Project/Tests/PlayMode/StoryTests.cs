using System.Collections;
using System.Linq;
using Lumen.Player;
using Lumen.Story;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Lumen.Tests
{
    /// <summary>Giriş koridorundaki hikâye metinleri: sırayla gösterilir, her tetik bir kez çalışır.</summary>
    public class StoryTests
    {
        [UnityTest]
        public IEnumerator WalkingTheCorridor_ShowsEachStoryLineOnce()
        {
            yield return SceneManager.LoadSceneAsync("Lumen");
            yield return null;
            PlayerController player = Object.FindAnyObjectByType<PlayerController>();
            StoryView view = Object.FindAnyObjectByType<StoryView>();
            StoryTrigger[] triggers = Object.FindObjectsByType<StoryTrigger>().OrderBy(t => t.transform.position.z).ToArray();
            Assert.AreEqual(3, triggers.Length, "Koridorda 3 hikâye tetiği olmalı.");
            Assert.IsFalse(view.IsBusy, "Oyun başında metin gösterilmemeli.");

            // İlk tetikten geç: ilk metin ekrana gelir
            player.Teleport(triggers[0].transform.position + Vector3.down * 0.95f);
            yield return new WaitForSeconds(0.2f);
            Assert.IsTrue(triggers[0].Played);
            Assert.IsTrue(view.IsBusy);
            StringAssert.StartsWith("Lumen", view.CurrentText);

            // Geri dönüp yeniden geçmek metni tekrarlamaz; diğer iki tetik sıraya girer
            player.Teleport(new Vector3(0f, 0.05f, -18f));
            yield return new WaitForSeconds(0.1f);
            foreach (StoryTrigger trigger in triggers)
            {
                player.Teleport(trigger.transform.position + Vector3.down * 0.95f);
                yield return new WaitForSeconds(0.1f);
            }

            Assert.IsTrue(triggers.All(t => t.Played), "Üç tetik de çalışmalı.");
        }
    }
}
