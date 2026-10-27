using System;
using Lumen.Companion.Sensing;
using Lumen.Core;
using UnityEngine;
using UnityEngine.AI;

namespace Lumen.Companion.StateMachine
{
    /// <summary>
    /// Durumların paylaştığı veriler. Durumlar MonoBehaviour'a değil bu bağlama bağımlıdır.
    /// </summary>
    public sealed class CompanionContext
    {
        public CompanionContext(NavMeshAgent agent, Transform self, Transform player, ISensor sensor, ITargetSelector selector, CompanionSettings settings)
        {
            Agent = agent;
            Self = self;
            Player = player;
            Sensor = sensor;
            Selector = selector;
            Settings = settings;
        }

        public NavMeshAgent Agent { get; }

        public Transform Self { get; }

        public Transform Player { get; }

        public ISensor Sensor { get; }

        public ITargetSelector Selector { get; }

        public CompanionSettings Settings { get; }

        public ICompanionTarget CurrentTarget { get; set; }

        /// <summary>Avoid durumundan sonra dönülecek durum.</summary>
        public CompanionStateId ResumeState { get; set; } = CompanionStateId.Follow;

        /// <summary>Avoid sırasında yolu güncel tutmak için kullanılan hedef konum.</summary>
        public Func<Vector3> AvoidDestination { get; set; }

        public float PlanarDistanceTo(Vector3 point)
        {
            Vector3 delta = point - Self.position;
            delta.y = 0f;
            return delta.magnitude;
        }
    }
}
