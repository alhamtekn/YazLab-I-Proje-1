using UnityEngine;
using UnityEngine.EventSystems;

namespace Lumen.Menu
{
    /// <summary>
    /// Ana menü düğmesi üzerine gelince: yazı biraz sağa kayar ve solunda ince bir ışık çizgisi belirir.
    /// </summary>
    public class MenuButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private RectTransform label;
        [SerializeField] private CanvasGroup marker;
        [SerializeField] private float shift = 24f;

        private Vector2 restPosition;
        private bool hovered;

        public void Configure(RectTransform labelRect, CanvasGroup markerGroup)
        {
            label = labelRect;
            marker = markerGroup;
        }

        private void Awake()
        {
            restPosition = label.anchoredPosition;
            marker.alpha = 0f;
        }

        public void OnPointerEnter(PointerEventData eventData) => hovered = true;

        public void OnPointerExit(PointerEventData eventData) => hovered = false;

        private void Update()
        {
            // Hedefe yumuşakça yaklaş (menü duraklatılmış zamanla da çalışsın diye unscaled)
            float k = 1f - Mathf.Exp(-12f * Time.unscaledDeltaTime);
            Vector2 target = restPosition + (hovered ? Vector2.right * shift : Vector2.zero);
            label.anchoredPosition = Vector2.Lerp(label.anchoredPosition, target, k);
            marker.alpha = Mathf.Lerp(marker.alpha, hovered ? 1f : 0f, k);
        }
    }
}
