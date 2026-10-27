namespace Lumen.Companion.StateMachine
{
    public interface ICompanionState
    {
        CompanionStateId Id { get; }

        void Enter();

        void Tick(float deltaTime);

        void Exit();
    }
}
