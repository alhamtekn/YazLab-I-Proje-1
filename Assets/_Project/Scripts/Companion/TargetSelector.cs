using Lumen.Core;
using UnityEngine;
using UnityEngine.AI;

namespace Lumen.Companion
{
    public interface ITargetSelector
    {
        /// <summary>Yardım bekleyen hedefler arasından en kısa NavMesh yoluna sahip olanı seçer.</summary>
        ICompanionTarget SelectBest(Vector3 from);
    }

    /// <summary>
    /// Her aday için NavMesh.CalculatePath ile gerçek yol uzunluğunu hesaplar.
    /// Tam yol bulunan hedefler kısmi yollara tercih edilir.
    /// </summary>
    public sealed class NavMeshTargetSelector : ITargetSelector
    {
        private readonly NavMeshPath path = new NavMeshPath();

        public ICompanionTarget SelectBest(Vector3 from)
        {
            ICompanionTarget best = null;
            float bestScore = float.MaxValue;

            foreach (ICompanionTarget target in CompanionTargetRegistry.All)
            {
                if (!target.NeedsHelp || !NavMesh.CalculatePath(from, target.ApproachPoint, NavMesh.AllAreas, path))
                {
                    continue;
                }

                if (path.status == NavMeshPathStatus.PathInvalid)
                {
                    continue;
                }

                float score = PathLength(path);
                if (path.status == NavMeshPathStatus.PathPartial)
                {
                    score += 1000f;
                }

                if (score < bestScore)
                {
                    bestScore = score;
                    best = target;
                }
            }

            return best;
        }

        private static float PathLength(NavMeshPath navPath)
        {
            Vector3[] corners = navPath.corners;
            float length = 0f;
            for (int i = 1; i < corners.Length; i++)
            {
                length += Vector3.Distance(corners[i - 1], corners[i]);
            }

            return length;
        }
    }
}
