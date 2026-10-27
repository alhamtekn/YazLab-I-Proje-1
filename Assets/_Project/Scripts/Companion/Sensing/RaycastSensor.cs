using System.Collections.Generic;
using Lumen.Core;
using UnityEngine;

namespace Lumen.Companion.Sensing
{
    /// <summary>
    /// Bakış yönü etrafında yelpaze biçiminde ışın gönderen sensör.
    /// Yalnızca Hazard katmanındaki nesneleri görür; tuzakların tetik alanları da algılanır.
    /// </summary>
    public class RaycastSensor : MonoBehaviour, ISensor
    {
        [SerializeField] private int rayCount = 9;
        [SerializeField] private float spreadAngle = 90f;
        [SerializeField] private float range = 3.5f;
        [SerializeField] private float eyeHeight = 0.35f;
        [SerializeField] private LayerMask hazardMask;

        private readonly List<SensorReading> readings = new List<SensorReading>();
        private Vector3 lastForward = Vector3.forward;

        public float Range => range;

        public void Configure(LayerMask mask)
        {
            hazardMask = mask;
        }

        public IReadOnlyList<SensorReading> Scan(Vector3 forward)
        {
            readings.Clear();
            forward = Vector3.ProjectOnPlane(forward, Vector3.up);
            if (forward.sqrMagnitude < 0.0001f)
            {
                forward = transform.forward;
            }

            forward.Normalize();
            lastForward = forward;
            Vector3 origin = transform.position + Vector3.up * eyeHeight;

            for (int i = 0; i < rayCount; i++)
            {
                float angle = RayAngle(i);
                Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * forward;

                if (Physics.Raycast(origin, direction, out RaycastHit hit, range, hazardMask, QueryTriggerInteraction.Collide))
                {
                    IHazard hazard = hit.collider.GetComponentInParent<IHazard>();
                    if (hazard != null)
                    {
                        readings.Add(new SensorReading(hit.distance, angle, hit.point, hazard));
                    }
                }
            }

            return readings;
        }

        public bool TryDetectHazard(Vector3 forward, float maxDistance, float maxAngle, out SensorReading closest)
        {
            closest = default;
            bool found = false;

            foreach (SensorReading reading in Scan(forward))
            {
                if (!reading.IsDangerous || reading.Distance > maxDistance || Mathf.Abs(reading.Angle) > maxAngle)
                {
                    continue;
                }

                if (!found || reading.Distance < closest.Distance)
                {
                    closest = reading;
                    found = true;
                }
            }

            return found;
        }

        private float RayAngle(int index)
        {
            if (rayCount <= 1)
            {
                return 0f;
            }

            return -spreadAngle * 0.5f + spreadAngle * index / (rayCount - 1);
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 origin = transform.position + Vector3.up * eyeHeight;
            Gizmos.color = new Color(1f, 1f, 1f, 0.35f);
            for (int i = 0; i < rayCount; i++)
            {
                Gizmos.DrawRay(origin, Quaternion.AngleAxis(RayAngle(i), Vector3.up) * lastForward * range);
            }

            foreach (SensorReading reading in readings)
            {
                Gizmos.color = reading.IsDangerous ? Color.red : Color.yellow;
                Gizmos.DrawSphere(reading.Point, 0.08f);
            }
        }
    }
}
