using System.Collections;
using Lumen.Level;
using Lumen.NPC;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Lumen.Tests
{
    /// <summary>Sohbetteki güven çubuğu güvenle birlikte dolar.</summary>
    public class TrustBarTests
    {
        [UnityTest]
        public IEnumerator TrustBar_FillsWithTrust()
        {
            yield return SceneManager.LoadSceneAsync("Lumen");
            yield return null;
            var view = Object.FindAnyObjectByType<ChatView>();
            var dialogue = Object.FindAnyObjectByType<ArvenDialogue>();
            Assert.AreEqual(0f, view.TrustBarFill, 0.001f, "Oyun başında çubuk boş olmalı.");

            // Bölümleri bitir ki güven tavana takılmasın; sonra Arven'in iki +10'luk cevabını taklit et
            foreach (CrystalPickup crystal in Object.FindObjectsByType<CrystalPickup>()) crystal.Collect();
            GameObject player = Object.FindAnyObjectByType<Lumen.Player.PlayerController>().gameObject;
            foreach (LoreInscription inscription in Object.FindObjectsByType<LoreInscription>()) inscription.Interact(player);
            Object.FindAnyObjectByType<Lumen.Player.LoreReaderView>().Close(); // okuma paneli açık kalmasın
            yield return null;

            var reply = new NPCResponse { dialogue = "Işığı tanıyorsun.", mood = "friendly", trustDelta = 10, action = "none" };
            RaiseResponded(dialogue, reply);
            Assert.AreEqual(10f / 30f, view.TrustBarFill, 0.01f);
            RaiseResponded(dialogue, reply);
            Assert.AreEqual(20f / 30f, view.TrustBarFill, 0.01f);
        }

        // ArvenDialogue.Responded olayını dışarıdan tetikler (olay alanına yansıma ile erişilir)
        private static void RaiseResponded(ArvenDialogue dialogue, NPCResponse response)
        {
            var field = typeof(ArvenDialogue).GetField("Responded", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            ((System.Action<NPCResponse>)field.GetValue(dialogue))?.Invoke(response);
        }
    }
}
