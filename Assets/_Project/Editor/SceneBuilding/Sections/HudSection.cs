using Lumen.Companion;
using Lumen.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Lumen.EditorTools.SceneBuilding.Sections
{
    /// <summary>Geliştirici 1 — HUD (kontroller, Lumi durumu, etkileşim ipucu) ve EventSystem.</summary>
    public sealed class HudSection : ISceneSection
    {
        public int Order => 30;

        public void Build(SceneBuildContext ctx)
        {
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();

            Canvas canvas = CreateCanvas("HUD", 0);
            ctx.Set(canvas);

            Text controls = CreateText(ctx, canvas.transform, "Controls", new Vector2(0f, 1f), new Vector2(24f, -24f), new Vector2(560f, 140f), 22, TextAnchor.UpperLeft);
            controls.text = "WASD: Hareket   Fare: Kamera\nF: Lumi'den yardım iste   G: Lumi beni takip etsin\nE: Etkileşim   Esc: Menü";
            controls.color = new Color(1f, 1f, 1f, 0.75f);

            Text state = CreateText(ctx, canvas.transform, "CompanionState", new Vector2(1f, 1f), new Vector2(-24f, -24f), new Vector2(520f, 40f), 24, TextAnchor.UpperRight);
            Text prompt = CreateText(ctx, canvas.transform, "Prompt", new Vector2(0.5f, 0f), new Vector2(0f, 140f), new Vector2(800f, 40f), 28, TextAnchor.MiddleCenter);
            Text feedback = CreateText(ctx, canvas.transform, "Feedback", new Vector2(0.5f, 0f), new Vector2(0f, 90f), new Vector2(1000f, 40f), 24, TextAnchor.MiddleCenter);
            feedback.color = new Color(0.85f, 0.93f, 1f);

            HudView hud = canvas.gameObject.AddComponent<HudView>();
            hud.Configure(state, prompt, feedback, ctx.Get<CompanionController>(), ctx.Get<PlayerInteractor>(), ctx.Get<PlayerCommandIssuer>());
            ctx.Set(hud);
        }

        public static Canvas CreateCanvas(string name, int sortingOrder)
        {
            var go = new GameObject(name);
            Canvas canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            CanvasScaler scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        public static Text CreateText(SceneBuildContext ctx, Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size, int fontSize, TextAnchor alignment)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            Text text = go.AddComponent<Text>();
            text.font = ctx.UiFont;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            go.AddComponent<Shadow>().effectColor = new Color(0f, 0f, 0f, 0.8f);
            return text;
        }
    }
}
