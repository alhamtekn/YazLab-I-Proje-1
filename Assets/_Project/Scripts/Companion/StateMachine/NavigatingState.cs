using UnityEngine;

namespace Lumen.Companion.StateMachine
{
    /// <summary>
    /// NavMesh üzerinde bir hedefe ilerleyen durumların ortak davranışı:
    /// yolu periyodik olarak yeniler ve gidiş yönünde tehlike algılarsa Avoid durumuna devreder.
    /// </summary>
    public abstract class NavigatingState : CompanionState
    {
        private float repathTimer;

        protected NavigatingState(CompanionContext context, CompanionStateMachine machine)
            : base(context, machine)
        {
        }

        protected abstract Vector3 Destination { get; }

        public override void Enter()
        {
            Context.Agent.isStopped = false;
            repathTimer = 0f;
        }

        public override void Tick(float deltaTime)
        {
            if (!IsStillValid())
            {
                return;
            }

            repathTimer -= deltaTime;
            if (repathTimer <= 0f)
            {
                Context.Agent.SetDestination(Destination);
                repathTimer = Context.Settings.repathInterval;
            }

            if (Context.Agent.pathPending)
            {
                return;
            }

            if (HasArrived())
            {
                OnArrived();
                return;
            }

            Vector3 heading = Context.Agent.steeringTarget - Context.Self.position;
            if (Context.Sensor.TryDetectHazard(heading, Context.Settings.avoidDistance, Context.Settings.avoidAngle, out _))
            {
                Context.ResumeState = Id;
                Context.AvoidDestination = () => Destination;
                Machine.ChangeState(CompanionStateId.Avoid);
            }
        }

        /// <summary>Durum geçerliliğini yitirdiyse başka duruma geçip false döner.</summary>
        protected virtual bool IsStillValid() => true;

        protected abstract bool HasArrived();

        protected abstract void OnArrived();
    }
}
