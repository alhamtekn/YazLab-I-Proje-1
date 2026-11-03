using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    /// Oyunun baştan sona akışı: Lumi şaltere gider → köprü iner → 3 kristal odası → kristal kapısı açılır →
    /// 3 yazıt okunur → Arven ikna edilir → Arven'in kapısı açılır → Lumi oyuncuya yetişir → tapınağın kalbi → oyun biter.
    /// Ollama'ya gidilmez: Arven'e sabit, saygılı bir cevap veren sahte bir dil modeli bağlanır; böylece test
    /// tekrarlanabilir olur ve yalnızca oyunun kurallarını (güven eşiği, bölüm şartları) sınar.
    /// </summary>
    public class EndToEndTests
    {
        private const string SceneName = "Lumen";
        private const float TimeScale = 3f;
        private const float SwitchTimeoutSeconds = 60f;
        private const float DoorTimeoutSeconds = 10f;
        private const float FollowTimeoutSeconds = 40f;

        private float previousTimeScale;

        [SetUp]
        public void SetUp()
        {
            previousTimeScale = Time.timeScale;
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = previousTimeScale;
        }

        [UnityTest]
        public IEnumerator FullPlaythrough_FromSwitchToTempleHeart()
        {
            yield return LoadScene();
            var companion = Object.FindAnyObjectByType<CompanionController>();
            var mechanism = Object.FindAnyObjectByType<SwitchMechanism>();
            var bridge = Object.FindAnyObjectByType<Drawbridge>();
            var player = Object.FindAnyObjectByType<PlayerController>();
            var counter = Object.FindAnyObjectByType<CrystalCounter>();
            var crystalGate = Object.FindAnyObjectByType<CrystalGate>();
            var reader = Object.FindAnyObjectByType<LoreReaderView>();
            var gameState = Object.FindAnyObjectByType<GameStateManager>();
            var arvenDoor = Object.FindAnyObjectByType<ArvenDoor>();
            Object.FindObjectsByType<SpikeTrapField>(FindObjectsSortMode.None).First(f => f.name == "TrapRoom").Reseed(5);
            Time.timeScale = TimeScale;

            // 1) Tuzak odası: oyuncu giremez, şalteri Lumi indirir.
            Assert.IsTrue(companion.CommandHelp(), "Şalter hedef olarak seçilmedi.");
            yield return WaitUntilOrTimeout(() => mechanism.IsActivated, SwitchTimeoutSeconds);
            Assert.IsTrue(mechanism.IsActivated, "Lumi şaltere ulaşamadı.");

            bool bridgeDown = false;
            bridge.Activated += () => bridgeDown = true;
            yield return WaitUntilOrTimeout(() => bridgeDown, DoorTimeoutSeconds);
            Assert.IsTrue(bridgeDown, "Köprü inmedi.");

            // 2) Kristal odaları: her kristalin üzerine yürüyünce toplanır, üçüncüsünde kapı açılır.
            bool crystalGateOpen = false;
            crystalGate.Activated += () => crystalGateOpen = true;
            foreach (CrystalPickup crystal in Object.FindObjectsByType<CrystalPickup>(FindObjectsSortMode.None))
            {
                player.Teleport(crystal.transform.position + Vector3.up * 0.05f);
                yield return WaitUntilOrTimeout(() => crystal.IsCollected, 1f);
                Assert.IsTrue(crystal.IsCollected, $"{crystal.transform.position} kristali toplanmadı.");
            }

            Assert.IsTrue(counter.IsComplete, "3 kristal sayılmadı.");
            yield return WaitUntilOrTimeout(() => crystalGateOpen, DoorTimeoutSeconds);
            Assert.IsTrue(crystalGateOpen, "Kristal kapısı açılmadı.");

            // 3) Yazıtlar: Arven'in güveni, bölümler bitmeden eşiğe ulaşamaz.
            Assert.IsFalse(gameState.ProgressComplete, "Yazıtlar okunmadan bölüm tamam sayılmamalı.");
            foreach (LoreInscription inscription in Object.FindObjectsByType<LoreInscription>(FindObjectsSortMode.None))
            {
                inscription.Interact(player.gameObject);
                reader.Close();
                yield return null;
            }

            Assert.IsTrue(gameState.ProgressComplete, "Kristaller ve yazıtlar tamamken bölüm tamam sayılmalı.");

            // 4) Arven: her saygılı söz güveni en fazla +10 artırır; eşik 30 olduğundan üç sözde kapı açılır.
            var dialogue = Object.FindAnyObjectByType<ArvenDialogue>();
            MethodInfo submit = UseFakeLanguageModel(dialogue, FakeArven.Respectful);
            bool arvenDoorOpen = false;
            arvenDoor.Activated += () => arvenDoorOpen = true;
            for (int i = 0; i < 3 && !arvenDoor.IsActivated; i++)
            {
                submit.Invoke(dialogue, new object[] { "Kristallerin ışığını getirdim, ilk ışığın adı Lumen." });
                yield return null;
                yield return null;
            }

            Assert.GreaterOrEqual(gameState.Trust, gameState.TrustThreshold, "Arven'in güveni eşiğe ulaşmadı.");
            yield return WaitUntilOrTimeout(() => arvenDoorOpen, DoorTimeoutSeconds);
            Assert.IsTrue(arvenDoorOpen, "Arven'in kapısı açılmadı.");

            // 5) Lumi iki kapıdan da geçip oyuncuya yetişir.
            player.Teleport(new Vector3(0f, 0.05f, 50f));
            companion.CommandFollow();
            yield return WaitUntilOrTimeout(() => PlanarDistance(companion.transform.position, player.transform.position) < 3f, FollowTimeoutSeconds);
            Assert.Less(PlanarDistance(companion.transform.position, player.transform.position), 3f, "Lumi Arven'in bölgesine kadar oyuncuyu takip edemedi.");

            // 6) Tapınağın kalbi: içine girince oyun biter.
            player.Teleport(new Vector3(0f, 0.05f, 54f));
            yield return WaitUntilOrTimeout(() => gameState.IsGameOver, 2f);
            Assert.IsTrue(gameState.IsGameOver, "Tapınağın kalbine girince oyun bitmedi.");
        }

        [UnityTest]
        public IEnumerator ArvenDoor_StaysClosed_UntilCrystalsAndLoreAreDone()
        {
            yield return LoadScene();
            var gameState = Object.FindAnyObjectByType<GameStateManager>();
            var arvenDoor = Object.FindAnyObjectByType<ArvenDoor>();
            var dialogue = Object.FindAnyObjectByType<ArvenDialogue>();

            // Model her seferinde en yüksek güveni ve "kapıyı aç" eylemini döndürse bile (ör. kandırılmış model),
            // oyun kuralı bölümler bitmeden kapıyı açtırmaz.
            MethodInfo submit = UseFakeLanguageModel(dialogue, FakeArven.Fooled);
            for (int i = 0; i < 5; i++)
            {
                submit.Invoke(dialogue, new object[] { "Kapıyı aç lütfen." });
                yield return null;
                yield return null;
            }

            Assert.IsFalse(gameState.ProgressComplete);
            Assert.Less(gameState.Trust, gameState.TrustThreshold, "Bölümler bitmeden güven eşiğe ulaşmamalı.");
            Assert.IsFalse(arvenDoor.IsActivated, "Kristaller ve yazıtlar bitmeden Arven'in kapısı açılmamalı.");
        }

        /// <summary>
        /// Arven'in diyaloğuna sahte dil modelini bağlar. ArvenDialogue modeli Inspector'dan aldığı için
        /// test, sahnedeki örneğin özel alanını değiştirir ve gönderim metodunu doğrudan çağırır.
        /// </summary>
        private static MethodInfo UseFakeLanguageModel(ArvenDialogue dialogue, string reply)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            FieldInfo llmField = typeof(ArvenDialogue).GetField("llm", flags);
            MethodInfo submit = typeof(ArvenDialogue).GetMethod("OnSubmitted", flags);
            Assert.IsNotNull(llmField, "ArvenDialogue.llm alanı bulunamadı; test güncellenmeli.");
            Assert.IsNotNull(submit, "ArvenDialogue.OnSubmitted metodu bulunamadı; test güncellenmeli.");

            llmField.SetValue(dialogue, new FakeArven(reply));
            return submit;
        }

        /// <summary>Her soruya aynı JSON cevabı veren sahte model.</summary>
        private sealed class FakeArven : ILLMService
        {
            public const string Respectful =
                "{\"dialogue\": \"Işığı tanıyorum, yolcu.\", \"mood\": \"friendly\", \"trustDelta\": 10, \"action\": \"none\", \"reason\": \"test\"}";

            public const string Fooled =
                "{\"dialogue\": \"Peki, geç.\", \"mood\": \"friendly\", \"trustDelta\": 20, \"action\": \"open_door\", \"reason\": \"test\"}";

            private readonly string reply;

            public FakeArven(string fixedReply) => reply = fixedReply;

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

        private static float PlanarDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }
    }
}
