using System;
using System.Collections;
using Lumen.Core;
using UnityEngine;
using UnityEngine.AI;

namespace Lumen.Sanctum
{
    /// <summary>
    /// 1. sınav "İkiz Mühür": ışık bariyerini açmak için iki mühür aynı anda yanmalı.
    /// - Oyuncu mührü: oyuncu üstünde durduğu sürece yanar.
    /// - Lumi mührü: yalnızca Lumi'nin sığdığı alçak bir girintide. Lumi'ye "yardım et" (F) denince oraya gider,
    ///   mührü chargeSeconds boyunca şarj eder.
    /// İkisi aynı anda yanınca bariyer söner ve bir daha kapanmaz. Tek başına kimse açamaz: oyuncu girintiye sığmaz,
    /// Lumi de oyuncu mührüne kendiliğinden basmaz.
    /// </summary>
    public class TwinSeal : MonoBehaviour, ICompanionTarget
    {
        [SerializeField] private Transform player;
        [SerializeField] private Transform playerPlate;
        [SerializeField] private Transform lumiPlate;
        [SerializeField] private Light playerPlateLight;
        [SerializeField] private Light lumiPlateLight;
        [SerializeField] private GameObject barrier;          // collider + NavMeshObstacle taşıyan enerji duvarı
        [SerializeField] private float activeFromZ;           // oyuncu bu z'yi geçmeden Lumi buraya çağrılmaz
        [SerializeField] private float plateRadius = 1.2f;
        [SerializeField] private float chargeSeconds = 8f;
        [SerializeField] private float plateIntensity = 2.5f;

        private float charge; // Lumi mührünün kalan şarj süresi (sn)

        public event Action Opened;

        public bool IsOpen { get; private set; }

        public bool PlayerOnPlate { get; private set; }

        public bool LumiSealCharged => charge > 0f;

        public Vector3 ApproachPoint => lumiPlate.position;

        // Oyuncu bölgeye girmeden Lumi önceki bölümlerde buraya yönelmesin; şarjlıyken ve açıldıktan sonra gerek yok
        public bool NeedsHelp => !IsOpen && !LumiSealCharged && player.position.z > activeFromZ;

        public void Configure(Transform playerTransform, Transform playerSeal, Transform lumiSeal, Light playerLight, Light lumiLight, GameObject energyBarrier, float zoneStartZ)
        {
            activeFromZ = zoneStartZ;
            player = playerTransform;
            playerPlate = playerSeal;
            lumiPlate = lumiSeal;
            playerPlateLight = playerLight;
            lumiPlateLight = lumiLight;
            barrier = energyBarrier;
        }

        public void OnCompanionArrived(GameObject companion)
        {
            if (!IsOpen)
            {
                charge = chargeSeconds;
            }
        }

        private void OnEnable() => CompanionTargetRegistry.Register(this);

        private void OnDisable() => CompanionTargetRegistry.Unregister(this);

        private void Update()
        {
            if (IsOpen)
            {
                return;
            }

            charge = Mathf.Max(0f, charge - Time.deltaTime);
            Vector3 offset = player.position - playerPlate.position;
            PlayerOnPlate = new Vector2(offset.x, offset.z).magnitude < plateRadius && Mathf.Abs(offset.y) < 1.5f;

            // Işıklar: oyuncu mührü basılıyken yanar, Lumi mührü şarjı azaldıkça söner
            playerPlateLight.intensity = PlayerOnPlate ? plateIntensity : 0.3f;
            lumiPlateLight.intensity = 0.3f + plateIntensity * (charge / chargeSeconds);

            if (PlayerOnPlate && LumiSealCharged)
            {
                StartCoroutine(Open());
            }
        }

        private IEnumerator Open()
        {
            IsOpen = true;
            barrier.GetComponent<Collider>().enabled = false;          // oyuncu geçebilsin
            barrier.GetComponent<NavMeshObstacle>().enabled = false;   // Lumi geçebilsin
            playerPlateLight.intensity = lumiPlateLight.intensity = plateIntensity;
            Opened?.Invoke();

            // Bariyer incelip yok olur
            Vector3 from = barrier.transform.localScale;
            for (float t = 0f; t < 1f; t += Time.deltaTime / 1.5f)
            {
                barrier.transform.localScale = new Vector3(from.x, from.y * (1f - t), from.z);
                yield return null;
            }

            barrier.SetActive(false);
        }
    }
}
