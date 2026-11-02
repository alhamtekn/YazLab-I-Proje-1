using System;

namespace Lumen.NPC
{
    [Serializable]
    public sealed class ChatMessage
    {
        public string role;     // rolü
        public string content;  // içerik

        public ChatMessage(string role, string content)
        {
            this.role = role;
            this.content = content;
        }
    }
}