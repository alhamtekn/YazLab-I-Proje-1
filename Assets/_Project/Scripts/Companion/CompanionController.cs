using System;
using Lumen.Companion.Sensing;
using Lumen.Companion.StateMachine;
using Lumen.Core;
using UnityEngine;
using UnityEngine.AI;

namespace Lumen.Companion
{
    /// <summary>
    /// Lumi: NavMeshAgent ile hareket eden, RaycastSensor ile tehlike algılayan ve kararlarını
    /// durum makinesiyle veren eşlikçi. Bu sınıf yalnızca bileşenleri birbirine bağlar;
    /// karar mantığı durum sınıflarındadır.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class CompanionController : MonoBehaviour, ICompanionCommandReceiver, IHazardVictim
    {
        [SerializeField] private Transform player;
        [SerializeField] private RaycastSensor sensor;
        [SerializeField] private CompanionSettings settings = new CompanionSettings();

        private NavMeshAgent agent;
        private CompanionStateMachine machine;
        private CompanionContext context;

        public event Action<CompanionStateId> StateChanged;

        public CompanionStateId CurrentState => machine?.CurrentId ?? CompanionStateId.Idle;

        public ICompanionTarget CurrentTarget => context?.CurrentTarget;

        /// <summary>Tuzağa yakalanma sayısı (testlerde raporlanır).</summary>
        public int HazardHits { get; private set; }

        public void Configure(Transform playerTransform, RaycastSensor raycastSensor)
        {
            player = playerTransform;
            sensor = raycastSensor;
        }

        public bool CommandHelp()
        {
            ICompanionTarget target = context.Selector.SelectBest(transform.position);
            if (target == null)
            {
                return false;
            }

            context.CurrentTarget = target;
            machine.ChangeState(CompanionStateId.MoveToTarget);
            return true;
        }

        public void CommandFollow()
        {
            context.CurrentTarget = null;
            machine.ChangeState(CompanionStateId.Follow);
        }

        public void OnHazardHit(Vector3 resetPosition)
        {
            HazardHits++;
            agent.Warp(resetPosition);
            machine.ChangeState(context.CurrentTarget != null ? CompanionStateId.MoveToTarget : CompanionStateId.Follow);
        }

        /// <summary>Testler için: eşlikçiyi verilen noktaya yerleştirir ve sayaçları sıfırlar.</summary>
        public void PlaceAt(Vector3 position)
        {
            agent.Warp(position);
            HazardHits = 0;
            context.CurrentTarget = null;
            machine.ChangeState(CompanionStateId.Idle);
        }

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            context = new CompanionContext(agent, transform, player, sensor, new NavMeshTargetSelector(), settings);

            machine = new CompanionStateMachine();
            machine.Register(new IdleState(context, machine));
            machine.Register(new FollowState(context, machine));
            machine.Register(new MoveToTargetState(context, machine));
            machine.Register(new AvoidState(context, machine));
            machine.Register(new InteractState(context, machine));
            machine.StateChanged += id => StateChanged?.Invoke(id);
        }

        private void Start()
        {
            machine.ChangeState(CompanionStateId.Idle);
        }

        private void Update()
        {
            if (agent.isOnNavMesh)
            {
                machine.Tick(Time.deltaTime);
            }
        }
    }
}
