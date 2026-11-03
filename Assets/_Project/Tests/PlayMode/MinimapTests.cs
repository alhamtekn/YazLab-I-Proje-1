using System.Collections;
using Lumen.Core;
using Lumen.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Lumen.Tests
{
    /// <summary>Mini harita ve tavan: harita oyuncuyu izler, tavanı çizmez; tavan NavMesh'e ve fiziğe karışmaz.</summary>
    public class MinimapTests
    {
        [UnityTest]
        public IEnumerator Minimap_FollowsPlayer_AndIgnoresCeiling()
        {
            yield return SceneManager.LoadSceneAsync("Lumen");
            yield return null;
            MinimapView minimap = Object.FindAnyObjectByType<MinimapView>();
            PlayerController player = Object.FindAnyObjectByType<PlayerController>();
            Assert.IsNotNull(minimap, "Sahnede mini harita yok.");

            Camera cam = minimap.GetComponent<Camera>();
            Assert.IsTrue(cam.orthographic);
            Assert.IsNotNull(cam.targetTexture, "Mini harita bir dokuya çizmeli.");
            Assert.AreEqual(0, cam.cullingMask & (1 << GameLayers.CeilingIndex), "Mini harita tavanı çizmemeli.");

            player.Teleport(new Vector3(0f, 0.05f, 30f));
            yield return null;
            yield return null;
            Vector3 offset = minimap.transform.position - player.transform.position;
            Assert.AreEqual(0f, new Vector2(offset.x, offset.z).magnitude, 0.01f, "Harita kamerası oyuncunun tepesinde olmalı.");
        }

        [UnityTest]
        public IEnumerator Ceilings_HaveNoCollider()
        {
            yield return SceneManager.LoadSceneAsync("Lumen");
            int ceilings = 0;
            foreach (Renderer renderer in Object.FindObjectsByType<Renderer>())
            {
                if (renderer.gameObject.layer == GameLayers.CeilingIndex)
                {
                    ceilings++;
                    Assert.IsNull(renderer.GetComponent<Collider>(), $"{renderer.name} tavanının çarpıştırıcısı olmamalı.");
                }
            }

            Assert.Greater(ceilings, 0, "Sahnede tavan yok.");
        }
    }
}
