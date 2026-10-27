using System;
using System.Collections;
using Lumen.Core;
using UnityEngine;

namespace Lumen.Level
{
    /// <summary>
    /// Uçurumun karşı kenarında dik duran köprü. NavMesh sahnede inik hâliyle pişirilir;
    /// oyun başında kaldırılır ve inene kadar engelleyiciler (görünmez duvar + NavMeshObstacle) açık kalır.
    /// </summary>
    public class Drawbridge : MonoBehaviour, IActivatable
    {
        [SerializeField] private Transform pivot;
        [SerializeField] private GameObject[] blockersWhileRaised;
        [SerializeField] private float raisedAngle = 80f;
        [SerializeField] private float lowerDuration = 2f;

        public event Action Activated;

        public bool IsActivated { get; private set; }

        public void Configure(Transform bridgePivot, GameObject[] blockers)
        {
            pivot = bridgePivot;
            blockersWhileRaised = blockers;
        }

        public void Activate()
        {
            if (IsActivated)
            {
                return;
            }

            IsActivated = true;
            StartCoroutine(Lower());
        }

        private void Awake()
        {
            pivot.localRotation = Quaternion.Euler(raisedAngle, 0f, 0f);
            SetBlockers(true);
        }

        private IEnumerator Lower()
        {
            Quaternion from = pivot.localRotation;
            for (float t = 0f; t < 1f; t += Time.deltaTime / lowerDuration)
            {
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                pivot.localRotation = Quaternion.Slerp(from, Quaternion.identity, eased);
                yield return null;
            }

            pivot.localRotation = Quaternion.identity;
            SetBlockers(false);
            Activated?.Invoke();
        }

        private void SetBlockers(bool active)
        {
            foreach (GameObject blocker in blockersWhileRaised)
            {
                blocker.SetActive(active);
            }
        }
    }
}
