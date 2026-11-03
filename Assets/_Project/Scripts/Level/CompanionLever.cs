using Lumen.Core;
using UnityEngine;

namespace Lumen.Level
{
    /// <summary>
    /// Yalnızca Lumi'nin sığdığı alçak bir geçidin ardındaki şalter. Bağlı kapı kapalıyken yardım bekler;
    /// Lumi varınca kapıyı açar. Süreli kapı yeniden kapanınca şalter de yeniden yardım bekler.
    /// </summary>
    public class CompanionLever : MonoBehaviour, ICompanionTarget
    {
        [SerializeField] private Transform approachPoint;
        [SerializeField] private MazeGate gate;
        [SerializeField] private Light indicator;

        private static readonly Color Waiting = new Color(1f, 0.2f, 0.15f);
        private static readonly Color Open = new Color(0.3f, 1f, 0.4f);

        public Vector3 ApproachPoint => approachPoint.position;

        public bool NeedsHelp => !gate.IsActivated;

        public void Configure(Transform approach, MazeGate linkedGate, Light statusLight)
        {
            approachPoint = approach;
            gate = linkedGate;
            indicator = statusLight;
        }

        public void OnCompanionArrived(GameObject companion)
        {
            gate.Activate();
        }

        private void OnEnable()
        {
            CompanionTargetRegistry.Register(this);
        }

        private void OnDisable()
        {
            CompanionTargetRegistry.Unregister(this);
        }

        private void Update()
        {
            if (indicator != null)
            {
                indicator.color = gate.IsActivated ? Open : Waiting;
            }
        }
    }
}
