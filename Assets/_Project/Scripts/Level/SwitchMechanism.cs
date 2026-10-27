using System;
using System.Collections;
using Lumen.Core;
using UnityEngine;

namespace Lumen.Level
{
    /// <summary>
    /// Yalnızca eşlikçinin ulaşabildiği şalter. Eşlikçi yaklaşma noktasına varınca indirilir
    /// ve bağlı IActivatable nesneleri (örn. köprü) tetikler.
    /// </summary>
    public class SwitchMechanism : MonoBehaviour, ICompanionTarget, IActivatable
    {
        [SerializeField] private Transform approachPoint;
        [SerializeField] private Transform lever;
        [SerializeField] private Light indicator;
        [SerializeField] private MonoBehaviour[] linkedActivatables;
        [SerializeField] private float leverAngle = 70f;

        public event Action Activated;

        public bool IsActivated { get; private set; }

        public Vector3 ApproachPoint => approachPoint.position;

        public bool NeedsHelp => !IsActivated;

        public void Configure(Transform approach, Transform leverHandle, Light statusLight, MonoBehaviour[] linked)
        {
            approachPoint = approach;
            lever = leverHandle;
            indicator = statusLight;
            linkedActivatables = linked;
        }

        public void OnCompanionArrived(GameObject companion)
        {
            Activate();
        }

        public void Activate()
        {
            if (IsActivated)
            {
                return;
            }

            IsActivated = true;
            StartCoroutine(PullLever());

            if (indicator != null)
            {
                indicator.color = new Color(0.3f, 1f, 0.4f);
            }

            foreach (MonoBehaviour behaviour in linkedActivatables)
            {
                (behaviour as IActivatable)?.Activate();
            }

            Activated?.Invoke();
        }

        private void OnEnable()
        {
            CompanionTargetRegistry.Register(this);
        }

        private void OnDisable()
        {
            CompanionTargetRegistry.Unregister(this);
        }

        private IEnumerator PullLever()
        {
            if (lever == null)
            {
                yield break;
            }

            Quaternion from = lever.localRotation;
            Quaternion to = Quaternion.Euler(leverAngle, 0f, 0f) * from;
            for (float t = 0f; t < 1f; t += Time.deltaTime * 3f)
            {
                lever.localRotation = Quaternion.Slerp(from, to, t);
                yield return null;
            }

            lever.localRotation = to;
        }
    }
}
