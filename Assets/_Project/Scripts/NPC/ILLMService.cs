using UnityEngine;
using System.Collections.Generic;

namespace Lumen.NPC


//dil modeline sohbeti gönderip cevap almayı sağlayan interface (ollama)
{
    public interface ILLMService
    {
        Awaitable<string> SendAsync(IReadOnlyList<ChatMessage> messages);
    }
}
