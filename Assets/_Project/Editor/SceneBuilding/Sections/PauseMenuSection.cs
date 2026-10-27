using Lumen.Menu;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Lumen.EditorTools.SceneBuilding.Sections
{
    /// <summary>Esc ile açılan duraklatma menüsü: Devam Et, Ses, Işık, Tam Ekran, Ana Menü.</summary>
    public sealed class PauseMenuSection : ISceneSection
    {
        private static readonly Color PanelColor = new Color(0.04f, 0.05f, 0.08f, 0.92f);
        private static readonly Color AccentColor = new Color(0.85f, 0.92f, 1f);

        public int Order => 40;

        public void Build(SceneBuildContext ctx)
        {
            Canvas canvas = HudSection.CreateCanvas("PauseMenu", 100);
            DefaultControls.Resources resources = LoadUiResources();

            GameObject dim = CreateImage("Dim", canvas.transform, new Color(0f, 0f, 0f, 0.55f));
            Stretch((RectTransform)dim.transform);

            GameObject panel = CreateImage("Panel", dim.transform, PanelColor);
            var panelRect = (RectTransform)panel.transform;
            panelRect.sizeDelta = new Vector2(560f, 520f);

            Text title = HudSection.CreateText(ctx, panel.transform, "Title", new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(500f, 60f), 40, TextAnchor.MiddleCenter);
            title.text = "Duraklatıldı";
            title.color = AccentColor;

            Button resume = CreateButton(ctx, resources, panel.transform, "Resume", "Devam Et", -120f);
            Slider volume = CreateLabeledSlider(ctx, resources, panel.transform, "Volume", "Ses", -200f);
            Slider brightness = CreateLabeledSlider(ctx, resources, panel.transform, "Brightness", "Işık", -270f);
            Toggle fullscreen = CreateLabeledToggle(ctx, resources, panel.transform, "Fullscreen", "Tam Ekran", -340f);
            Button mainMenu = CreateButton(ctx, resources, panel.transform, "MainMenu", "Ana Menü", -420f);

            Text status = HudSection.CreateText(ctx, panel.transform, "Status", new Vector2(0.5f, 0f), new Vector2(0f, 14f), new Vector2(500f, 30f), 20, TextAnchor.MiddleCenter);
            status.color = new Color(1f, 0.75f, 0.45f);

            PauseMenuController controller = canvas.gameObject.AddComponent<PauseMenuController>();
            controller.Configure(dim, resume, volume, brightness, fullscreen, mainMenu, status);
            ctx.Set(controller);
        }

        private static DefaultControls.Resources LoadUiResources()
        {
            return new DefaultControls.Resources
            {
                standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"),
                background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd"),
                knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"),
                checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd")
            };
        }

        private static GameObject CreateImage(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            return go;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void PlaceRow(RectTransform rect, float y, float width, float height)
        {
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, y);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static Button CreateButton(SceneBuildContext ctx, DefaultControls.Resources resources, Transform parent, string name, string label, float y)
        {
            GameObject go = DefaultControls.CreateButton(resources);
            go.name = name;
            go.transform.SetParent(parent, false);
            PlaceRow((RectTransform)go.transform, y, 360f, 52f);

            Text text = go.GetComponentInChildren<Text>();
            text.font = ctx.UiFont;
            text.fontSize = 26;
            text.text = label;
            return go.GetComponent<Button>();
        }

        private static Slider CreateLabeledSlider(SceneBuildContext ctx, DefaultControls.Resources resources, Transform parent, string name, string label, float y)
        {
            Transform row = CreateRow(ctx, parent, name, label, y);
            GameObject go = DefaultControls.CreateSlider(resources);
            go.name = "Slider";
            go.transform.SetParent(row, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(1f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(260f, 24f);
            return go.GetComponent<Slider>();
        }

        private static Toggle CreateLabeledToggle(SceneBuildContext ctx, DefaultControls.Resources resources, Transform parent, string name, string label, float y)
        {
            Transform row = CreateRow(ctx, parent, name, label, y);
            GameObject go = DefaultControls.CreateToggle(resources);
            go.name = "Toggle";
            go.transform.SetParent(row, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(1f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(40f, 40f);

            // Varsayılan toggle'ın kendi etiketi kullanılmıyor; etiket satırın solunda.
            Text builtInLabel = go.GetComponentInChildren<Text>();
            if (builtInLabel != null)
            {
                Object.DestroyImmediate(builtInLabel.gameObject);
            }

            Transform background = go.transform.Find("Background");
            if (background != null)
            {
                var bgRect = (RectTransform)background;
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.offsetMin = Vector2.zero;
                bgRect.offsetMax = Vector2.zero;
            }

            return go.GetComponent<Toggle>();
        }

        private static Transform CreateRow(SceneBuildContext ctx, Transform parent, string name, string label, float y)
        {
            var row = new GameObject(name, typeof(RectTransform));
            row.transform.SetParent(parent, false);
            PlaceRow((RectTransform)row.transform, y, 440f, 50f);

            Text text = HudSection.CreateText(ctx, row.transform, "Label", new Vector2(0f, 0.5f), Vector2.zero, new Vector2(160f, 40f), 26, TextAnchor.MiddleLeft);
            text.text = label;
            return row.transform;
        }
    }
}
