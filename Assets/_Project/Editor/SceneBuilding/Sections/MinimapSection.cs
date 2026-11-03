using Lumen.Companion;
using Lumen.Core;
using Lumen.Player;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Lumen.EditorTools.SceneBuilding.Sections
{
    /// <summary>
    /// Geliştirici 2 — sağ üstte mini harita: yukarıdan bakan ortografik kamera bir RenderTexture'a çizer,
    /// HUD'daki RawImage onu gösterir. Ortada oyuncu oku, mavi nokta Lumi. Tavan katmanı çizilmez.
    /// HUD'dan (Order 30) sonra çalışır; Lumi durum yazısını haritanın altına kaydırır.
    /// </summary>
    public sealed class MinimapSection : ISceneSection
    {
        private const string TexturePath = "Assets/_Project/Textures/Minimap.renderTexture";
        private const string ArrowPath = "Assets/_Project/Textures/MapArrow.png";
        private const float MapSize = 280f;
        private static readonly Color Gold = new Color(0.86f, 0.74f, 0.48f);

        public int Order => 42;

        public void Build(SceneBuildContext ctx)
        {
            Canvas hud = ctx.Get<Canvas>();
            Transform player = ctx.Get<PlayerController>().transform;

            // Kamera: tavanı, oyuncunun kendi gövdesini ve arayüzü çizmez
            var cameraGo = new GameObject("MinimapCamera");
            Camera cam = cameraGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 15f; // 30 m × 30 m alan
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.cullingMask = ~((1 << GameLayers.CeilingIndex) | (1 << GameLayers.PlayerIndex) | (1 << LayerMask.NameToLayer("UI")));
            cam.depth = -10f;
            cam.targetTexture = EnsureRenderTexture();

            // Harita çerçevesi ve görüntü
            var frame = new GameObject("Minimap", typeof(RectTransform), typeof(Image));
            frame.transform.SetParent(hud.transform, false);
            var frameRect = (RectTransform)frame.transform;
            frameRect.anchorMin = frameRect.anchorMax = frameRect.pivot = new Vector2(1f, 1f);
            frameRect.anchoredPosition = new Vector2(-24f, -24f);
            frameRect.sizeDelta = new Vector2(MapSize + 6f, MapSize + 6f);
            frame.GetComponent<Image>().color = new Color(Gold.r, Gold.g, Gold.b, 0.8f);

            var mapGo = new GameObject("Map", typeof(RectTransform), typeof(RawImage));
            mapGo.transform.SetParent(frame.transform, false);
            var mapRect = (RectTransform)mapGo.transform;
            mapRect.sizeDelta = new Vector2(MapSize, MapSize);
            RawImage map = mapGo.GetComponent<RawImage>();
            map.texture = cam.targetTexture;
            map.raycastTarget = false;

            Sprite arrow = MainMenuBuilder.EnsureSprite(ArrowPath, 64, ArrowPixel);
            Marker("Player", mapRect, arrow, 22f, Color.white);
            RectTransform lumiDot = Marker("Lumi", mapRect, AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"), 14f, new Color(0.5f, 0.85f, 1f));

            cameraGo.AddComponent<MinimapView>().Configure(player, ctx.Get<Camera>().transform, ctx.Get<CompanionController>().transform, mapRect, lumiDot);

            // Sağ üstteki Lumi durum yazısı haritanın altına iner
            var state = (RectTransform)hud.transform.Find("CompanionState");
            if (state != null)
            {
                state.anchoredPosition = new Vector2(-24f, -MapSize - 44f);
            }
        }

        private static RectTransform Marker(string name, Transform parent, Sprite sprite, float size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            ((RectTransform)go.transform).sizeDelta = new Vector2(size, size);
            Image image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            return (RectTransform)go.transform;
        }

        private static RenderTexture EnsureRenderTexture()
        {
            var texture = AssetDatabase.LoadAssetAtPath<RenderTexture>(TexturePath);
            if (texture == null)
            {
                texture = new RenderTexture(512, 512, 16);
                AssetDatabase.CreateAsset(texture, TexturePath);
            }

            return texture;
        }

        // Yukarıyı gösteren üçgen ok (oyuncunun baktığı yön haritanın üstü)
        private static Color ArrowPixel(float u, float v)
        {
            float halfWidth = (1f - v) * 0.4f;               // tepede sivri, tabanda geniş
            bool inside = v > 0.1f && v < 0.95f && Mathf.Abs(u - 0.5f) < halfWidth;
            return new Color(1f, 1f, 1f, inside ? 1f : 0f);
        }
    }
}
