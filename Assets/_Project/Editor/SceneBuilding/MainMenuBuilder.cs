using System;
using System.IO;
using Lumen.EditorTools.SceneBuilding.Sections;
using Lumen.Menu;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Lumen.EditorTools.SceneBuilding
{
    /// <summary>
    /// Geliştirici 2 — ana menü sahnesi (Assets/_Project/Scenes/MainMenu.unity). Tasarım: siyah zemin, beyaz yazı;
    /// solda "Lumen", alt başlık, Oyna / Ayarlar / Çıkış ve isimler, sağda parlayan bir ışık küresi ve çevresinde yavaşça genişleyen soluk bir halka.
    /// Arkada çok loş taş duvar dokusu ve kenarları karartan gölge, yükselen ışık zerreleri ve menü müziği vardır.
    /// Yazı tipi Cinzel (SIL Open Font License, Assets/_Project/Fonts/OFL.txt).
    /// LumenSceneBuilder her çalıştığında bu sahneyi de yeniden üretir.
    /// </summary>
    public static class MainMenuBuilder
    {
        public const string ScenePath = LumenSceneBuilder.SceneFolder + "/MainMenu.unity";
        private const string TextureFolder = "Assets/_Project/Textures";
        private const string GlowPath = TextureFolder + "/Glow.png";
        private const string CorePath = TextureFolder + "/OrbCore.png";
        private const string RingPath = TextureFolder + "/OrbRing.png";
        private const string BackgroundPath = TextureFolder + "/MenuBackground.png";
        internal const string FontPath = "Assets/_Project/Fonts/Cinzel.ttf";

        private static readonly Color Gold = new Color(0.86f, 0.74f, 0.48f);

        public static void Build()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var ctx = new SceneBuildContext();
            Sprite glow = EnsureSprite(GlowPath, 512, GlowPixel);
            Sprite core = EnsureSprite(CorePath, 512, CorePixel);
            Sprite ring = EnsureSprite(RingPath, 512, RingPixel);
            Sprite background = EnsureSprite(BackgroundPath, 512, BackgroundPixel);
            Font font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);

            var cameraGo = new GameObject("Main Camera") { tag = "MainCamera" };
            Camera cam = cameraGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cameraGo.AddComponent<AudioListener>();

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
            MusicGenerator.AddMusic("Music", MusicGenerator.MenuMusic(), 0.5f);

            Canvas canvas = HudSection.CreateCanvas("MainMenu", 0);
            Transform root = canvas.transform;

            Image bg = PauseMenuSection.CreateImage("Background", root, Color.white).GetComponent<Image>();
            bg.sprite = background;
            bg.raycastTarget = false;
            PauseMenuSection.Stretch((RectTransform)bg.transform);
            BuildMotes(root, glow);
            BuildOrb(root, glow, core, ring);

            Text title = Label(ctx, root, font, "Title", new Vector2(140f, -110f), new Vector2(1000f, 240f), 170, "Lumen");
            Text subtitle = Label(ctx, root, font, "Subtitle", new Vector2(150f, -330f), new Vector2(900f, 50f), 30, "I Ş I Ğ I N   T A P I N A Ğ I");
            subtitle.color = Gold;
            Image rule = PauseMenuSection.CreateImage("Rule", root, new Color(Gold.r, Gold.g, Gold.b, 0.6f)).GetComponent<Image>();
            Place((RectTransform)rule.transform, new Vector2(150f, -392f), new Vector2(420f, 2f));

            Button play = TextButton(ctx, root, font, "Play", "Oyna", -470f);
            Button settings = TextButton(ctx, root, font, "Settings", "Ayarlar", -560f);
            Button keys = TextButton(ctx, root, font, "Keys", "Tuşlar", -650f);
            Button quit = TextButton(ctx, root, font, "Quit", "Çıkış", -740f);

            Text credits = Label(ctx, root, font, "Credits", new Vector2(150f, -870f), new Vector2(800f, 100f), 26, "Onur TOPBAŞ\nAli Hamza TEKİNBAŞ");
            credits.color = new Color(1f, 1f, 1f, 0.7f);
            credits.lineSpacing = 1.3f;

            Text footer = HudSection.CreateText(ctx, root, "Footer", new Vector2(0.5f, 0f), new Vector2(0f, 36f), new Vector2(1000f, 40f), 20, TextAnchor.MiddleCenter);
            footer.font = font;
            footer.text = "© Yazılım Geliştirme Laboratuvarı";
            footer.color = new Color(1f, 1f, 1f, 0.45f);

            GameObject panel = BuildSettingsPanel(ctx, root, font, out Slider volume, out Slider brightness, out Toggle fullscreen, out Button back);
            GameObject controls = PauseMenuSection.CreateControlsPanel(ctx, PauseMenuSection.LoadUiResources(), root, font, new Color(0.02f, 0.02f, 0.03f, 0.96f), Gold, out Button controlsBack);
            canvas.gameObject.AddComponent<ControlsPanel>().Configure(keys, controls, controlsBack);

            // En üstte siyah perde: menü bununla açılır, Oyna'ya basınca bununla kararır
            GameObject fade = PauseMenuSection.CreateImage("Fader", root, Color.black);
            PauseMenuSection.Stretch((RectTransform)fade.transform);
            CanvasGroup fader = fade.AddComponent<CanvasGroup>();

            canvas.gameObject.AddComponent<MainMenuController>().Configure(play, settings, quit, panel, volume, brightness, fullscreen, back, fader);

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log("[MainMenuBuilder] Ana menü oluşturuldu: " + ScenePath);
        }

        // Sol üst köşeye göre yerleşen Cinzel yazı
        private static Text Label(SceneBuildContext ctx, Transform parent, Font font, string name, Vector2 position, Vector2 size, int fontSize, string text)
        {
            Text label = HudSection.CreateText(ctx, parent, name, new Vector2(0f, 1f), position, size, fontSize, TextAnchor.UpperLeft);
            label.font = font;
            label.text = text;
            return label;
        }

        private static void Place(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        // Arka planı olmayan yazı düğmesi; üzerine gelince yazı kayar ve solunda altın çizgi belirir
        internal static Button TextButton(SceneBuildContext ctx, Transform parent, Font font, string name, string label, float y)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            Place((RectTransform)go.transform, new Vector2(110f, y), new Vector2(460f, 80f));
            Image hitArea = go.GetComponent<Image>();
            hitArea.color = Color.clear; // görünmez ama tıklanabilir alan

            GameObject marker = PauseMenuSection.CreateImage("Marker", go.transform, Gold);
            var markerRect = (RectTransform)marker.transform;
            markerRect.anchorMin = markerRect.anchorMax = new Vector2(0f, 0.5f);
            markerRect.anchoredPosition = new Vector2(30f, 0f);
            markerRect.sizeDelta = new Vector2(26f, 2f);
            marker.GetComponent<Image>().raycastTarget = false;
            CanvasGroup markerGroup = marker.AddComponent<CanvasGroup>();

            Text text = HudSection.CreateText(ctx, go.transform, "Label", new Vector2(0f, 0.5f), new Vector2(40f, 0f), new Vector2(420f, 80f), 52, TextAnchor.MiddleLeft);
            text.font = font;
            text.text = label;

            Button button = go.AddComponent<Button>();
            button.targetGraphic = text;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.92f, 0.75f); // üzerine gelince sıcak beyaz
            colors.pressedColor = Gold;
            colors.selectedColor = Color.white;
            button.colors = colors;
            go.AddComponent<MenuButtonHover>().Configure((RectTransform)text.transform, markerGroup);
            return button;
        }

        private static GameObject BuildSettingsPanel(SceneBuildContext ctx, Transform root, Font font, out Slider volume, out Slider brightness, out Toggle fullscreen, out Button back)
        {
            DefaultControls.Resources resources = PauseMenuSection.LoadUiResources();
            GameObject panel = PauseMenuSection.CreateImage("SettingsPanel", root, new Color(0.02f, 0.02f, 0.03f, 0.96f));
            ((RectTransform)panel.transform).sizeDelta = new Vector2(560f, 440f);
            panel.AddComponent<Outline>().effectColor = new Color(Gold.r, Gold.g, Gold.b, 0.7f);
            Text panelTitle = HudSection.CreateText(ctx, panel.transform, "Title", new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(500f, 60f), 40, TextAnchor.MiddleCenter);
            panelTitle.font = font;
            panelTitle.text = "Ayarlar";
            panelTitle.color = Gold;
            volume = PauseMenuSection.CreateLabeledSlider(ctx, resources, panel.transform, "Volume", "Ses", -130f);
            brightness = PauseMenuSection.CreateLabeledSlider(ctx, resources, panel.transform, "Brightness", "Işık", -200f);
            fullscreen = PauseMenuSection.CreateLabeledToggle(ctx, resources, panel.transform, "Fullscreen", "Tam Ekran", -270f);
            back = PauseMenuSection.CreateButton(ctx, resources, panel.transform, "Back", "Geri", -370f);
            foreach (Text text in panel.GetComponentsInChildren<Text>(true))
            {
                text.font = font;
            }

            return panel;
        }

        // Sağdaki ışık küresi: loş hale + genişleyen ışık halkası + parlak çekirdek
        private static void BuildOrb(Transform parent, Sprite glow, Sprite coreDisc, Sprite ringSprite)
        {
            var orb = new GameObject("LightOrb", typeof(RectTransform));
            orb.transform.SetParent(parent, false);
            var orbRect = (RectTransform)orb.transform;
            orbRect.anchorMin = orbRect.anchorMax = new Vector2(1f, 0.5f);
            orbRect.anchoredPosition = new Vector2(-480f, 20f);
            orbRect.sizeDelta = new Vector2(800f, 800f);

            Image halo = SpriteImage("Halo", orb.transform, glow, new Vector2(900f, 900f), new Color(0.75f, 0.85f, 1f, 1f));
            CanvasGroup haloGroup = halo.gameObject.AddComponent<CanvasGroup>();

            // Soluk ışık halkası: LightOrbView onu yavaşça büyütüp söndürür
            Image ring = SpriteImage("Ring", orb.transform, ringSprite, new Vector2(600f, 600f), new Color(0.85f, 0.92f, 1f, 1f));
            CanvasGroup ringGroup = ring.gameObject.AddComponent<CanvasGroup>();

            Image core = SpriteImage("Core", orb.transform, glow, new Vector2(320f, 320f), Color.white);
            SpriteImage("CoreSolid", core.transform, coreDisc, new Vector2(130f, 130f), Color.white);

            orb.AddComponent<LightOrbView>().Configure((RectTransform)core.transform, haloGroup, (RectTransform)ring.transform, ringGroup);
        }

        // Ekranda yavaşça yükselen 40 küçük ışık zerresi
        private static void BuildMotes(Transform parent, Sprite glow)
        {
            var layer = new GameObject("Motes", typeof(RectTransform));
            layer.transform.SetParent(parent, false);
            var random = new System.Random(3);
            var motes = new RectTransform[40];
            for (int i = 0; i < motes.Length; i++)
            {
                float size = 6f + (float)random.NextDouble() * 14f;
                float alpha = 0.15f + (float)random.NextDouble() * 0.35f;
                motes[i] = (RectTransform)SpriteImage("Mote", layer.transform, glow, new Vector2(size, size), new Color(1f, 0.93f, 0.8f, alpha)).transform;
            }

            layer.AddComponent<DustMotes>().Configure(motes);
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

        // Merkezden kenara yumuşakça sönen beyaz daire
        private static Color GlowPixel(float u, float v)
        {
            float d = Vector2.Distance(new Vector2(u, v), new Vector2(0.5f, 0.5f)) * 2f;
            return new Color(1f, 1f, 1f, Mathf.Pow(Mathf.Clamp01(1f - d), 2.2f));
        }

        // Dolu beyaz daire; kenarı birkaç pikselde yumuşakça biter (çerçeve çizgisi yok, büyütünce de pürüzsüz)
        private static Color CorePixel(float u, float v)
        {
            float d = Vector2.Distance(new Vector2(u, v), new Vector2(0.5f, 0.5f)) * 2f;
            return new Color(1f, 1f, 1f, Mathf.Clamp01((1f - d) * 80f));
        }

        // İnce, iki yana yumuşakça sönen halka (yarıçapın %90'ında)
        private static Color RingPixel(float u, float v)
        {
            float d = Vector2.Distance(new Vector2(u, v), new Vector2(0.5f, 0.5f)) * 2f;
            return new Color(1f, 1f, 1f, Mathf.Exp(-Mathf.Pow((d - 0.9f) / 0.03f, 2f)));
        }

        // Çok loş taş sıraları (her sıra yarım taş kaydırılmış), üstünde gürültü ve kenarları karartan gölge
        private static Color BackgroundPixel(float u, float v)
        {
            const float rows = 9f;
            float row = Mathf.Floor(v * rows);
            float x = u * 6f + (row % 2f) * 0.5f;
            bool joint = Mathf.Repeat(v * rows, 1f) < 0.04f || Mathf.Repeat(x, 1f) < 0.02f;
            float stone = 0.06f + 0.03f * Mathf.PerlinNoise(u * 18f, v * 18f) + 0.015f * Mathf.PerlinNoise(row * 3.1f, Mathf.Floor(x) * 1.7f);
            float vignette = Mathf.Clamp01(1.25f - Vector2.Distance(new Vector2(u, v), new Vector2(0.5f, 0.5f)) * 1.9f);
            float b = (joint ? 0.02f : stone) * vignette;
            return new Color(b, b * 0.97f, b * 1.08f, 1f);
        }

        // Dokuyu ilk seferde (ya da boyutu değişince) üretip PNG olarak kaydeder, sonra sprite olarak yükler
        internal static Sprite EnsureSprite(string path, int size, Func<float, float, Color> pixel)
        {
            if (!File.Exists(path) || AssetDatabase.LoadAssetAtPath<Texture2D>(path).width != size)
            {
                // Boyut değiştiyse eski dosyayı ayarlarıyla birlikte sil: eski sprite kesim alanı (ör. 256×256) kalırsa
                // yeni dokunun yalnızca bir köşesi görünür
                AssetDatabase.DeleteAsset(path);
                SceneBuildContext.EnsureFolder(TextureFolder);
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        texture.SetPixel(x, y, pixel(x / (float)size, y / (float)size));
                    }
                }

                File.WriteAllBytes(path, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(path);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = true;                                   // küçültünce pürüzsüz
                importer.textureCompression = TextureImporterCompression.Uncompressed; // ışıkta bantlaşma olmasın
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
