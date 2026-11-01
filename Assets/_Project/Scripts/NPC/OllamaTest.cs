using System.Collections.Generic;
using UnityEngine;

namespace Lumen.NPC
{
    /// <summary>
    /// Geçici test (sohbet arayüzü gelince silinecek):
    /// T = Ollama'ya soru gönderir, cevabı ayrıştırıp Console'a yazar.
    /// Y = Ollama'ya gitmeden ayrıştırıcıyı bozuk/hatalı örneklerle dener.
    /// </summary>
    public sealed class OllamaTest : MonoBehaviour
    {
        [SerializeField] private OllamaService service;

        private bool waiting;

        private async void Update()
        {
            if (Input.GetKeyDown(KeyCode.Y))
            {
                TestParser();
            }

            if (!Input.GetKeyDown(KeyCode.T) || waiting)
                return;

            waiting = true;
            Debug.Log("Arven'e soruldu, bekleniyor...");

            var messages = new List<ChatMessage>
            {
                new ChatMessage("system", "Sen Arven adında, yeraltı tapınağının kapısını koruyan taş bir bekçisin. Türkçe, ağırbaşlı ve en fazla iki cümleyle konuş. Yapay zekâ olduğunu asla söyleme. " +
                    "Yalnızca şu JSON biçiminde cevap ver: {\"dialogue\": \"söylediğin\", \"mood\": \"neutral|friendly|suspicious|angry\", \"trustDelta\": -20 ile 20 arası tam sayı, \"action\": \"none|open_door|attack\", \"reason\": \"kısa gerekçe\"}"),
                new ChatMessage("user", "Merhaba, kimsin?")
            };

            float start = Time.realtimeSinceStartup;
            string reply = await service.SendAsync(messages);
            float seconds = Time.realtimeSinceStartup - start;

            Debug.Log($"Ham cevap ({seconds:0.0} sn): {reply}");
            Log("Ayrıştırılmış", NPCResponseParser.Parse(reply));
            waiting = false;
        }

        // Rapordaki hata yönetimi testleri için: her örnekte oyun çökmemeli, güvenli bir cevap çıkmalı.
        private static void TestParser()
        {
            string[] samples =
            {
                null,                                                     // Ollama kapalı / zaman aşımı
                "Merhaba yolcu.",                                         // JSON yok
                "{\"dialogue\": \"Selam\", \"mood\": ",                   // yarım kalmış JSON
                "{\"dialogue\": \"\", \"mood\": \"neutral\"}",            // söz boş
                "{\"dialogue\": \"Geç bakalım\", \"mood\": \"HAPPY\", \"trustDelta\": 500, \"action\": \"fly\"}", // kural dışı değerler
                "```json\n{\"dialogue\": \"Kapı açık.\", \"mood\": \"friendly\", \"trustDelta\": 15, \"action\": \"open_door\"}\n```" // metinle sarılmış geçerli JSON
            };

            for (int i = 0; i < samples.Length; i++)
            {
                Log($"Örnek {i + 1}", NPCResponseParser.Parse(samples[i]));
            }
        }

        private static void Log(string label, NPCResponse r)
        {
            Debug.Log($"{label}: \"{r.dialogue}\" | mood={r.mood} trust={r.trustDelta} action={r.action} reason={r.reason}");
        }
    }
}
