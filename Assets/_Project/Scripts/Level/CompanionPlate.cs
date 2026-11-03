using System;
using Lumen.Core;
using UnityEngine;

namespace Lumen.Level
{
    /// <summary>
    /// Diken odasının Lumi plakası: tuzakların karşı tarafında. "Yardım et" denince Lumi RaycastSensor ile
    /// açık dikenleri görüp etrafından dolaşarak buraya gider; varınca bağlı tuzaklar kalıcı olarak durur.
    /// </summary>
    public class CompanionPlate : MonoBehaviour, ICompanionTarget
    {
        [SerializeField] private Transform approachPoint;
        [SerializeField] private SpikeTrap[] traps = Array.Empty<SpikeTrap>();
        [SerializeField] private Light indicator;

        public bool IsPressed { get; private set; }

        public Vector3 ApproachPoint => approachPoint.position;

        public bool NeedsHelp => !IsPressed;

        public void Configure(Transform approach, SpikeTrap[] linkedTraps, Light statusLight)
        {
            approachPoint = approach;
            traps = linkedTraps;
            indicator = statusLight;
        }

        public void OnCompanionArrived(GameObject companion)
        {
            IsPressed = true;
            foreach (SpikeTrap trap in traps)
            {
                trap.SetSuppressed(true);
            }

            if (indicator != null)
            {
                indicator.color = new Color(0.3f, 1f, 0.4f);
                indicator.intensity = 1.8f;
            }
        }

        private void OnEnable()
        {
            CompanionTargetRegistry.Register(this);
        }

        private void OnDisable()
        {
            CompanionTargetRegistry.Unregister(this);
        }
    }
}
