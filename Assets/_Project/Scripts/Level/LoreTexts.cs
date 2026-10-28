namespace Lumen.Level
{
    /// <summary>
    /// Ara bölgedeki yazıtların metinleri. Sahne bölümü bunları yazıtlara dağıtır;
    /// Arven (NPC) okunan yazıtları LoreJournal üzerinden sohbet bağlamına alır.
    /// </summary>
    public static class LoreTexts
    {
        public readonly struct Entry
        {
            public readonly string Title;
            public readonly string Text;

            public Entry(string title, string text)
            {
                Title = title;
                Text = text;
            }
        }

        public static readonly Entry[] All =
        {
            new Entry(
                "İlk Işığın Yazıtı",
                "Bu tapınak, yeryüzüne hiç çıkmayan bir halk tarafından ışığa adandı. " +
                "Üç kristal, Lumen'in ilk ışığını taşıdı. Kristaller sönünce tapınak karanlığa gömüldü " +
                "ve kapılar kapandı."),
            new Entry(
                "Bekçinin Yemini",
                "Taşçı Arven son kapının önünde yemin etti: \"Işık geri dönmeden kimse geçmeyecek.\" " +
                "Yıllar geçti, Arven taşa dönüştü ama yemini bozulmadı. " +
                "O hâlâ bekliyor ve konuşanı dinliyor."),
            new Entry(
                "Duvardaki Fısıltı",
                "Arven'i tehditle ya da hileyle kimse geçemedi. Ona kristallerin ışığını getirdiğini söyle, " +
                "ilk ışığın adını an. Taş bile hatırladığı şeye kulak verir."),
        };
    }
}
