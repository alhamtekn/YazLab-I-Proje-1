using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Lumen.NPC;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Lumen.Tests
{
    /// <summary>
    /// Asenkron çalışma: model cevap verirken oyun donmaz. Cevabı 1 sn geciktiren sahte model bağlanır;
    /// beklerken karelerin akmaya devam ettiği ve "Arven düşünüyor..." göstergesinin açık olduğu ölçülür.
    /// </summary>
    public class AsyncDialogueTests
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private const string Reply = "{\"dialogue\": \"Dinliyorum, yolcu.\", \"mood\": \"neutral\", \"trustDelta\": 0, \"action\": \"none\", \"reason\": \"test\"}";

        [UnityTest]
        public IEnumerator WaitingForModel_DoesNotFreezeTheGame()
        {
            yield return SceneManager.LoadSceneAsync("Lumen");
            yield return null;
            var dialogue = Object.FindAnyObjectByType<ArvenDialogue>();
            var view = Object.FindAnyObjectByType<ChatView>();
            var slow = new SlowModel();
            typeof(ArvenDialogue).GetField("llm", Private).SetValue(dialogue, slow);

            NPCResponse reply = null;
            dialogue.Responded += r => reply = r;
            view.Open();
            typeof(ArvenDialogue).GetMethod("OnSubmitted", Private).Invoke(dialogue, new object[] { "Merhaba Arven." });

            // Model 1 sn sonra cevap verecek; bu sürede oyun kare çizmeye devam etmeli
            int frames = 0;
            bool thinkingShown = false;
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime)
            {
                thinkingShown |= view.transform.Find("Panel/Thinking").gameObject.activeSelf;
                frames++;
                yield return null;
            }

            Assert.IsNull(reply, "Model henüz cevap vermemişken cevap gelmemeli.");
            Assert.Greater(frames, 10, "Model beklenirken oyun donmamalı (kareler akmaya devam etmeli).");
            Assert.IsTrue(thinkingShown, "Beklerken \"Arven düşünüyor...\" görünmeli.");

            slow.Complete(Reply);
            yield return null;
            yield return null;
            Assert.IsNotNull(reply, "Model cevap verince cevap gösterilmeli.");
            Assert.AreEqual("Dinliyorum, yolcu.", reply.dialogue);
            view.Close();
        }

        /// <summary>Cevabı test istediğinde veren sahte model (gecikmeli ağ isteğinin yerine).</summary>
        private sealed class SlowModel : ILLMService
        {
            private AwaitableCompletionSource<string> source;

            public Awaitable<string> SendAsync(IReadOnlyList<ChatMessage> messages)
            {
                source = new AwaitableCompletionSource<string>();
                return source.Awaitable;
            }

            public void Complete(string reply) => source.SetResult(reply);
        }
    }
}
