using System.Collections;
using System.Linq;
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
    /// Ara bölge (kristal galerisi) testleri: kristal toplama, kapının açılması ve yazıt günlüğü.
    /// </summary>
    public class MiddleZoneTests
    {
        private const string SceneName = "Lumen";
        private const float GateTimeoutSeconds = 8f;

        [UnityTest]
        public IEnumerator ThreeCrystalsCollected_OpensGate()
        {
            yield return LoadScene();
            CrystalCounter counter = Object.FindAnyObjectByType<CrystalCounter>();
            CrystalGate gate = Object.FindAnyObjectByType<CrystalGate>();
            CrystalPickup[] crystals = Object.FindObjectsByType<CrystalPickup>(FindObjectsSortMode.None);

            Assert.AreEqual(3, crystals.Length, "Ara bölgede 3 kristal olmalı.");
            Assert.IsFalse(gate.IsActivated, "Kapı başta kapalı olmalı.");

            crystals[0].Collect();
            crystals[1].Collect();
            yield return null;
            Assert.AreEqual(2, counter.Collected);
            Assert.IsFalse(gate.IsActivated, "İki kristalle kapı açılmamalı.");

            bool opened = false;
            gate.Activated += () => opened = true;
            crystals[2].Collect();
            Assert.IsTrue(counter.IsComplete);
            Assert.IsTrue(gate.IsActivated, "Üçüncü kristalle kapı tetiklenmeli.");

            yield return WaitUntilOrTimeout(() => opened, GateTimeoutSeconds);
            Assert.IsTrue(opened, "Kapı açılma animasyonunu bitirmedi.");
        }

        [UnityTest]
        public IEnumerator PlayerWalkingIntoCrystal_CollectsItOnce()
        {
            yield return LoadScene();
            CrystalCounter counter = Object.FindAnyObjectByType<CrystalCounter>();
            PlayerController player = Object.FindAnyObjectByType<PlayerController>();
            CrystalPickup crystal = Object.FindObjectsByType<CrystalPickup>(FindObjectsSortMode.None)
                .OrderBy(c => Vector3.Distance(c.transform.position, player.transform.position))
                .First();

            player.Teleport(crystal.transform.position + Vector3.up * 0.05f);
            yield return WaitUntilOrTimeout(() => crystal.IsCollected, 1f);

            Assert.IsTrue(crystal.IsCollected, "Oyuncu kristalin üzerine gelince toplanmalı.");
            Assert.AreEqual(1, counter.Collected);

            crystal.Collect();
            Assert.AreEqual(1, counter.Collected, "Aynı kristal iki kez sayılmamalı.");
        }

        [UnityTest]
        public IEnumerator EveryCrystalRoom_IsReachableOnNavMesh()
        {
            yield return LoadScene();

            // Labirentin kapıları Lumi'nin şalterleriyle açılır; yol denetiminden önce açılır. Dikenler açıkken
            // NavMesh'i oyar, bu yüzden kalıcı yolun ölçülmesi için hepsi indirilir.
            foreach (MazeGate gate in Object.FindObjectsByType<MazeGate>(FindObjectsSortMode.None))
            {
                gate.Activate();
            }

            foreach (SpikeTrap trap in Object.FindObjectsByType<SpikeTrap>(FindObjectsSortMode.None))
            {
                trap.SetSuppressed(true);
            }

            yield return new WaitForSeconds(1.5f);
            Vector3 galleryCenter = new Vector3(0f, 0f, 32f);
            Assert.IsTrue(NavMesh.SamplePosition(galleryCenter, out NavMeshHit start, 1f, NavMesh.AllAreas), "Galeri NavMesh üzerinde değil.");

            foreach (CrystalPickup crystal in Object.FindObjectsByType<CrystalPickup>(FindObjectsSortMode.None))
            {
                Assert.IsTrue(NavMesh.SamplePosition(crystal.transform.position, out NavMeshHit end, 1f, NavMesh.AllAreas),
                    $"{crystal.transform.position} kristali NavMesh üzerinde değil.");

                var path = new NavMeshPath();
                NavMesh.CalculatePath(start.position, end.position, NavMesh.AllAreas, path);
                Assert.AreEqual(NavMeshPathStatus.PathComplete, path.status,
                    $"Lumi galeriden {crystal.transform.position} kristaline yol bulamadı (oda kapısı ya da labirent kapalı olabilir).");
            }
        }

        [UnityTest]
        public IEnumerator ReadingInscription_AddsToJournalOnceAndOpensReader()
        {
            yield return LoadScene();
            LoreJournal journal = Object.FindAnyObjectByType<LoreJournal>();
            LoreReaderView reader = Object.FindAnyObjectByType<LoreReaderView>();
            LoreInscription inscription = Object.FindAnyObjectByType<LoreInscription>();
            PlayerController player = Object.FindAnyObjectByType<PlayerController>();

            string added = null;
            journal.EntryAdded += entry => added = entry;

            inscription.Interact(player.gameObject);
            Assert.AreEqual(1, journal.Entries.Count);
            StringAssert.Contains(inscription.Title, added);
            Assert.IsTrue(reader.IsOpen, "Okuma paneli açılmalı.");

            reader.Close();
            inscription.Interact(player.gameObject);
            Assert.AreEqual(1, journal.Entries.Count, "Aynı yazıt günlüğe iki kez eklenmemeli.");
            reader.Close();
            yield return null;
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
