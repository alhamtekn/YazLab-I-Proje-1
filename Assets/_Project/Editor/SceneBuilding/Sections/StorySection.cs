using Lumen.Story;
using UnityEngine;
using UnityEngine.UI;

namespace Lumen.EditorTools.SceneBuilding.Sections
{
    /// <summary>
    /// Geliştirici 2 — giriş koridorundaki hikâye anlatımı (koridor Onur'un IntroSection'ında).
    /// z = -15, -10, -5'te koridoru kaplayan görünmez tetikler; metinler Arven'in bildikleri ve yazıtlarla uyumlu.
    /// Koridorun sonunda Lumi ile tanışma (CompanionMeeting) hikâyeyi devralır.
    /// </summary>
    public sealed class StorySection : ISceneSection
    {
        public static readonly (float z, string text)[] Lines =
        {
            (-15f, "Lumen... Yeryüzüne hiç çıkmayan bir halk, bu tapınağı yüzyıllar önce ışığa adamıştı."),
            (-10f, "Üç kristal ilk ışığı taşıyordu. Kristaller sönünce tapınak karanlığa gömüldü, kapıları kapandı."),
            (-5f, "Son kapıda taşçı Arven bekliyor: \"Işık geri dönmeden kimse geçmeyecek.\"\nAma yalnız değilsin..."),
        };

        public int Order => 33; // HUD (30) ve koridordan (32) sonra

        public void Build(SceneBuildContext ctx)
        {
            StoryView view = BuildView(ctx);
            Transform root = new GameObject("Story").transform;
            root.SetParent(ctx.Root("Level"), false);

            foreach ((float z, string text) in Lines)
            {
                GameObject trigger = ctx.Box("StoryTrigger", root, new Vector3(0f, 1f, z), new Vector3(IntroSection.HalfWidth * 2f, 2f, 0.6f), null, true, 2);
                trigger.GetComponent<Collider>().isTrigger = true;
                trigger.AddComponent<StoryTrigger>().Configure(view, text);
            }
        }

        // Alt ortada, gölgeli, büyük italik metin; HUD'dan ayrı tuval (sohbet ve menünün altında kalır)
        private static StoryView BuildView(SceneBuildContext ctx)
        {
            Canvas canvas = HudSection.CreateCanvas("Story", 5);
            CanvasGroup group = canvas.gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.interactable = false;

            Text text = HudSection.CreateText(ctx, canvas.transform, "StoryText", new Vector2(0.5f, 0f), new Vector2(0f, 220f), new Vector2(1300f, 140f), 34, TextAnchor.MiddleCenter);
            text.fontStyle = FontStyle.Italic;
            text.color = new Color(1f, 0.95f, 0.85f);

            StoryView view = canvas.gameObject.AddComponent<StoryView>();
            view.Configure(text, group);
            return view;
        }
    }
}
