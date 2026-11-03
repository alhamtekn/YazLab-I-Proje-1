using System.Collections;
using Lumen.Companion;
using Lumen.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Lumen.Tests
{
    /// <summary>Giriş koridoru: oyuncu koridorun başında başlar, sonunda Lumi ile tanışır.</summary>
    public class IntroTests
    {
        private const string SceneName = "Lumen";

        [UnityTest]
        public IEnumerator Player_StartsInCorridor_AndCanReachHall()
        {
            yield return LoadScene();
            PlayerController player = Object.FindAnyObjectByType<PlayerController>();
            Assert.Less(player.transform.position.z, -15f, "Oyuncu giriş koridorunun başında başlamalı.");

            Assert.IsTrue(NavMesh.SamplePosition(player.transform.position, out NavMeshHit start, 1f, NavMesh.AllAreas), "Koridor NavMesh üzerinde değil.");
            Assert.IsTrue(NavMesh.SamplePosition(new Vector3(0f, 0f, 8f), out NavMeshHit hall, 1f, NavMesh.AllAreas));
            var path = new NavMeshPath();
            NavMesh.CalculatePath(start.position, hall.position, NavMesh.AllAreas, path);
            Assert.AreEqual(NavMeshPathStatus.PathComplete, path.status, "Koridordan salona geçilemiyor (arka duvardaki kapı kapalı olabilir).");
        }

        [UnityTest]
        public IEnumerator ApproachingLumi_IntroducesAndStartsFollowing()
        {
            yield return LoadScene();
            PlayerController player = Object.FindAnyObjectByType<PlayerController>();
            CompanionController companion = Object.FindAnyObjectByType<CompanionController>();
            CompanionMeeting meeting = Object.FindAnyObjectByType<CompanionMeeting>();

            Assert.IsFalse(meeting.HasMet, "Tanışma oyun başında olmamalı.");
            Assert.AreNotEqual(CompanionStateId.Follow, companion.CurrentState, "Lumi tanışmadan önce takip etmemeli.");

            player.Teleport(companion.transform.position + new Vector3(0f, 0.05f, -2f));
            yield return WaitUntilOrTimeout(() => meeting.HasMet, 1f);

            Assert.IsTrue(meeting.HasMet, "Lumi'ye yaklaşınca tanışma başlamalı.");

            // Oyuncu yanındayken Lumi durur; salona doğru uzaklaşınca peşinden gelmeli.
            player.Teleport(new Vector3(0f, 0.05f, 8f));
            yield return WaitUntilOrTimeout(() => companion.CurrentState == CompanionStateId.Follow, 2f);
            Assert.AreEqual(CompanionStateId.Follow, companion.CurrentState, "Tanışmadan sonra Lumi oyuncuyu takip etmeli.");
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
