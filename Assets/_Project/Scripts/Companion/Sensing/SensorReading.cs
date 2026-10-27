using Lumen.Core;
using UnityEngine;

namespace Lumen.Companion.Sensing
{
    /// <summary>
    /// Tek bir ışının sonucu: tehlikeye olan mesafe, ajanın bakış yönüne göre açı ve tehlikenin durumu.
    /// </summary>
    public readonly struct SensorReading
    {
        public SensorReading(float distance, float angle, Vector3 point, IHazard hazard)
        {
            Distance = distance;
            Angle = angle;
            Point = point;
            Hazard = hazard;
        }

        public float Distance { get; }

        /// <summary>Derece cinsinden, bakış yönüne göre işaretli açı (sol negatif, sağ pozitif).</summary>
        public float Angle { get; }

        public Vector3 Point { get; }

        public IHazard Hazard { get; }

        public bool IsDangerous => Hazard != null && Hazard.IsDangerous;
    }
}
