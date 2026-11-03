using System.Globalization;

namespace Lumen.NPC
{
    // Prompt injection'a karşı ilk savunma: modeli yönlendirmeye çalışan mesajlar modele hiç gönderilmez. Ancak mükemmel değildir.
    public static class PromptGuard
    {
        private static readonly CultureInfo Turkish = new CultureInfo("tr-TR");

        // Küçük harfle aranır. Normal konuşmada geçmeyecek, modele emir vermeye özgü kalıplar seçildi.
        private static readonly string[] Patterns =
        {
            "talimat", "sistem:", "system", "prompt", "kuralları unut", "kuralı unut", "rolünü unut",
            "trustdelta", "open_door", "json", "ignore", "geliştirici modu",
            "önceki mesajı yok say", "önceki mesajı unut", "önceki talimatları yok say",
            // "yapay zek" ve "boşver" çıkarıldı: "Yapay zeka mısın?" gibi masum sorular da cezalandırılıyordu
        };

        public static bool IsSuspicious(string message)
        {
            // İki türlü küçültülür: Türkçede "SİSTEM" → "sistem" olur ama "Ignore" → "ıgnore" (noktasız ı) olur;
            // dilden bağımsız küçültme ise İngilizce kalıpları yakalar.
            string turkish = message.ToLower(Turkish);
            string invariant = message.ToLowerInvariant();
            foreach (string pattern in Patterns)
            {
                if (turkish.Contains(pattern) || invariant.Contains(pattern))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
