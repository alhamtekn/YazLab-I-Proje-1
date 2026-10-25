using System;

namespace Lumen.Core
{
    /// <summary>
    /// Bir kez tetiklenip durumu değişen dünya nesneleri (şalter, köprü, kapı).
    /// </summary>
    public interface IActivatable
    {
        bool IsActivated { get; }

        event Action Activated;

        void Activate();
    }
}
