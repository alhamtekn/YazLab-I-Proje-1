using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Lumen.Companion;
using Lumen.Level;
using Lumen.NPC;
using Lumen.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Lumen.Tests
{
    /// <summary>
    /// Kaybetme durumları: lav, Lumi'ye özel tuzak odası, Arven'in öfkesi ve "Yeniden Oyna".
    /// </summary>
    public class GameOverTests
    {
        private const string SceneName = "Lumen";
        private static readonly Vector3 TrapRoom = new Vector3(-13f, 0.05f, 8f);

        [UnityTest]
        public IEnumerator WalkingOffBridgeEdge_IntoLava_LosesTheGame()
        {
            yield return LoadScene();
            var player = Object.FindAnyObjectByType<PlayerController>();
            var gameState = Object.FindAnyObjectByType<GameStateManager>();
            var screen = Object.FindAnyObjectByType<GameOverScreen>();

            Assert.IsNull(GameObject.Find("EdgeBarrier_L"), "Köprü kenarındaki görünmez duvar kaldırılmalı.");
            Assert.IsFalse(Physics.Raycast(new Vector3(-4f, 1f, 15f), Vector3.forward, 2f, ~0, QueryTriggerInteraction.Ignore),
                "Köprünün yanında, uçurumun kenarında oyuncuyu durduran bir şey olmamalı.");

            // Kenarın hemen ötesi: oyuncu boşluğa adım atmış gibi
            player.Teleport(new Vector3(-4f, 0.05f, 16.6f));
            yield return WaitUntilOrTimeout(() => gameState.IsLost, 4f);

            Assert.IsTrue(gameState.IsLost, "Lava düşen oyuncu kaybetmeli.");
            yield return new WaitForSeconds(1f);
            Assert.IsTrue(screen.IsOpen, "Şansını kaybettin ekranı açılmalı.");
            Assert.AreEqual("Lava düştün.", screen.Reason);
            Assert.IsTrue(screen.RetryButton.gameObject.activeInHierarchy, "Yeniden Oyna görünmeli.");
        }

        [UnityTest]
        public IEnumerator TrapRoom_LosesForPlayer_ButNotForLumi()
        {
            yield return LoadScene();
            var player = Object.FindAnyObjectByType<PlayerController>();
            var companion = Object.FindAnyObjectByType<CompanionController>();
            var gameState = Object.FindAnyObjectByType<GameStateManager>();

            companion.PlaceAt(new Vector3(TrapRoom.x, 0f, TrapRoom.z));
            yield return new WaitForSeconds(0.5f);
            Assert.IsFalse(gameState.IsLost, "Lumi kendi parkurunda kaybettirmemeli.");

            player.Teleport(TrapRoom);
            yield return WaitUntilOrTimeout(() => gameState.IsLost, 2f);
            Assert.IsTrue(gameState.IsLost, "Tuzak odasına giren oyuncu kaybetmeli.");
            Assert.AreEqual("Bu yol yalnızca Lumi'ye ait.", Object.FindAnyObjectByType<GameOverScreen>().Reason);
        }

        [UnityTest]
        public IEnumerator ArvenAttacksThreeTimes_LosesTheGame()
        {
            yield return LoadScene();
            var gameState = Object.FindAnyObjectByType<GameStateManager>();
            var dialogue = Object.FindAnyObjectByType<ArvenDialogue>();
            MethodInfo submit = UseFakeModel(dialogue,
                "{\"dialogue\": \"Defol!\", \"mood\": \"angry\", \"trustDelta\": -5, \"action\": \"attack\", \"reason\": \"test\"}");

            for (int i = 1; i <= 2; i++)
            {
                submit.Invoke(dialogue, new object[] { "Çekil önümden taş yığını." });
                yield return WaitUntilOrTimeout(() => gameState.Attacks == i, 2f);
                Assert.IsFalse(gameState.IsLost, $"{i}. saldırıda oyun kaybedilmemeli, oyuncu yalnızca geri atılmalı.");
                yield return new WaitForSeconds(0.5f);
            }

            submit.Invoke(dialogue, new object[] { "Çekil önümden taş yığını." });
            yield return WaitUntilOrTimeout(() => gameState.IsLost, 2f);
            Assert.AreEqual(3, gameState.Attacks);
            Assert.IsTrue(gameState.IsLost, "Üçüncü saldırıda oyun kaybedilmeli.");
            Assert.AreEqual("Arven'i fazla kızdırdın.", Object.FindAnyObjectByType<GameOverScreen>().Reason);
        }

        [UnityTest]
        public IEnumerator RetryButton_ReloadsSceneWithNormalTime()
        {
            yield return LoadScene();
            var gameState = Object.FindAnyObjectByType<GameStateManager>();
            var screen = Object.FindAnyObjectByType<GameOverScreen>();
            gameState.Lose("test");
            yield return null;
            Time.timeScale = 0.5f;

            screen.RetryButton.onClick.Invoke();
            yield return null;
            yield return null;

            var reloaded = Object.FindAnyObjectByType<GameStateManager>();
            Assert.IsNotNull(reloaded);
            Assert.AreNotSame(gameState, reloaded, "Sahne baştan yüklenmeli.");
            Assert.IsFalse(reloaded.IsLost);
            Assert.IsFalse(Object.FindAnyObjectByType<GameOverScreen>().IsOpen);
            Assert.AreEqual(1f, Time.timeScale);
        }

        private static MethodInfo UseFakeModel(ArvenDialogue dialogue, string reply)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            FieldInfo llmField = typeof(ArvenDialogue).GetField("llm", flags);
            MethodInfo submit = typeof(ArvenDialogue).GetMethod("OnSubmitted", flags);
            Assert.IsNotNull(llmField, "ArvenDialogue.llm alanı bulunamadı; test güncellenmeli.");
            Assert.IsNotNull(submit, "ArvenDialogue.OnSubmitted metodu bulunamadı; test güncellenmeli.");
            llmField.SetValue(dialogue, new FixedReply(reply));
            return submit;
        }

        /// <summary>Her soruya aynı JSON cevabı veren sahte model.</summary>
        private sealed class FixedReply : ILLMService
        {
            private readonly string reply;

            public FixedReply(string fixedReply) => reply = fixedReply;

            public Awaitable<string> SendAsync(IReadOnlyList<ChatMessage> messages)
            {
                var source = new AwaitableCompletionSource<string>();
                source.SetResult(reply);
                return source.Awaitable;
            }
        }

        private static IEnumerator LoadScene()
        {
            Time.timeScale = 1f;
            AsyncOperation load = SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            while (!load.isDone)
            {
                yield return null;
            }

            yield return null;
            yield return null;
        }

        private static IEnumerator WaitUntilOrTimeout(System.Func<bool> condition, float timeout)
        {
            float start = Time.time;
            while (!condition() && Time.time - start < timeout)
            {
                yield return null;
            }
        }
    }
}
