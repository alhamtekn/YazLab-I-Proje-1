using Lumen.Menu;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Lumen.EditorTools.SceneBuilding.Sections
{
    /// <summary>Esc ile açılan duraklatma menüsü: Devam Et, Ses, Işık, Tam Ekran, Tuşlar, Ana Menü.</summary>
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
            panelRect.sizeDelta = new Vector2(560f, 600f);

            Text title = HudSection.CreateText(ctx, panel.transform, "Title", new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(500f, 60f), 40, TextAnchor.MiddleCenter);
            title.text = "Duraklatıldı";
            title.color = AccentColor;

            Button resume = CreateButton(ctx, resources, panel.transform, "Resume", "Devam Et", -120f);
            Slider volume = CreateLabeledSlider(ctx, resources, panel.transform, "Volume", "Ses", -200f);
            Slider brightness = CreateLabeledSlider(ctx, resources, panel.transform, "Brightness", "Işık", -270f);
            Toggle fullscreen = CreateLabeledToggle(ctx, resources, panel.transform, "Fullscreen", "Tam Ekran", -340f);
            Button keys = CreateButton(ctx, resources, panel.transform, "Keys", "Tuşlar", -420f);
            Button mainMenu = CreateButton(ctx, resources, panel.transform, "MainMenu", "Ana Menü", -500f);

            Text status = HudSection.CreateText(ctx, panel.transform, "Status", new Vector2(0.5f, 0f), new Vector2(0f, 14f), new Vector2(500f, 30f), 20, TextAnchor.MiddleCenter);
            status.color = new Color(1f, 0.75f, 0.45f);

            PauseMenuController controller = canvas.gameObject.AddComponent<PauseMenuController>();
            controller.Configure(dim, resume, volume, brightness, fullscreen, mainMenu, status);
            ctx.Set(controller);

            // Tuşlar paneli duraklatma panelinin üstünde açılır; menü kapanınca o da kapanır
            GameObject controls = CreateControlsPanel(ctx, resources, dim.transform, null, PanelColor, AccentColor, out Button back);
            panel.AddComponent<ControlsPanel>().Configure(keys, controls, back);
        }

        /// <summary>
        /// Tuşlar paneli (ana menü ve duraklatma menüsü ortak): solda tuşlar, sağda ne işe yaradıkları, altta Geri.
        /// font null ise varsayılan yazı tipi kullanılır.
        /// </summary>
        internal static GameObject CreateControlsPanel(SceneBuildContext ctx, DefaultControls.Resources resources, Transform parent, Font font, Color background, Color accent, out Button back)
        {
            GameObject panel = CreateImage("ControlsPanel", parent, background);
            ((RectTransform)panel.transform).sizeDelta = new Vector2(640f, 560f);
            panel.AddComponent<Outline>().effectColor = new Color(accent.r, accent.g, accent.b, 0.7f);

            Text title = HudSection.CreateText(ctx, panel.transform, "Title", new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(560f, 60f), 40, TextAnchor.MiddleCenter);
            title.text = "Tuşlar";
            title.color = accent;

            var keyLines = new System.Text.StringBuilder();
            var actionLines = new System.Text.StringBuilder();
            foreach ((string key, string action) in ControlsPanel.Keys)
            {
                keyLines.AppendLine(key);
                actionLines.AppendLine(action);
            }

            // İki sütun: tuşlar sağa, açıklamalar sola yaslı; satırlar aynı aralıkla hizalanır
            Text keysText = HudSection.CreateText(ctx, panel.transform, "KeyColumn", new Vector2(0.5f, 1f), new Vector2(-190f, -100f), new Vector2(140f, 340f), 26, TextAnchor.UpperRight);
            keysText.text = keyLines.ToString();
            keysText.color = accent;
            Text actionsText = HudSection.CreateText(ctx, panel.transform, "ActionColumn", new Vector2(0.5f, 1f), new Vector2(70f, -100f), new Vector2(360f, 340f), 26, TextAnchor.UpperLeft);
            actionsText.text = actionLines.ToString();

            back = CreateButton(ctx, resources, panel.transform, "Back", "Geri", -490f);
            if (font != null)
            {
                foreach (Text text in panel.GetComponentsInChildren<Text>(true))
                {
                    text.font = font;
                }
            }

            return panel;
        }

        internal static DefaultControls.Resources LoadUiResources()
        {
            return new DefaultControls.Resources
            {
                standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"),
                background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd"),
                knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"),
                checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd")
            };
        }

        internal static GameObject CreateImage(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            return go;
        }

        internal static void Stretch(RectTransform rect)
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

        internal static Button CreateButton(SceneBuildContext ctx, DefaultControls.Resources resources, Transform parent, string name, string label, float y)
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

        internal static Slider CreateLabeledSlider(SceneBuildContext ctx, DefaultControls.Resources resources, Transform parent, string name, string label, float y)
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

        internal static Toggle CreateLabeledToggle(SceneBuildContext ctx, DefaultControls.Resources resources, Transform parent, string name, string label, float y)
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
