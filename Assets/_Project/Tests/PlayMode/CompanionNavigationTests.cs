using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Lumen.Companion;
using Lumen.Level;
using Lumen.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Lumen.Tests
{
    /// <summary>
    /// Lumi'nin şaltere ulaşma testleri. Her deneme sahneyi yeniden yükler, Lumi'yi farklı bir başlangıç
    /// noktasına koyar, tuzak alanını farklı bir tohumla başlatır ve "yardım et" emri verir.
    /// Sonuçlar TestResults/companion_trials.csv ve TestResults/companion_summary.md dosyalarına yazılır.
    /// </summary>
    public class CompanionNavigationTests
    {
        private const string SceneName = "Lumen";
        private const float TimeoutSeconds = 60f;
        private const float TimeScale = 3f;

        private static readonly Vector3[] StartPoints =
        {
            new Vector3(-6f, 0f, 2f), new Vector3(-3f, 0f, 3f), new Vector3(0f, 0f, 3f), new Vector3(3f, 0f, 3f), new Vector3(6f, 0f, 2f),
            new Vector3(-6f, 0f, 10f), new Vector3(-3f, 0f, 12f), new Vector3(0f, 0f, 14f), new Vector3(3f, 0f, 10f), new Vector3(6f, 0f, 14f)
        };

        private static readonly int[] Seeds = { 11, 23, 37 };

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
        public IEnumerator HelpCommand_ReachesSwitch_FromVariedStartsAndTrapSeeds()
        {
            var results = new List<TrialResult>();

            foreach (int seed in Seeds)
            {
                for (int i = 0; i < StartPoints.Length; i++)
                {
                    var trial = new TrialResult { Index = results.Count + 1, Start = StartPoints[i], Seed = seed };
                    yield return RunTrial(trial);
                    results.Add(trial);
                }
            }

            WriteReports(results);

            float successRate = results.Count(r => r.Success) / (float)results.Count;
            Assert.GreaterOrEqual(successRate, 0.8f, $"Başarı oranı %{successRate * 100f:F0}. Ayrıntılar TestResults/companion_trials.csv içinde.");
        }

        [UnityTest]
        public IEnumerator AfterSwitch_CompanionCrossesBridgeAndRejoinsPlayer()
        {
            yield return LoadScene();
            var companion = Object.FindAnyObjectByType<CompanionController>();
            var mechanism = Object.FindAnyObjectByType<SwitchMechanism>();
            var player = Object.FindAnyObjectByType<PlayerController>();
            Object.FindAnyObjectByType<SpikeTrapField>().Reseed(5);
            Time.timeScale = TimeScale;

            Assert.IsTrue(companion.CommandHelp(), "Şalter hedef olarak seçilmedi.");
            yield return WaitUntilOrTimeout(() => mechanism.IsActivated, TimeoutSeconds);
            Assert.IsTrue(mechanism.IsActivated, "Lumi şaltere ulaşamadı.");

            // Köprünün inmesini bekle, sonra oyuncuyu karşı tarafa taşı.
            yield return new WaitForSeconds(2.5f);
            player.Teleport(new Vector3(0f, 0.05f, 32f));
            companion.CommandFollow();

            float start = Time.time;
            yield return WaitUntilOrTimeout(() => PlanarDistance(companion.transform.position, player.transform.position) < 3f, 40f);
            float elapsed = Time.time - start;

            Assert.Less(PlanarDistance(companion.transform.position, player.transform.position), 3f,
                $"Lumi {elapsed:F1} sn içinde köprüyü geçip oyuncuya ulaşamadı.");
        }

        private IEnumerator RunTrial(TrialResult trial)
        {
            yield return LoadScene();

            var companion = Object.FindAnyObjectByType<CompanionController>();
            var mechanism = Object.FindAnyObjectByType<SwitchMechanism>();
            var field = Object.FindAnyObjectByType<SpikeTrapField>();

            field.Reseed(trial.Seed);
            companion.PlaceAt(trial.Start);
            int avoidEntries = 0;
            companion.StateChanged += s =>
            {
                if (s == CompanionStateId.Avoid)
                {
                    avoidEntries++;
                }
            };

            yield return null;
            Time.timeScale = TimeScale;
            float startTime = Time.time;

            if (!companion.CommandHelp())
            {
                trial.Note = "hedef seçilemedi";
                yield break;
            }

            yield return WaitUntilOrTimeout(() => mechanism.IsActivated, TimeoutSeconds);

            trial.Success = mechanism.IsActivated;
            trial.Seconds = Time.time - startTime;
            trial.HazardHits = companion.HazardHits;
            trial.AvoidEntries = avoidEntries;
            if (!trial.Success)
            {
                trial.Note = "zaman aşımı";
            }

            Time.timeScale = 1f;
        }

        private static IEnumerator LoadScene()
        {
            Time.timeScale = 1f;
            AsyncOperation load = SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            while (!load.isDone)
            {
                yield return null;
            }

            // Start() çağrılarının tamamlanması için iki kare bekle.
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

        private static void WriteReports(List<TrialResult> results)
        {
            string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "TestResults"));
            Directory.CreateDirectory(folder);
            CultureInfo inv = CultureInfo.InvariantCulture;

            var csv = new StringBuilder("deneme,baslangic_x,baslangic_z,tohum,basarili,sure_sn,tuzak_carpma,avoid_sayisi,not\n");
            foreach (TrialResult r in results)
            {
                csv.AppendLine(string.Join(",",
                    r.Index.ToString(inv), r.Start.x.ToString("F1", inv), r.Start.z.ToString("F1", inv), r.Seed.ToString(inv),
                    r.Success ? "1" : "0", r.Seconds.ToString("F2", inv), r.HazardHits.ToString(inv), r.AvoidEntries.ToString(inv), r.Note));
            }

            File.WriteAllText(Path.Combine(folder, "companion_trials.csv"), csv.ToString(), Encoding.UTF8);

            List<TrialResult> ok = results.Where(r => r.Success).ToList();
            var md = new StringBuilder();
            md.AppendLine("| Ölçüt | Değer |");
            md.AppendLine("| --- | --- |");
            md.AppendLine($"| Deneme sayısı | {results.Count} ({StartPoints.Length} başlangıç noktası × {Seeds.Length} tuzak tohumu) |");
            md.AppendLine($"| Başarı oranı | %{100f * ok.Count / results.Count:F0} ({ok.Count}/{results.Count}) |");
            md.AppendLine($"| Ortalama tamamlama süresi | {(ok.Count > 0 ? ok.Average(r => r.Seconds) : 0f).ToString("F2", inv)} sn |");
            md.AppendLine($"| En kısa / en uzun süre | {(ok.Count > 0 ? ok.Min(r => r.Seconds) : 0f).ToString("F2", inv)} / {(ok.Count > 0 ? ok.Max(r => r.Seconds) : 0f).ToString("F2", inv)} sn |");
            md.AppendLine($"| Toplam tuzağa yakalanma | {results.Sum(r => r.HazardHits)} |");
            md.AppendLine($"| Ortalama Avoid durumuna geçiş | {results.Average(r => r.AvoidEntries).ToString("F1", inv)} |");
            File.WriteAllText(Path.Combine(folder, "companion_summary.md"), md.ToString(), Encoding.UTF8);

            Debug.Log("[CompanionNavigationTests]\n" + md);
        }

        private sealed class TrialResult
        {
            public int Index;
            public Vector3 Start;
            public int Seed;
            public bool Success;
            public float Seconds;
            public int HazardHits;
            public int AvoidEntries;
            public string Note = string.Empty;
        }
    }
}
