using Lumen.Level;
using Lumen.NPC;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Lumen.EditorTools.SceneBuilding.Sections
{
    /// <summary>
    /// Geliştirici 1 — "Şansını kaybettin" ekranı. Yazı tipi ve yazı düğmeleri ana menüdeki gibi (Cinzel, hover'da kayan
    /// yazı ve altın çizgi). Oyun durumu sohbet bölümünde (Order 35) kurulduğu için ondan sonra çalışır.
    /// </summary>
    public sealed class GameOverSection : ISceneSection
    {
        private static readonly Color Yellow = new Color(1f, 0.82f, 0.3f);

        public int Order => 44;

        public void Build(SceneBuildContext ctx)
        {
            GameStateManager gameState = ctx.Get<GameStateManager>();
            Font font = AssetDatabase.LoadAssetAtPath<Font>(MainMenuBuilder.FontPath);

            Canvas canvas = HudSection.CreateCanvas("GameOver", 60);
            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            panel.transform.SetParent(canvas.transform, false);
            PauseMenuSection.Stretch((RectTransform)panel.transform);
            panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.92f);

            Text title = HudSection.CreateText(ctx, panel.transform, "Title", new Vector2(0.5f, 0.5f), new Vector2(0f, 110f), new Vector2(1400f, 120f), 84, TextAnchor.MiddleCenter);
            title.font = font;
            title.text = "Şansını kaybettin";
            title.color = new Color(0.95f, 0.9f, 0.82f);

            Text reason = HudSection.CreateText(ctx, panel.transform, "Reason", new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(1200f, 50f), 30, TextAnchor.MiddleCenter);
            reason.font = font;
            reason.color = new Color(1f, 1f, 1f, 0.7f);

            Button retry = CenteredButton(ctx, panel.transform, font, "Retry", "Yeniden Oyna", -90f, 52, Yellow);
            Button mainMenu = CenteredButton(ctx, panel.transform, font, "MainMenu", "Ana Menü", -170f, 30, Color.white);

            canvas.gameObject.AddComponent<GameOverScreen>().Configure(gameState, panel.GetComponent<CanvasGroup>(), reason, retry, mainMenu);

            // Kaybetme sebebi verilmiş bütün bölgeler (lav, Lumi'nin parkuru) oyun durumuna bağlanır
            foreach (KillZone zone in Object.FindObjectsByType<KillZone>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (!string.IsNullOrEmpty(zone.LoseReason))
                {
                    zone.SetGameOver(gameState);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(zone); // ilk uçurumun lavı ortam prefab'ının içinde
                }
            }
        }

        // Ana menünün yazı düğmesi, ekranın ortasına taşınmış ve renklendirilmiş hâli
        private static Button CenteredButton(SceneBuildContext ctx, Transform parent, Font font, string name, string label, float y, int fontSize, Color color)
        {
            Button button = MainMenuBuilder.TextButton(ctx, parent, font, name, label, 0f);
            var rect = (RectTransform)button.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, y);

            Text text = button.GetComponentInChildren<Text>();
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;

            ColorBlock colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.4f);
            colors.selectedColor = color;
            button.colors = colors;
            return button;
        }
    }
}
