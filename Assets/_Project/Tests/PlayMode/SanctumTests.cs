using System;
using System.Collections;
using Lumen.Companion;
using Lumen.Player;
using Lumen.Sanctum;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Lumen.Tests
{
    /// <summary>
    /// Kutsal bölüm (Arven'in kapısından sonra) testleri: İkiz Mühür ve Işığın Yolu ancak Lumi ile birlikte geçilir.
    /// </summary>
    public class SanctumTests
    {
        private const string SceneName = "Lumen";
        private const float LumiTimeoutSeconds = 25f;
        private const float ChasmStartZ = 74f;

        // SanctumSection (editör kodu) ile aynı yerler; test derlemesi editör koduna erişemez
        private static readonly Vector3 PlayerSeal = new Vector3(-5f, 0f, 64f);
        private static readonly Vector3 Beacon = new Vector3(1.5f, 0f, 87.5f);

        private TwinSeal seal;
        private LightPath path;
        private PlayerController player;
        private CompanionController lumi;

        [UnityTest]
        public IEnumerator TwinSeal_OpensOnlyWhenPlayerAndLumiPressTogether()
        {
            yield return Setup();
            player.Teleport(new Vector3(0f, 0.05f, 60f));
            lumi.PlaceAt(new Vector3(1f, 0f, 60f));
            yield return null;

            // Lumi mührünü şarj eder ama oyuncu kendi mühründe değil: bariyer kapalı kalmalı
            Assert.IsTrue(lumi.CommandHelp(), "Lumi'ye yardım hedefi bulunamadı.");
            Assert.AreSame(seal, lumi.CurrentTarget, "Lumi ikiz mührü seçmeli.");
            yield return WaitUntilOrTimeout(() => seal.LumiSealCharged, LumiTimeoutSeconds);
            Assert.IsTrue(seal.LumiSealCharged, "Lumi alçak girintideki mühre ulaşamadı.");
            Assert.IsFalse(seal.IsOpen, "Oyuncu mührüne basılmadan bariyer açılmamalı.");

            // Oyuncu mühre basınca, şarj sürerken bariyer açılır
            player.Teleport(PlayerSeal + Vector3.up * 0.05f);
            yield return WaitUntilOrTimeout(() => seal.IsOpen, 1f);
            Assert.IsTrue(seal.IsOpen, "İki mühür birlikte yanınca bariyer açılmalı.");
        }

        [UnityTest]
        public IEnumerator TwinSeal_PlayerAloneCannotOpen()
        {
            yield return Setup();
            player.Teleport(PlayerSeal + Vector3.up * 0.05f);
            yield return new WaitForSeconds(1f);
            Assert.IsTrue(seal.PlayerOnPlate);
            Assert.IsFalse(seal.IsOpen, "Oyuncu tek başına bariyeri açamamalı.");

            // Lumi girintisine oyuncu sığmamalı
            Assert.IsTrue(Physics.CheckBox(new Vector3(8.25f, 1.4f, 63.5f), new Vector3(0.2f, 0.25f, 0.9f)),
                "Girintinin üstü 1.1 m'de kapanmalı (oyuncu sığmaz).");
        }

        [UnityTest]
        public IEnumerator Trials_AreNotOfferedToLumi_BeforeTheirTurn()
        {
            yield return Setup();
            player.Teleport(new Vector3(0f, 0.05f, 45f)); // Arven'in bölgesi
            yield return null;
            Assert.IsFalse(seal.NeedsHelp, "Oyuncu kutsal bölüme girmeden Lumi mühre çağrılmamalı.");
            Assert.IsFalse(path.NeedsHelp, "Bariyer açılmadan Lumi fenere çağrılmamalı.");
        }

        [UnityTest]
        public IEnumerator Lumi_FollowsPlayer_ThroughSanctumEntrance()
        {
            yield return Setup();

            // Arven'in kapısının ardından (z 55) kutsal bölümün içine (z 62) kesintisiz yol olmalı
            var navPath = new NavMeshPath();
            NavMesh.CalculatePath(new Vector3(0f, 0f, 55f), new Vector3(0f, 0f, 62f), NavMesh.AllAreas, navPath);
            Assert.AreEqual(NavMeshPathStatus.PathComplete, navPath.status, "Kutsal bölümün girişinde NavMesh kopuk.");

            lumi.PlaceAt(new Vector3(0f, 0f, 55f));
            player.Teleport(new Vector3(0f, 0.05f, 62f));
            lumi.CommandFollow();
            yield return WaitUntilOrTimeout(() => lumi.transform.position.z > 59f, LumiTimeoutSeconds);
            Assert.Greater(lumi.transform.position.z, 59f, "Lumi giriş geçidinden geçemedi.");
        }

        [UnityTest]
        public IEnumerator LightPath_IsHidden_AndLumiRevealsItOnTheWayToBeacon()
        {
            yield return Setup();
            yield return OpenSeal();

            // Yol karşıya NavMesh ile bağlı olmalı
            var navPath = new NavMeshPath();
            NavMesh.CalculatePath(new Vector3(0f, 0f, 72.5f), Beacon, NavMesh.AllAreas, navPath);
            Assert.AreEqual(NavMeshPathStatus.PathComplete, navPath.status, "Görünmez yol fenere bağlı değil.");

            player.Teleport(new Vector3(-4f, 0.05f, 72.5f));
            lumi.PlaceAt(new Vector3(-6f, 0f, 66f));
            yield return null;
            yield return null;
            Assert.AreEqual(0, path.VisibleTiles, "Lumi uzaktayken yol görünmemeli.");

            Assert.IsTrue(lumi.CommandHelp());
            Assert.AreSame(path, lumi.CurrentTarget, "Bariyer açıldıktan sonra Lumi fenere gitmeli.");
            int maxVisible = 0;
            yield return WaitUntilOrTimeout(() =>
            {
                maxVisible = Math.Max(maxVisible, path.VisibleTiles);
                return path.BeaconLit;
            }, LumiTimeoutSeconds);

            Assert.IsTrue(path.BeaconLit, "Lumi fenere ulaşamadı.");
            Assert.Greater(maxVisible, 3, "Lumi yürürken yolun taşları görünmeliydi.");
        }

        [UnityTest]
        public IEnumerator FallingIntoChasm_LosesTheGame()
        {
            yield return Setup();
            yield return OpenSeal();
            var gameState = Object.FindAnyObjectByType<Lumen.NPC.GameStateManager>();

            player.Teleport(new Vector3(-6f, 0.05f, 80f)); // yolun olmadığı yer
            yield return WaitUntilOrTimeout(() => gameState.IsLost, 5f);
            Assert.IsTrue(gameState.IsLost, "Işığın Yolu'ndan lava düşen oyuncu kaybetmeli.");
        }

        private IEnumerator Setup()
        {
            yield return SceneManager.LoadSceneAsync(SceneName);
            yield return null;
            seal = Object.FindAnyObjectByType<TwinSeal>();
            path = Object.FindAnyObjectByType<LightPath>();
            player = Object.FindAnyObjectByType<PlayerController>();
            lumi = Object.FindAnyObjectByType<CompanionController>();
            Assert.IsNotNull(seal, "Sahnede İkiz Mühür yok.");
            Assert.IsNotNull(path, "Sahnede Işığın Yolu yok.");
        }

        // Bariyeri doğrudan aç: oyuncu mühürde + Lumi'nin varışı taklit edilir
        private IEnumerator OpenSeal()
        {
            player.Teleport(PlayerSeal + Vector3.up * 0.05f);
            seal.OnCompanionArrived(null);
            yield return WaitUntilOrTimeout(() => seal.IsOpen, 1f);
            Assert.IsTrue(seal.IsOpen, "Bariyer açılamadı.");
            yield return new WaitForSeconds(0.3f); // NavMesh oyuğu kapansın
        }

        private static IEnumerator WaitUntilOrTimeout(Func<bool> condition, float timeout)
        {
            for (float t = 0f; t < timeout && !condition(); t += Time.deltaTime)
            {
                yield return null;
            }
        }
    }
}
