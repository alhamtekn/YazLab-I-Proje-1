using System.Collections;
using System.Reflection;
using Lumen.NPC;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Lumen.Tests
{
    /// <summary>
    /// Ollama kapalıyken (sunucuya ulaşılamıyor): oyun çökmez, Arven "sessizliğe gömüldü" der, güven değişmez.
    /// Gerçek Ollama açık olsa bile test kapalı bir porta bağlanır, böylece her bilgisayarda aynı sonucu verir.
    /// </summary>
    public class OllamaOfflineTests
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

        [UnityTest]
        public IEnumerator UnreachableServer_GivesOfflineReply_AndKeepsTrust()
        {
            yield return SceneManager.LoadSceneAsync("Lumen");
            yield return null;
            var service = Object.FindAnyObjectByType<OllamaService>();
            var dialogue = Object.FindAnyObjectByType<ArvenDialogue>();
            var gameState = Object.FindAnyObjectByType<GameStateManager>();

            // 1 numaralı portta dinleyen sunucu yok: bağlantı hemen reddedilir
            typeof(OllamaService).GetField("baseUrl", Private).SetValue(service, "http://127.0.0.1:1");

            NPCResponse reply = null;
            dialogue.Responded += r => reply = r;
            int trustBefore = gameState.Trust;
            MethodInfo submit = typeof(ArvenDialogue).GetMethod("OnSubmitted", Private);
            Assert.IsNotNull(submit, "ArvenDialogue.OnSubmitted bulunamadı; test güncellenmeli.");
            submit.Invoke(dialogue, new object[] { "Merhaba Arven, tapınağı anlatır mısın?" });

            for (float t = 0f; t < 10f && reply == null; t += Time.deltaTime)
            {
                yield return null;
            }

            Assert.IsNotNull(reply, "Sunucu kapalıyken de bir cevap gösterilmeli.");
            Assert.AreEqual(NPCResponseParser.OfflineMessage, reply.dialogue);
            Assert.AreEqual("fallback", reply.reason);
            Assert.AreEqual(trustBefore, gameState.Trust, "Cevap alınamayınca güven değişmemeli.");
        }
    }
}
