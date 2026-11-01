using UnityEngine;

namespace Lumen.NPC
{
    /// <summary>
    /// Geçici test (Arven sahneye gelince C kısmı silinecek):
    /// C = Arven henüz sahnede olmadığı için sohbet panelini doğrudan açar.
    /// Y = Ollama'ya gitmeden ayrıştırıcıyı bozuk/hatalı örneklerle dener.
    /// </summary>
    public sealed class OllamaTest : MonoBehaviour
    {
        [SerializeField] private ChatView chat;

        public void Configure(ChatView view) => chat = view;

        private void Update()
        {
            // Panel açıkken oyuncu yazı yazıyordur; C ve Y harfleri test tetiklemesin
            if (chat.IsOpen)
                return;

            if (Input.GetKeyDown(KeyCode.C))
            {
                chat.Open();
            }

            if (Input.GetKeyDown(KeyCode.Y))
            {
                TestParser();
            }
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
