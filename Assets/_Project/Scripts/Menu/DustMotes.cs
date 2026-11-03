using UnityEngine;

namespace Lumen.Menu
{
    /// <summary>
    /// Menüde yavaşça yükselen ışık zerreleri. Her zerre yukarı süzülür, hafifçe yana salınır ve
    /// ekranın üstünden çıkınca alttan yeniden başlar.
    /// </summary>
    public class DustMotes : MonoBehaviour
    {
        [SerializeField] private RectTransform[] motes;
        [SerializeField] private Vector2 area = new Vector2(1920f, 1080f);
        [SerializeField] private float speed = 22f;

        private float[] seeds;

        public void Configure(RectTransform[] moteRects) => motes = moteRects;

        private void Awake()
        {
            seeds = new float[motes.Length];
            for (int i = 0; i < motes.Length; i++)
            {
                seeds[i] = Random.value * 100f;
                motes[i].anchoredPosition = new Vector2(Random.Range(-area.x, area.x) * 0.5f, Random.Range(-area.y, area.y) * 0.5f);
            }
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            for (int i = 0; i < motes.Length; i++)
            {
                Vector2 p = motes[i].anchoredPosition;
                float sway = Mathf.Sin(Time.unscaledTime * 0.6f + seeds[i]) * 8f;
                p += new Vector2(sway, speed * (0.5f + (seeds[i] % 1f))) * dt;
                if (p.y > area.y * 0.5f)
                {
                    p = new Vector2(Random.Range(-area.x, area.x) * 0.5f, -area.y * 0.5f);
                }

                motes[i].anchoredPosition = p;
            }
        }
    }
}
