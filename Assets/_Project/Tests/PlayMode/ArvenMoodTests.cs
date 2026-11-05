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
    /// <summary>Arven'in gözleri modelin döndürdüğü ruh haline göre renk değiştirir.</summary>
    public class ArvenMoodTests
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

        [Test]
        public void UnknownMood_UsesCalmColor()
        {
            Assert.AreEqual(ArvenMoodView.ColorOf("neutral"), ArvenMoodView.ColorOf("happy"));
            Assert.AreNotEqual(ArvenMoodView.ColorOf("neutral"), ArvenMoodView.ColorOf("angry"));
        }

        [UnityTest]
        public IEnumerator AngryReply_TurnsEyesRed()
        {
            yield return SceneManager.LoadSceneAsync("Lumen");
            yield return null;
            var dialogue = Object.FindAnyObjectByType<ArvenDialogue>();
            var mood = Object.FindAnyObjectByType<ArvenMoodView>();
            Assert.IsNotNull(mood, "Arven'de göz rengi bileşeni yok.");

            typeof(ArvenDialogue).GetField("llm", Private).SetValue(dialogue, new FixedModel(
                "{\"dialogue\": \"Sabrımı sınama!\", \"mood\": \"angry\", \"trustDelta\": -10, \"action\": \"none\", \"reason\": \"test\"}"));
            typeof(ArvenDialogue).GetMethod("OnSubmitted", Private).Invoke(dialogue, new object[] { "Çekil önümden taş yığını." });
            yield return new WaitForSeconds(1f); // renk yumuşakça geçer

            Assert.AreEqual("angry", mood.CurrentMood);
            Light glow = mood.transform.Find("EyeGlow").GetComponent<Light>();
            Assert.Greater(glow.color.r, glow.color.b, "Öfkeli Arven'in göz ışığı kırmızıya dönmeli.");
        }

        private sealed class FixedModel : ILLMService
        {
            private readonly string reply;

            public FixedModel(string fixedReply) => reply = fixedReply;

            public Awaitable<string> SendAsync(IReadOnlyList<ChatMessage> messages)
            {
                var source = new AwaitableCompletionSource<string>();
                source.SetResult(reply);
                return source.Awaitable;
            }
        }
    }
}
