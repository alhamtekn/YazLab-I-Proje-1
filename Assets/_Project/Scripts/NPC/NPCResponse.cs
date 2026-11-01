using System;

namespace Lumen.NPC
{
    // Arven'in (LLM) her cevabı bu sınıfa çevrilir. Alan adları, modelden istenen JSON'daki adlarla aynıdır:
    // { "dialogue": "...", "mood": "neutral", "trustDelta": 5, "action": "none", "reason": "..." }
    [Serializable]
    public sealed class NPCResponse
    {
        public string dialogue;  // Arven'in oyuncuya söylediği cümle
        public string mood;      // ruh hâli: neutral | friendly | suspicious | angry
        public int trustDelta;   // güven puanındaki değişim: -20 ile +20 arası
        public string action;    // oyun durumunu değiştiren eylem: none | open_door | attack
        public string reason;    // modelin kısa gerekçesi (hata ayıklama ve rapor için)

        // İzin verilen değerler: hem şemada hem ayrıştırıcıdaki doğrulamada bu listeler kullanılır
        public static readonly string[] Moods = { "neutral", "friendly", "suspicious", "angry" };
        public static readonly string[] Actions = { "none", "open_door", "attack" };

        // Ollama'ya "format" olarak gönderilen JSON şeması: model yalnızca bu yapıda cevap üretebilir.
        // (C#'ta enum ayrılmış kelime olduğu için @enum yazılır; JSON'a "enum" olarak geçer.)
        public static readonly object Schema = new
        {
            type = "object",
            properties = new
            {
                dialogue = new { type = "string" },
                mood = new { type = "string", @enum = Moods },
                trustDelta = new { type = "integer", minimum = -20, maximum = 20 },
                action = new { type = "string", @enum = Actions },
                reason = new { type = "string" }
            },
            required = new[] { "dialogue", "mood", "trustDelta", "action", "reason" }
        };

        // Model cevap veremezse ya da bozuk JSON gelirse kullanılacak güvenli cevap:
        // Arven bir şey söyler ama oyun durumu değişmez (güven 0, eylem yok).
        public static NPCResponse Fallback(string message)
        {
            return new NPCResponse
            {
                dialogue = message,
                mood = "neutral",
                trustDelta = 0,
                action = "none",
                reason = "fallback"
            };
        }
    }
}
