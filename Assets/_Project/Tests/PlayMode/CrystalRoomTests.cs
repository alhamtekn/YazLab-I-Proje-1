using System.Collections;
using System.Linq;
using Lumen.Companion;
using Lumen.Level;
using Lumen.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Lumen.Tests
{
    /// <summary>
    /// Kristal odası parkurları: her oda Lumi'nin otonom yardımını gerektirir. Testler Lumi'yi odanın girişine
    /// koyup "yardım et" der ve Lumi'nin doğru hedefi kendisi seçip tamamladığını denetler.
    /// </summary>
    public class CrystalRoomTests
    {
        private const string SceneName = "Lumen";
        private const float TimeScale = 3f;
        private const float LumiTimeoutSeconds = 30f;

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
        public IEnumerator DarkRoom_LumiFindsSafePathAndLightsBeacon()
        {
            yield return LoadScene();
            var companion = Object.FindAnyObjectByType<CompanionController>();
            var player = Object.FindAnyObjectByType<PlayerController>();
            var beacon = Object.FindAnyObjectByType<CompanionBeacon>();
            Time.timeScale = TimeScale;

            player.Teleport(new Vector3(-6.5f, 0.05f, 35f));
            companion.PlaceAt(new Vector3(-7f, 0f, 35f));
            Assert.IsTrue(companion.CommandHelp(), "Lumi bir hedef seçmedi.");
            Assert.AreSame(beacon, companion.CurrentTarget, "Karanlık odanın girişinde en yakın hedef işaret olmalı.");

            yield return WaitUntilOrTimeout(() => beacon.IsLit, LumiTimeoutSeconds);
            Assert.IsTrue(beacon.IsLit, "Lumi karanlık odadaki işarete güvenli yoldan ulaşamadı.");
            Assert.Greater(companion.transform.position.y, -0.5f, "Lumi çukura düşmemeli.");
        }

        [UnityTest]
        public IEnumerator DarkRoom_FallingIntoPit_ReturnsToEntrance()
        {
            yield return LoadScene();
            var player = Object.FindAnyObjectByType<PlayerController>();

            // (0, 0) karosu çukur: galeriye en yakın köşe.
            player.Teleport(new Vector3(-9.05f, 0.05f, 33.8f));
            yield return WaitUntilOrTimeout(() => player.transform.position.x > -8f, 3f);

            Assert.Greater(player.transform.position.x, -8f, "Çukura düşen oyuncu odanın girişine dönmeli.");
            Assert.Greater(player.transform.position.y, -0.5f);
        }

        [UnityTest]
        public IEnumerator Maze_LumiOpensBothGates_AndTimedGateClosesAgain()
        {
            yield return LoadScene();
            var companion = Object.FindAnyObjectByType<CompanionController>();
            var player = Object.FindAnyObjectByType<PlayerController>();
            MazeGate[] gates = Object.FindObjectsByType<MazeGate>(FindObjectsSortMode.None).OrderBy(g => g.name).ToArray();
            MazeGate gate1 = gates[0];
            MazeGate gate2 = gates[1];
            Time.timeScale = TimeScale;

            // Batı koridoru: en yakın kapalı kapının şalteri batı nişinde.
            player.Teleport(new Vector3(9.5f, 0.05f, 28f));
            companion.PlaceAt(new Vector3(9.5f, 0f, 28.8f));
            Assert.IsTrue(companion.CommandHelp());
            yield return WaitUntilOrTimeout(() => gate1.IsActivated, LumiTimeoutSeconds);
            Assert.IsTrue(gate1.IsActivated, "Lumi alçak geçitten 1. şaltere ulaşamadı.");

            // Orta koridor: 2. şalter.
            yield return new WaitForSeconds(1f);
            player.Teleport(new Vector3(11.5f, 0.05f, 29f));
            companion.PlaceAt(new Vector3(11.5f, 0f, 29.8f));
            Assert.IsTrue(companion.CommandHelp());
            yield return WaitUntilOrTimeout(() => gate2.IsActivated, LumiTimeoutSeconds);
            Assert.IsTrue(gate2.IsActivated, "Lumi 2. şaltere ulaşamadı.");

            yield return new WaitForSeconds(1f);
            CrystalPickup crystal = NearestCrystal(new Vector3(13.3f, 0f, 30.8f));
            Assert.AreEqual(NavMeshPathStatus.PathComplete, PathStatus(new Vector3(11.5f, 0f, 29f), crystal.transform.position),
                "Kapılar açıkken kristale yol olmalı.");

            bool closed = false;
            gate2.Closed += () => closed = true;
            yield return WaitUntilOrTimeout(() => closed, gate2.OpenSeconds + 5f);
            Assert.IsTrue(closed, "Süreli kapı yeniden kapanmalı.");
            Assert.IsTrue(gate1.IsActivated, "Kalıcı kapı açık kalmalı.");
        }

        [UnityTest]
        public IEnumerator SpikeRoom_LumiHoldsFarPlate_PlayerPlateStopsNearTraps()
        {
            yield return LoadScene();
            var companion = Object.FindAnyObjectByType<CompanionController>();
            var player = Object.FindAnyObjectByType<PlayerController>();
            var lumiPlate = Object.FindAnyObjectByType<CompanionPlate>();
            var playerPlate = Object.FindAnyObjectByType<PressurePlate>();
            Time.timeScale = TimeScale;

            player.Teleport(new Vector3(7f, 0.05f, 36f));
            companion.PlaceAt(new Vector3(9f, 0f, 36f));
            Assert.IsTrue(companion.CommandHelp());
            Assert.AreSame(lumiPlate, companion.CurrentTarget, "Diken odasının girişinde en yakın hedef Lumi plakası olmalı.");

            yield return WaitUntilOrTimeout(() => lumiPlate.IsPressed, LumiTimeoutSeconds * 2f);
            Assert.IsTrue(lumiPlate.IsPressed, "Lumi tuzakların arasından plakaya ulaşamadı.");

            SpikeTrap[] roomTraps = playerPlate.transform.parent.GetComponentsInChildren<SpikeTrap>();
            Assert.AreEqual(3, roomTraps.Count(t => t.IsSuppressed), "Lumi plakası uzak sütunun 3 tuzağını durdurmalı.");

            player.Teleport(playerPlate.transform.position + Vector3.up * 0.05f);
            yield return WaitUntilOrTimeout(() => playerPlate.IsHolding, 1f);
            Assert.IsTrue(playerPlate.IsHolding, "Oyuncu plakaya basınca plaka çalışmalı.");
            Assert.IsTrue(roomTraps.All(t => t.IsSuppressed && !t.IsDangerous), "İki plaka birlikte bütün dikenleri durdurmalı.");
        }

        private static CrystalPickup NearestCrystal(Vector3 position)
        {
            return Object.FindObjectsByType<CrystalPickup>(FindObjectsSortMode.None)
                .OrderBy(c => Vector3.Distance(c.transform.position, position))
                .First();
        }

        private static NavMeshPathStatus PathStatus(Vector3 from, Vector3 to)
        {
            NavMesh.SamplePosition(from, out NavMeshHit start, 1f, NavMesh.AllAreas);
            NavMesh.SamplePosition(to, out NavMeshHit end, 1f, NavMesh.AllAreas);
            var path = new NavMeshPath();
            NavMesh.CalculatePath(start.position, end.position, NavMesh.AllAreas, path);
            return path.status;
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
