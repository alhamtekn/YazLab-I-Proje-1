using System.Collections.Generic;
using UnityEngine;

namespace Lumen.Companion.Sensing
{
    /// <summary>
    /// Ajanın çevre gözlemi. Her okuma bir tehlikeye olan mesafe ve açıyı içerir.
    /// </summary>
    public interface ISensor
    {
        IReadOnlyList<SensorReading> Scan(Vector3 forward);

        /// <summary>
        /// Verilen yön etrafındaki koni içinde, maxDistance'tan yakın tehlikeli bir okuma varsa en yakınını döner.
        /// </summary>
        bool TryDetectHazard(Vector3 forward, float maxDistance, float maxAngle, out SensorReading closest);
    }
}
