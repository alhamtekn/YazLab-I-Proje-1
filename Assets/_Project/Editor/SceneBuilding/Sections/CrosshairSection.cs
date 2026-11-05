using Lumen.Player;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Lumen.EditorTools.SceneBuilding.Sections
{
    /// <summary>Geliştirici 2 — HUD'un ortasında nişangah noktası (HUD'dan, Order 30, sonra çalışır).</summary>
    public sealed class CrosshairSection : ISceneSection
    {
        public int Order => 43;

        public void Build(SceneBuildContext ctx)
        {
            var go = new GameObject("Crosshair", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(ctx.Get<Canvas>().transform, false);
            ((RectTransform)go.transform).sizeDelta = new Vector2(8f, 8f); // ekranın tam ortası (varsayılan çapa)

            Image dot = go.GetComponent<Image>();
            dot.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            dot.raycastTarget = false;

            go.AddComponent<CrosshairView>().Configure(dot, ctx.Get<PlayerInteractor>());
        }
    }
}
