using System.Linq;
using Lumen.EditorTools.SceneBuilding;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Lumen.EditorTools
{
    /// <summary>
    /// Geliştirici 2 — Windows build'i (.exe). Oyun adı, simge ve tam ekran açılışı her build'de buradan ayarlanır,
    /// böylece ayarlar elle değiştirilip unutulmaz. Çıktı: Builds/Lumen/Lumen.exe (git'e girmez).
    /// Menü: Lumen → Windows Build Al. Komut satırı: -executeMethod Lumen.EditorTools.BuildScript.BuildFromCommandLine
    /// </summary>
    public static class BuildScript
    {
        public const string OutputPath = "Builds/Lumen/Lumen.exe";
        private const string IconPath = "Assets/_Project/Textures/Icon.png";

        [MenuItem("Lumen/Windows Build Al")]
        public static void BuildFromMenu()
        {
            BuildReport report = Build();
            if (report.summary.result == BuildResult.Succeeded)
            {
                EditorUtility.RevealInFinder(OutputPath);
            }
        }

        public static void BuildFromCommandLine()
        {
            BuildReport report = Build();
            EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
        }

        public static BuildReport Build()
        {
            ApplyPlayerSettings();
            var options = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(), // MainMenu ilk sahne
                locationPathName = OutputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            Debug.Log($"[BuildScript] {report.summary.result}: {OutputPath} ({report.summary.totalSize / (1024 * 1024)} MB)");
            return report;
        }

        private static void ApplyPlayerSettings()
        {
            PlayerSettings.productName = "Lumen";      // pencere başlığı ve .exe adı
            PlayerSettings.companyName = "YazLab";
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow; // tam ekran açılır; menüden pencereye alınabilir
            PlayerSettings.defaultIsNativeResolution = true;                 // ekranın kendi çözünürlüğü
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = true; // Ollama cevabı beklenirken pencere arkada kalsa da oyun akar

            // Simge: menüdeki ışık küresiyle aynı (kodla üretilir)
            MainMenuBuilder.EnsureSprite(IconPath, 256, IconPixel);
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { icon }, IconKind.Any); // varsayılan simge
        }

        // Siyah zemin üstünde parlak beyaz küre ve mavimsi hale
        private static Color IconPixel(float u, float v)
        {
            float d = Vector2.Distance(new Vector2(u, v), new Vector2(0.5f, 0.5f)) * 2f;
            float core = Mathf.Clamp01((0.38f - d) * 60f);
            float halo = Mathf.Pow(Mathf.Clamp01(1f - d), 2.2f) * 0.8f;
            Color c = Color.Lerp(new Color(0.55f, 0.7f, 1f) * halo, Color.white, core);
            return new Color(c.r, c.g, c.b, 1f);
        }
    }
}
