using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Lumen.NPC
{
    // Modelden gelen metni NPCResponse'a çevirir. Ne gelirse gelsin oyunu çökertmez:
    // boş, bozuk ya da kurala uymayan cevaplar güvenli varsayılanlara düşer.
    public static class NPCResponseParser
    {
        public const string OfflineMessage = "Arven derin bir sessizliğe gömüldü... (Yerel dil modeline ulaşılamadı.)";
        public const string ConfusedMessage = "Arven bir şeyler mırıldandı ama anlaşılmadı. (Model geçersiz bir yanıt verdi.)";

        public static NPCResponse Parse(string raw)
        {
            // 1) Hiç cevap yok: Ollama kapalı, zaman aşımı ya da bağlantı hatası (OllamaService null döner)
            if (string.IsNullOrWhiteSpace(raw))
            {
                return NPCResponse.Fallback(OfflineMessage);
            }

            // 2) Model JSON'u ```json ... ``` gibi fazladan metinle sarmış olabilir: ilk '{' ile son '}' arasını al
            int start = raw.IndexOf('{');
            int end = raw.LastIndexOf('}');
            if (start < 0 || end <= start)
            {
                return NPCResponse.Fallback(ConfusedMessage);
            }
            string json = raw.Substring(start, end - start + 1);

            // 3) JSON'u nesneye çevir; bozuksa Newtonsoft hata fırlatır, yakalayıp güvenli cevaba düşeriz
            NPCResponse response;
            try
            {
                response = JsonConvert.DeserializeObject<NPCResponse>(json);
            }
            catch (JsonException e)
            {
                Debug.LogWarning($"Geçersiz JSON: {e.Message}");
                return NPCResponse.Fallback(ConfusedMessage);
            }

            // 4) Alanları doğrula: söz yoksa geçersiz; bilinmeyen değerler varsayılana, güven sınır içine
            if (response == null || string.IsNullOrWhiteSpace(response.dialogue))
            {
                return NPCResponse.Fallback(ConfusedMessage);
            }
            response.mood = Allowed(response.mood, NPCResponse.Moods, "neutral");
            response.action = Allowed(response.action, NPCResponse.Actions, "none");
            response.trustDelta = Mathf.Clamp(response.trustDelta, -20, 20);
            return response;
        }

        // Değer izin verilen listede yoksa varsayılanı döner (büyük/küçük harf ve boşluk önemsenmez)
        private static string Allowed(string value, string[] allowed, string fallback)
        {
            string normalized = value?.Trim().ToLowerInvariant();
            return Array.IndexOf(allowed, normalized) >= 0 ? normalized : fallback;
        }
    }
}
