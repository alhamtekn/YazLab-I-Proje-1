using Lumen.Level;
using Lumen.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Lumen.EditorTools.SceneBuilding.Sections
{
    /// <summary>Geliştirici 1 — yazıt okuma paneli. MiddleZoneSection'ın yazıtlarına bağlanır.</summary>
    public sealed class LoreReaderSection : ISceneSection
    {
        public int Order => 31;

        public void Build(SceneBuildContext ctx)
        {
            if (!ctx.TryGet(out LoreInscription[] inscriptions))
            {
                return;
            }

            Canvas canvas = HudSection.CreateCanvas("LoreReader", 5);

            var panel = new GameObject("Panel", typeof(RectTransform));
            panel.transform.SetParent(canvas.transform, false);
            var rect = (RectTransform)panel.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(900f, 460f);
            Image background = panel.AddComponent<Image>();
            background.color = new Color(0.04f, 0.05f, 0.08f, 0.92f);
            panel.AddComponent<Outline>().effectColor = new Color(0.45f, 0.7f, 1f, 0.6f);

            Text title = HudSection.CreateText(ctx, panel.transform, "Title", new Vector2(0.5f, 1f), new Vector2(0f, -36f), new Vector2(800f, 50f), 34, TextAnchor.UpperCenter);
            title.color = new Color(0.6f, 0.82f, 1f);
            Text body = HudSection.CreateText(ctx, panel.transform, "Body", new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(780f, 260f), 26, TextAnchor.UpperLeft);
            body.lineSpacing = 1.15f;
            Text hint = HudSection.CreateText(ctx, panel.transform, "Hint", new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(800f, 36f), 20, TextAnchor.LowerCenter);
            hint.text = "[E] Kapat";
            hint.color = new Color(1f, 1f, 1f, 0.6f);

            LoreReaderView view = canvas.gameObject.AddComponent<LoreReaderView>();
            view.Configure(panel, title, body, inscriptions);
            ctx.Set(view);
        }
    }
}
