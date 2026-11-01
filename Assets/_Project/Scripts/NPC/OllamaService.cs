using System.Collections.Generic;
using UnityEngine;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

namespace Lumen.NPC
{
    /// ILLMService'in Ollama ile çalışan hâli. Yerel Ollama sunucusunun /api/chat adresine istek atar.
    public sealed class OllamaService : MonoBehaviour, ILLMService
    {
        [SerializeField] private string baseUrl = "http://localhost:11434";
        [SerializeField] private string model = "qwen2.5:7b";
        [SerializeField] private int timeoutSeconds = 60;

        public async Awaitable<string> SendAsync(IReadOnlyList<ChatMessage> messages)
        {
            // 1) İsteği hazırla: Ollama'nın beklediği JSON. "format" şemayı zorunlu kılar (structured output);
            //    yalnızca istemde JSON istemek yetmedi, model düz metin döndürebiliyordu.
            string json = JsonConvert.SerializeObject(new { model, stream = false, format = NPCResponse.Schema, messages });
            byte[] body = Encoding.UTF8.GetBytes(json);

            // 2) Gönder ve bekle
            using var request = new UnityWebRequest(baseUrl + "/api/chat", "POST");
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = timeoutSeconds;

            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"Ollama isteği başarısız: {request.error}");
                return null;
            }

            // 3) Cevabı oku: { "message": { "role": "assistant", "content": "..." } }
            //    Sunucu beklenmedik bir şey döndürürse Parse hata fırlatır; oyunu çökertmemek için yakalarız.
            try
            {
                JObject response = JObject.Parse(request.downloadHandler.text);
                return (string)response["message"]?["content"];
            }
            catch (JsonException e)
            {
                Debug.LogWarning($"Ollama cevabı okunamadı: {e.Message}");
                return null;
            }
        }
    }
}
