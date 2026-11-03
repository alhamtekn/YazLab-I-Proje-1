using System;
using Lumen.Core;
using UnityEngine;

namespace Lumen.Sanctum
{
    /// <summary>
    /// 2. sınav "Işığın Yolu": uçurumun üstünde görünmez, dolambaçlı bir yol. Taşlar yalnızca Lumi'nin ışığı
    /// yakınlarına düşünce görünür ve bir süre sonra yeniden kaybolur. Oyuncu yolu göremez; Lumi'yi karşıdaki
    /// fenere gönderir (F) ve onun aydınlattığı taşlardan geçer. Düşen oyuncu kontrol noktasına döner.
    /// Taşların çarpıştırıcısı hep açıktır; Lumi yolu NavMesh ile kendisi bulur.
    /// </summary>
    public class LightPath : MonoBehaviour, ICompanionTarget
    {
        [SerializeField] private Transform companion;
        [SerializeField] private TwinSeal previousTrial;     // önce ikiz mühür açılmalı
        [SerializeField] private Renderer[] tiles = Array.Empty<Renderer>();
        [SerializeField] private Transform beacon;
        [SerializeField] private Light beaconLight;
        [SerializeField] private float revealRadius = 3.2f;
        [SerializeField] private float lingerSeconds = 6f;    // Lumi uzaklaştıktan sonra taşın görünür kalma süresi
        [SerializeField] private Color glow = new Color(0.6f, 0.85f, 1.6f);

        private float[] visibleFor;
        private MaterialPropertyBlock block;

        public bool BeaconLit { get; private set; }

        public Vector3 ApproachPoint => beacon.position;

        // Sınavlar sırayla: bariyer açılmadan Lumi fenere yönelmez
        public bool NeedsHelp => !BeaconLit && previousTrial.IsOpen;

        /// <summary>Şu an görünen taş sayısı (test için).</summary>
        public int VisibleTiles { get; private set; }

        public void Configure(Transform companionTransform, TwinSeal seal, Renderer[] pathTiles, Transform beaconPoint, Light light)
        {
            previousTrial = seal;
            companion = companionTransform;
            tiles = pathTiles;
            beacon = beaconPoint;
            beaconLight = light;
        }

        public void OnCompanionArrived(GameObject companionObject)
        {
            BeaconLit = true;
            beaconLight.intensity = 3f;
        }

        private void OnEnable() => CompanionTargetRegistry.Register(this);

        private void OnDisable() => CompanionTargetRegistry.Unregister(this);

        private void Awake()
        {
            visibleFor = new float[tiles.Length];
            block = new MaterialPropertyBlock();
            beaconLight.intensity = 0.4f;
        }

        private void Update()
        {
            VisibleTiles = 0;
            for (int i = 0; i < tiles.Length; i++)
            {
                Vector3 d = tiles[i].transform.position - companion.position;
                if (new Vector2(d.x, d.z).magnitude < revealRadius)
                {
                    visibleFor[i] = lingerSeconds;
                }

                visibleFor[i] = Mathf.Max(0f, visibleFor[i] - Time.deltaTime);
                bool visible = visibleFor[i] > 0f;
                tiles[i].enabled = visible;
                if (visible)
                {
                    VisibleTiles++;
                    // Son saniyelerde parlaklık azalarak söner
                    block.SetColor("_EmissionColor", glow * Mathf.Clamp01(visibleFor[i] / 2f));
                    tiles[i].SetPropertyBlock(block);
                }
            }
        }
    }
}
