using System.Collections;
using Lumen.Level;
using Lumen.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Lumen.Tests
{
    /// <summary>Etkileşim (E) yalnızca yakındaki ve arada duvar olmayan nesnelerle yapılabilir.</summary>
    public class InteractionTests
    {
        // Galerinin batı duvarındaki (x = -8) yazıt; duvarın öbür yanı karanlık oda (A)
        private static readonly Vector3 Inscription = new Vector3(-7.9f, 0f, 38f);

        [UnityTest]
        public IEnumerator Inscription_IsReadable_FromTheGallerySide()
        {
            yield return SceneManager.LoadSceneAsync("Lumen");
            PlayerController player = Object.FindAnyObjectByType<PlayerController>();
            PlayerInteractor interactor = player.GetComponent<PlayerInteractor>();

            player.Teleport(Inscription + new Vector3(1.5f, 0.05f, 0f));
            yield return null;
            yield return null;
            Assert.IsInstanceOf<LoreInscription>(interactor.Current, "Yazıtın önünde durunca okunabilmeli.");
        }

        [UnityTest]
        public IEnumerator Inscription_IsNotReadable_ThroughTheWall()
        {
            yield return SceneManager.LoadSceneAsync("Lumen");
            PlayerController player = Object.FindAnyObjectByType<PlayerController>();
            PlayerInteractor interactor = player.GetComponent<PlayerInteractor>();

            player.Teleport(Inscription + new Vector3(-1.3f, 0.05f, 0f)); // duvarın arkası, karanlık oda
            yield return null;
            yield return null;
            // Aynı odadaki kristal seçilebilir; önemli olan duvarın öbür yüzündeki yazıtın seçilmemesi
            Assert.IsNotInstanceOf<LoreInscription>(interactor.Current, "Duvarın arkasından yazıt okunmamalı.");
        }
    }
}
