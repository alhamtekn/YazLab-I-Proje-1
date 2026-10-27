namespace Lumen.Companion.StateMachine
{
    public abstract class CompanionState : ICompanionState
    {
        protected CompanionState(CompanionContext context, CompanionStateMachine machine)
        {
            Context = context;
            Machine = machine;
        }

        public abstract CompanionStateId Id { get; }

        protected CompanionContext Context { get; }

        protected CompanionStateMachine Machine { get; }

        public virtual void Enter()
        {
        }

        public virtual void Tick(float deltaTime)
        {
        }

        public virtual void Exit()
        {
        }
    }
}
