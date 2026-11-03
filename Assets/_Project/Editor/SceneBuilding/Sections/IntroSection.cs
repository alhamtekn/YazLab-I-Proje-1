using UnityEngine;

namespace Lumen.EditorTools.SceneBuilding.Sections
{
    /// <summary>
    /// Geliştirici 1 — giriş koridoru: oyuncu karanlık, dar bir koridorun başında uyanır ve salona doğru yürür.
    /// Koridorun sonunda (salon kapısının önünde) Lumi bekler.
    ///
    /// Yerleşim (X sağa, Z ileri, birim metre):
    ///   Koridor  x[-1.5, 1.5]  z[-20, 0]   (salonun arka duvarındaki kapıdan girilir)
    ///   Oyuncu başlangıcı (0, -18), Lumi (0, -2)
    /// Hikâye metni tetikleri z ≈ -15, -10, -5'te (Geliştirici 2, StorySection).
    /// </summary>
    public sealed class IntroSection : ISceneSection
    {
        public const float HalfWidth = 1.5f;
        public const float Length = 20f;

        public int Order => 32;

        public void Build(SceneBuildContext ctx)
        {
            Transform corridor = new GameObject("IntroCorridor").transform;
            corridor.SetParent(ctx.Root("Level"), false);
            BuildCorridor(ctx, corridor);
        }

        private static void BuildCorridor(SceneBuildContext ctx, Transform corridor)
        {
            Material floor = ctx.Mat("Floor", new Color(0.22f, 0.2f, 0.19f));
            Material stone = ctx.Mat("Stone", new Color(0.27f, 0.26f, 0.29f));
            float h = EnvironmentSection.WallHeight;
            float zCenter = -Length * 0.5f;

            ctx.Box("Floor", corridor, new Vector3(0f, -0.25f, zCenter - 0.25f), new Vector3(HalfWidth * 2f, 0.5f, Length + 0.5f), floor);
            ctx.Box("Wall_L", corridor, new Vector3(-HalfWidth - 0.25f, h * 0.5f, zCenter - 0.25f), new Vector3(0.5f, h, Length + 0.5f), stone);
            ctx.Box("Wall_R", corridor, new Vector3(HalfWidth + 0.25f, h * 0.5f, zCenter - 0.25f), new Vector3(0.5f, h, Length + 0.5f), stone);
            ctx.Box("Wall_End", corridor, new Vector3(0f, h * 0.5f, -Length - 0.25f), new Vector3(HalfWidth * 2f + 1f, h, 0.5f), stone);

            // Koridor bilerek loş: başta tek meşale, sonra uzun bir karanlık; salonun ışığı ve Lumi ileride.
            ctx.Torch("Torch_Intro_Start", corridor, new Vector3(-HalfWidth + 0.2f, 2.3f, -17f));
            ctx.Torch("Torch_Intro_Mid", corridor, new Vector3(HalfWidth - 0.2f, 2.3f, -9f), false);
        }
    }
}
