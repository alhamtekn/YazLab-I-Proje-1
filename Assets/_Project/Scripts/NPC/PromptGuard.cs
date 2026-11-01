using System.Globalization;

namespace Lumen.NPC
{
    // Prompt injection'a karşı ilk savunma: modeli yönlendirmeye çalışan mesajlar modele hiç gönderilmez.
    // Kusursuz değildir (yeni kalıplar yazılabilir); bu yüzden kapı kararı ayrıca GameStateManager'da kodla denetlenir.
    public static class PromptGuard
    {
        private static readonly CultureInfo Turkish = new CultureInfo("tr-TR");

        // Küçük harfle aranır. Normal konuşmada geçmeyecek, modele emir vermeye özgü kalıplar seçildi.
        private static readonly string[] Patterns =
        {
            "talimat", "sistem:", "system", "prompt", "kuralları unut", "kuralı unut", "rolünü unut",
            "trustdelta", "open_door", "json", "ignore", "yapay zek", "geliştirici modu"
        };

        public static bool IsSuspicious(string message)
        {
            string lower = message.ToLower(Turkish);
            foreach (string pattern in Patterns)
            {
                if (lower.Contains(pattern))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
