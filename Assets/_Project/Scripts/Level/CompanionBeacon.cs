using System;
using System.Collections;
using Lumen.Core;
using UnityEngine;

namespace Lumen.Level
{
    /// <summary>
    /// Karanlık odanın işareti: kristalin yanında sönük bir ışık. "Yardım et" denince Lumi buraya
    /// NavMesh ile, yani çukurlara düşmeden giden güvenli yoldan gider; oyuncu onun ışığını izler.
    /// Lumi varınca işaret yanar ve kristali aydınlatır.
    /// </summary>
    public class CompanionBeacon : MonoBehaviour, ICompanionTarget
    {
        [SerializeField] private Transform approachPoint;
        [SerializeField] private Light beaconLight;
        [SerializeField] private float litIntensity = 2f;
        [SerializeField] private float litRange = 6f;
        [SerializeField] private float igniteDuration = 1f;

        public event Action Lit;

        public bool IsLit { get; private set; }

        public Vector3 ApproachPoint => approachPoint.position;

        public bool NeedsHelp => !IsLit;

        public void Configure(Transform approach, Light light)
        {
            approachPoint = approach;
            beaconLight = light;
        }

        public void OnCompanionArrived(GameObject companion)
        {
            if (IsLit)
            {
                return;
            }

            IsLit = true;
            StartCoroutine(Ignite());
            Lit?.Invoke();
        }

        private void OnEnable()
        {
            CompanionTargetRegistry.Register(this);
        }

        private void OnDisable()
        {
            CompanionTargetRegistry.Unregister(this);
        }

        private IEnumerator Ignite()
        {
            if (beaconLight == null)
            {
                yield break;
            }

            float fromIntensity = beaconLight.intensity;
            float fromRange = beaconLight.range;
            for (float t = 0f; t < 1f; t += Time.deltaTime / igniteDuration)
            {
                beaconLight.intensity = Mathf.Lerp(fromIntensity, litIntensity, t);
                beaconLight.range = Mathf.Lerp(fromRange, litRange, t);
                yield return null;
            }

            beaconLight.intensity = litIntensity;
            beaconLight.range = litRange;
        }
    }
}
