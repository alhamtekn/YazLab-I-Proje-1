using UnityEngine;

namespace Lumen.Companion.StateMachine
{
    /// <summary>Oyuncunun yakınında bekler; oyuncu uzaklaşınca takibe geçer.</summary>
    public sealed class IdleState : CompanionState
    {
        public IdleState(CompanionContext context, CompanionStateMachine machine)
            : base(context, machine)
        {
        }

        public override CompanionStateId Id => CompanionStateId.Idle;

        public override void Enter()
        {
            if (Context.Agent.isOnNavMesh)
            {
                Context.Agent.ResetPath();
            }
        }

        public override void Tick(float deltaTime)
        {
            if (Context.PlanarDistanceTo(Context.Player.position) > Context.Settings.followStartDistance)
            {
                Machine.ChangeState(CompanionStateId.Follow);
            }
        }
    }

    /// <summary>NavMesh üzerinde oyuncuyu takip eder.</summary>
    public sealed class FollowState : NavigatingState
    {
        public FollowState(CompanionContext context, CompanionStateMachine machine)
            : base(context, machine)
        {
        }

        public override CompanionStateId Id => CompanionStateId.Follow;

        protected override Vector3 Destination => Context.Player.position;

        protected override bool HasArrived() =>
            Context.PlanarDistanceTo(Context.Player.position) <= Context.Settings.followStopDistance;

        protected override void OnArrived() => Machine.ChangeState(CompanionStateId.Idle);
    }

    /// <summary>Seçilen hedefin yaklaşma noktasına NavMesh ile ilerler.</summary>
    public sealed class MoveToTargetState : NavigatingState
    {
        public MoveToTargetState(CompanionContext context, CompanionStateMachine machine)
            : base(context, machine)
        {
        }

        public override CompanionStateId Id => CompanionStateId.MoveToTarget;

        protected override Vector3 Destination => Context.CurrentTarget.ApproachPoint;

        protected override bool IsStillValid()
        {
            if (Context.CurrentTarget != null && Context.CurrentTarget.NeedsHelp)
            {
                return true;
            }

            Context.CurrentTarget = null;
            Machine.ChangeState(CompanionStateId.Follow);
            return false;
        }

        protected override bool HasArrived() =>
            Context.PlanarDistanceTo(Context.CurrentTarget.ApproachPoint) <= Context.Settings.arriveDistance;

        protected override void OnArrived() => Machine.ChangeState(CompanionStateId.Interact);
    }

    /// <summary>
    /// Gidiş yönünde tehlike algılandı: durur, yolu güncel tutar (NavMesh oyulmuş tuzakların etrafından
    /// yeni rota bulabilir) ve yön temizlenince ya da süre dolunca önceki duruma döner.
    /// </summary>
    public sealed class AvoidState : CompanionState
    {
        private float elapsed;
        private float repathTimer;

        public AvoidState(CompanionContext context, CompanionStateMachine machine)
            : base(context, machine)
        {
        }

        public override CompanionStateId Id => CompanionStateId.Avoid;

        public override void Enter()
        {
            elapsed = 0f;
            repathTimer = 0f;
            Context.Agent.isStopped = true;
            Context.Agent.velocity = Vector3.zero;
        }

        public override void Tick(float deltaTime)
        {
            if (Context.ResumeState == CompanionStateId.MoveToTarget &&
                (Context.CurrentTarget == null || !Context.CurrentTarget.NeedsHelp))
            {
                Machine.ChangeState(CompanionStateId.Follow);
                return;
            }

            elapsed += deltaTime;
            repathTimer -= deltaTime;
            if (repathTimer <= 0f && Context.AvoidDestination != null)
            {
                Context.Agent.SetDestination(Context.AvoidDestination());
                repathTimer = Context.Settings.repathInterval;
            }

            if (Context.Agent.pathPending)
            {
                return;
            }

            Vector3 heading = Context.Agent.steeringTarget - Context.Self.position;
            float clearDistance = Context.Settings.avoidDistance + Context.Settings.avoidExitMargin;
            bool blocked = Context.Sensor.TryDetectHazard(heading, clearDistance, Context.Settings.avoidAngle, out _);

            if (!blocked || elapsed >= Context.Settings.avoidTimeout)
            {
                Machine.ChangeState(Context.ResumeState);
            }
        }

        public override void Exit()
        {
            Context.Agent.isStopped = false;
        }
    }

    /// <summary>Hedefe ulaşıldı: kısa bir süre sonra hedefi tetikler ve oyuncuya döner.</summary>
    public sealed class InteractState : CompanionState
    {
        private float elapsed;

        public InteractState(CompanionContext context, CompanionStateMachine machine)
            : base(context, machine)
        {
        }

        public override CompanionStateId Id => CompanionStateId.Interact;

        public override void Enter()
        {
            elapsed = 0f;
            Context.Agent.ResetPath();
        }

        public override void Tick(float deltaTime)
        {
            elapsed += deltaTime;
            if (elapsed < Context.Settings.interactDuration)
            {
                return;
            }

            Context.CurrentTarget?.OnCompanionArrived(Context.Self.gameObject);
            Context.CurrentTarget = null;
            Machine.ChangeState(CompanionStateId.Follow);
        }
    }
}
