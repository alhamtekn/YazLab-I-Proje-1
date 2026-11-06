using System;

namespace Lumen.Core
{
    /// <summary>Kaybetme durumu: lav, Lumi'nin parkuru ya da Arven tetikler; ekran yalnızca olayı dinler.</summary>
    public interface IGameOver
    {
        bool IsLost { get; }
        event Action<string> GameLost;
        void Lose(string reason);
    }
}
