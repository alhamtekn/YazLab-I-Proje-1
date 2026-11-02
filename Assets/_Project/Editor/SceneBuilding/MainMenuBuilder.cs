using System.IO;
using Lumen.EditorTools.SceneBuilding.Sections;
using Lumen.Menu;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Lumen.EditorTools.SceneBuilding
{
    /// <summary>
    /// Geliştirici 2 — ana menü sahnesi (Assets/_Project/Scenes/MainMenu.unity). Tasarım: siyah zemin, beyaz yazı;
    /// solda "Lumen", Oyna, Ayarlar ve isimler, sağda ışınları dönen bir ışık küresi ve loş hale.
    /// LumenSceneBuilder her çalıştığında bu sahneyi de yeniden üretir.
    /// </summary>
    public static class MainMenuBuilder
    {
        public const string ScenePath = LumenSceneBuilder.SceneFolder + "/MainMenu.unity";
        private const string TextureFolder = "Assets/_Project/Textures";
        private const string GlowPath = TextureFolder + "/Glow.png";

        public static void Build()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var ctx = new SceneBuildContext();
            Sprite glow = EnsureGlowSprite();

            var cameraGo = new GameObject("Main Camera") { tag = "MainCamera" };
            Camera cam = cameraGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cameraGo.AddComponent<AudioListener>();

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();

            Canvas canvas = HudSection.CreateCanvas("MainMenu", 0);
            Transform root = canvas.transform;

            Text title = HudSection.CreateText(ctx, root, "Title", new Vector2(0f, 1f), new Vector2(140f, -120f), new Vector2(900f, 260f), 200, TextAnchor.UpperLeft);
            title.text = "Lumen";

            Button play = TextButton(ctx, root, "Play", "Oyna", -470f);
            Button settings = TextButton(ctx, root, "Settings", "Ayarlar", -570f);

            Text credits = HudSection.CreateText(ctx, root, "Credits", new Vector2(0f, 1f), new Vector2(150f, -760f), new Vector2(800f, 110f), 32, TextAnchor.UpperLeft);
            credits.text = "Onur TOPBAŞ\nAli Hamza TEKİNBAŞ";
            credits.color = new Color(1f, 1f, 1f, 0.8f);

            Text footer = HudSection.CreateText(ctx, root, "Footer", new Vector2(0.5f, 0f), new Vector2(0f, 36f), new Vector2(1000f, 40f), 24, TextAnchor.MiddleCenter);
            footer.text = "© Yazılım Geliştirme Laboratuvarı";
            footer.color = new Color(1f, 1f, 1f, 0.55f);

            BuildOrb(root, glow);

            DefaultControls.Resources resources = PauseMenuSection.LoadUiResources();
            GameObject panel = PauseMenuSection.CreateImage("SettingsPanel", root, new Color(0f, 0f, 0f, 0.95f));
            ((RectTransform)panel.transform).sizeDelta = new Vector2(560f, 440f);
            panel.AddComponent<Outline>().effectColor = new Color(1f, 1f, 1f, 0.6f);
            Text panelTitle = HudSection.CreateText(ctx, panel.transform, "Title", new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(500f, 60f), 40, TextAnchor.MiddleCenter);
            panelTitle.text = "Ayarlar";
            Slider volume = PauseMenuSection.CreateLabeledSlider(ctx, resources, panel.transform, "Volume", "Ses", -130f);
            Slider brightness = PauseMenuSection.CreateLabeledSlider(ctx, resources, panel.transform, "Brightness", "Işık", -200f);
            Toggle fullscreen = PauseMenuSection.CreateLabeledToggle(ctx, resources, panel.transform, "Fullscreen", "Tam Ekran", -270f);
            Button back = PauseMenuSection.CreateButton(ctx, resources, panel.transform, "Back", "Geri", -370f);

            canvas.gameObject.AddComponent<MainMenuController>().Configure(play, settings, panel, volume, brightness, fullscreen, back);

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log("[MainMenuBuilder] Ana menü oluşturuldu: " + ScenePath);
        }

        // Arka planı olmayan, yalnızca yazıdan oluşan düğme (tasarımdaki "Oyna", "Ayarlar")
        private static Button TextButton(SceneBuildContext ctx, Transform parent, string name, string label, float y)
        {
            Text text = HudSection.CreateText(ctx, parent, name, new Vector2(0f, 1f), new Vector2(150f, y), new Vector2(420f, 90f), 64, TextAnchor.MiddleLeft);
            text.text = label;
            text.raycastTarget = true; // tıklanabilsin

            Button button = text.gameObject.AddComponent<Button>();
            button.targetGraphic = text;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.6f, 0.85f, 1f); // üzerine gelince ışık mavisi
            colors.pressedColor = new Color(0.4f, 0.7f, 1f);
            colors.selectedColor = Color.white;
            button.colors = colors;
            return button;
        }

        // Sağdaki ışık küresi: loş hale + dönen ışınlar + parlak çekirdek
        private static void BuildOrb(Transform parent, Sprite glow)
        {
            var orb = new GameObject("LightOrb", typeof(RectTransform));
            orb.transform.SetParent(parent, false);
            var orbRect = (RectTransform)orb.transform;
            orbRect.anchorMin = orbRect.anchorMax = new Vector2(1f, 0.5f);
            orbRect.anchoredPosition = new Vector2(-480f, 20f);
            orbRect.sizeDelta = new Vector2(800f, 800f);

            Image halo = SpriteImage("Halo", orb.transform, glow, new Vector2(820f, 820f), new Color(0.75f, 0.85f, 1f, 1f));
            CanvasGroup haloGroup = halo.gameObject.AddComponent<CanvasGroup>();

            var rays = new GameObject("Rays", typeof(RectTransform));
            rays.transform.SetParent(orb.transform, false);
            for (int i = 0; i < 16; i++)
            {
                var ray = new GameObject("Ray", typeof(RectTransform), typeof(Image));
                ray.transform.SetParent(rays.transform, false);
                var rect = (RectTransform)ray.transform;
                float angle = i * 360f / 16f;
                rect.localRotation = Quaternion.Euler(0f, 0f, angle);
                rect.anchoredPosition = Quaternion.Euler(0f, 0f, angle) * new Vector3(0f, 230f, 0f);
                rect.sizeDelta = new Vector2(5f, i % 2 == 0 ? 120f : 80f);
                ray.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.75f);
                ray.GetComponent<Image>().raycastTarget = false;
            }

            Image core = SpriteImage("Core", orb.transform, glow, new Vector2(320f, 320f), Color.white);
            SpriteImage("CoreSolid", core.transform, AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"), new Vector2(130f, 130f), Color.white);

            orb.AddComponent<LightOrbView>().Configure((RectTransform)rays.transform, (RectTransform)core.transform, haloGroup);
        }

        private static Image SpriteImage(string name, Transform parent, Sprite sprite, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            ((RectTransform)go.transform).sizeDelta = size;
            Image image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        // Merkezden kenara yumuşakça sönen beyaz daire dokusu; ilk seferde üretilip PNG olarak kaydedilir
        private static Sprite EnsureGlowSprite()
        {
            if (!File.Exists(GlowPath))
            {
                SceneBuildContext.EnsureFolder(TextureFolder);
                const int size = 256;
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float d = Vector2.Distance(new Vector2(x, y), new Vector2(size / 2f, size / 2f)) / (size / 2f);
                        float a = Mathf.Pow(Mathf.Clamp01(1f - d), 2.2f);
                        texture.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                    }
                }

                File.WriteAllBytes(GlowPath, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(GlowPath);
                var importer = (TextureImporter)AssetImporter.GetAtPath(GlowPath);
                importer.textureType = TextureImporterType.Sprite;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(GlowPath);
        }
    }
}
