using System.Collections;
using Lumen.NPC;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Lumen.Tests
{
    /// <summary>Arven'in cevabı harf harf yazılır; arkasından gelen bildirim cevap bitince görünür.</summary>
    public class TypewriterTests
    {
        [UnityTest]
        public IEnumerator Reply_IsTypedLetterByLetter_ThenNoticeAppears()
        {
            yield return SceneManager.LoadSceneAsync("Lumen");
            yield return null;
            var view = Object.FindAnyObjectByType<ChatView>();
            var reply = new NPCResponse { dialogue = "Işık geri dönmeden kimse bu kapıdan geçemez, yolcu.", mood = "neutral" };

            view.ShowReply(reply);
            typeof(ChatView).GetMethod("ShowNotice", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .Invoke(view, new object[] { "Arven sana tam güvenmiyor." });
            yield return null;

            Assert.IsTrue(view.IsTyping, "Cevap hemen tamamı görünmemeli, harf harf yazılmalı.");
            StringAssert.DoesNotContain("güvenmiyor", view.HistoryText, "Bildirim cevap bitmeden görünmemeli.");

            yield return new WaitForSeconds(2.5f);
            Assert.IsFalse(view.IsTyping);
            StringAssert.Contains(reply.dialogue, view.HistoryText);
            StringAssert.Contains("güvenmiyor", view.HistoryText, "Cevap bitince bildirim görünmeli.");
        }
    }
}
