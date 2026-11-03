using UnityEngine;

namespace Lumen.Player
{
    /// <summary>
    /// Sağ üstteki mini harita. Bu bileşen yukarıdan bakan (ortografik) kameranın üzerindedir:
    /// - Oyuncunun tepesinde durur ve baktığı yöne göre döner (haritanın üstü hep oyuncunun önü).
    /// - Tavan katmanını çizmez (kamera ayarında); sahne karanlık olduğu için çizerken ortam ışığını geçici açar.
    /// - Oyuncu ortadaki ok, Lumi mavi nokta olarak gösterilir; Lumi harita dışındaysa kenara yapışır.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class MinimapView : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private Transform viewCamera;     // oyuncunun kamerası (yön için)
        [SerializeField] private Transform lumi;
        [SerializeField] private RectTransform map;        // haritanın gösterildiği RawImage
        [SerializeField] private RectTransform lumiMarker;
        [SerializeField] private float height = 30f;
        [SerializeField] private Color mapAmbient = new Color(0.55f, 0.55f, 0.6f);

        private Camera mapCamera;
        private Color savedAmbient;
        private bool savedFog;

        public void Configure(Transform playerTransform, Transform playerCamera, Transform companion, RectTransform mapRect, RectTransform lumiDot)
        {
            player = playerTransform;
            viewCamera = playerCamera;
            lumi = companion;
            map = mapRect;
            lumiMarker = lumiDot;
        }

        private void Awake() => mapCamera = GetComponent<Camera>();

        private void LateUpdate()
        {
            transform.SetPositionAndRotation(player.position + Vector3.up * height, Quaternion.Euler(90f, viewCamera.eulerAngles.y, 0f));

            // Lumi'nin haritadaki yeri: kameranın görüş alanında 0-1 arası konum, dışındaysa kenara sıkıştır
            Vector3 viewport = mapCamera.WorldToViewportPoint(lumi.position);
            Vector2 p = new Vector2(Mathf.Clamp01(viewport.x), Mathf.Clamp01(viewport.y)) - new Vector2(0.5f, 0.5f);
            lumiMarker.anchoredPosition = Vector2.Scale(p, map.rect.size);
        }

        // Yalnızca bu kamera çizerken: ortam ışığı açık, sis kapalı (harita okunur olsun). Ana kamera etkilenmez.
        private void OnPreRender()
        {
            savedAmbient = RenderSettings.ambientLight;
            savedFog = RenderSettings.fog;
            RenderSettings.ambientLight = mapAmbient;
            RenderSettings.fog = false;
        }

        private void OnPostRender()
        {
            RenderSettings.ambientLight = savedAmbient;
            RenderSettings.fog = savedFog;
        }
    }
}
