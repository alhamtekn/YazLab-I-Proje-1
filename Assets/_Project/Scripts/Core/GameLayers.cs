using UnityEngine;

namespace Lumen.Core
{
    /// <summary>
    /// ProjectSettings/TagManager.asset içindeki katman adlarının tek kaynağı.
    /// </summary>
    public static class GameLayers
    {
        public const string Player = "Player";
        public const string Companion = "Companion";
        public const string Hazard = "Hazard";
        public const string Interactable = "Interactable";

        public static int PlayerIndex => LayerMask.NameToLayer(Player);
        public static int CompanionIndex => LayerMask.NameToLayer(Companion);
        public static int HazardIndex => LayerMask.NameToLayer(Hazard);
        public static int InteractableIndex => LayerMask.NameToLayer(Interactable);

        public static LayerMask HazardMask => LayerMask.GetMask(Hazard);
        public static LayerMask InteractableMask => LayerMask.GetMask(Interactable);
    }
}
