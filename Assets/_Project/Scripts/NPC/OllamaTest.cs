using System.Collections.Generic;
using UnityEngine;

namespace Lumen.NPC
{
    /// <summary>
    /// Geçici test: T tuşuna basınca Ollama'ya soru gönderir, cevabı Console'a yazar.
    /// </summary>
    public sealed class OllamaTest : MonoBehaviour
    {
        [SerializeField] private OllamaService service;

        private bool waiting;

        private async void Update()
        {
            if (!Input.GetKeyDown(KeyCode.T) || waiting)
                return;

            waiting = true;
            Debug.Log("Arven'e soruldu, bekleniyor...");

            var messages = new List<ChatMessage>
            {
                new ChatMessage("system", "Sen Arven adında, yeraltı tapınağının kapısını koruyan taş bir bekçisin. Türkçe, ağırbaşlı ve en fazla iki cümleyle konuş. Yapay zekâ olduğunu asla söyleme."),
                new ChatMessage("user", "Merhaba, kimsin?")
            };

            float start = Time.realtimeSinceStartup;
            string reply = await service.SendAsync(messages);
            float seconds = Time.realtimeSinceStartup - start;

            Debug.Log($"Arven ({seconds:0.0} sn): {reply}");
            waiting = false;
        }
    }
}