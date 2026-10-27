using System;
using System.Collections.Generic;

namespace Lumen.Companion.StateMachine
{
    public sealed class CompanionStateMachine
    {
        private readonly Dictionary<CompanionStateId, ICompanionState> states = new Dictionary<CompanionStateId, ICompanionState>();

        public ICompanionState Current { get; private set; }

        public CompanionStateId CurrentId => Current?.Id ?? CompanionStateId.Idle;

        public event Action<CompanionStateId> StateChanged;

        public void Register(ICompanionState state)
        {
            states[state.Id] = state;
        }

        public void ChangeState(CompanionStateId id)
        {
            Current?.Exit();
            Current = states[id];
            Current.Enter();
            StateChanged?.Invoke(id);
        }

        public void Tick(float deltaTime)
        {
            Current?.Tick(deltaTime);
        }
    }
}
