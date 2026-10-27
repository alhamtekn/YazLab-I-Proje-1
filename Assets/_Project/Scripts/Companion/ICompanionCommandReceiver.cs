using System;

namespace Lumen.Companion
{
    public enum CompanionStateId
    {
        Idle,
        Follow,
        MoveToTarget,
        Avoid,
        Interact
    }

    /// <summary>
    /// Oyuncunun eşlikçiye verebildiği emirler. Oyuncu tarafı yalnızca bu arayüzü bilir.
    /// </summary>
    public interface ICompanionCommandReceiver
    {
        CompanionStateId CurrentState { get; }

        event Action<CompanionStateId> StateChanged;

        /// <summary>Yardım bekleyen bir hedef seçip ona gitmeyi dener. Hedef yoksa false döner.</summary>
        bool CommandHelp();

        void CommandFollow();
    }
}
