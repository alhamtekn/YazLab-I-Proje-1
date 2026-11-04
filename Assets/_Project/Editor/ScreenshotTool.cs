using System.IO;
using Lumen.Companion;
using Lumen.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Lumen.EditorTools
{
    /// <summary>
    /// "Lumen/Ekran Görüntüleri Al": Lumen sahnesinden README ve rapor için sabit açılı görüntüler alır.
    /// Her karede Lumi o bölümün bulmacasındaki yerine konur; oyuncu gövdesi çizilmez. Sahne kaydedilmez.
    /// Görüntüler proje kökündeki Docs/screenshots klasörüne yazılır (Assets dışında, Unity içe aktarmaz).
    /// Komut satırı: -executeMethod Lumen.EditorTools.ScreenshotTool.CaptureFromCommandLine
    /// </summary>
    public static class ScreenshotTool
    {
        private const string ScenePath = "Assets/_Project/Scenes/Lumen.unity";
        private const int Width = 1280;
        private const int Height = 720;

        private readonly struct Shot
        {
            public readonly string Name;
            public readonly Vector3 Camera;
            public readonly Vector3 Target;
            public readonly Vector3 Lumi;

            public Shot(string name, Vector3 camera, Vector3 target, Vector3 lumi)
            {
                Name = name;
                Camera = camera;
                Target = target;
                Lumi = lumi;
            }
        }

        private static readonly Shot[] Shots =
        {
            new Shot("01_koridor_lumi", new Vector3(0f, 1.6f, -6f), new Vector3(0f, 0.6f, -2f), new Vector3(0f, 0f, -2f)),
            new Shot("02_tuzak_odasi_salter", new Vector3(-9.5f, 2.6f, 8f), new Vector3(-13f, 0.8f, 15f), new Vector3(-12.9f, 0f, 14.9f)),
            new Shot("03_kopru_ve_lav", new Vector3(5.5f, 2.8f, 14.5f), new Vector3(0f, -5f, 21f), new Vector3(0f, 0f, 14f)),
            new Shot("04_karanlik_oda", new Vector3(-7.2f, 1.7f, 34.6f), new Vector3(-11f, 0f, 36.5f), new Vector3(-10.15f, 0f, 37.1f)),
            new Shot("05_labirent_kapi_ve_nis", new Vector3(9.2f, 1.7f, 27.2f), new Vector3(10.6f, 1.2f, 31f), new Vector3(9.5f, 0f, 32.75f)),
            new Shot("06_diken_odasi_plakalar", new Vector3(8.6f, 1.8f, 35.2f), new Vector3(12.5f, 0f, 37f), new Vector3(13.3f, 0f, 34.4f)),
            new Shot("07_galeri_ve_kristal_kapisi", new Vector3(0f, 1.7f, 25.5f), new Vector3(0f, 1.4f, 40f), new Vector3(1f, 0f, 27f)),
            new Shot("08_arven", new Vector3(0f, 1.7f, 44f), new Vector3(-3f, 1.5f, 49.5f), new Vector3(1f, 0f, 45f)),
            new Shot("09_kutsal_bolum", new Vector3(0f, 1.7f, 61f), new Vector3(0f, 1f, 75f), new Vector3(1f, 0f, 62f)),
        };

        [MenuItem("Lumen/Ekran Görüntüleri Al")]
        public static void CaptureFromMenu()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                string folder = Capture();
                EditorUtility.RevealInFinder(folder);
            }
        }

        public static void CaptureFromCommandLine()
        {
            Capture();
            EditorApplication.Exit(0);
        }

        private static string Capture()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Docs", "screenshots"));
            Directory.CreateDirectory(folder);

            Transform lumi = Object.FindAnyObjectByType<CompanionController>().transform;
            Vector3 lumiStart = lumi.position;

            var cameraObject = new GameObject("ScreenshotCamera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 70f;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 120f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.cullingMask = ~(1 << GameLayers.PlayerIndex);

            var target = new RenderTexture(Width, Height, 24) { antiAliasing = 4 };
            var image = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            try
            {
                foreach (Shot shot in Shots)
                {
                    lumi.position = shot.Lumi;
                    camera.transform.position = shot.Camera;
                    camera.transform.LookAt(shot.Target);
                    camera.targetTexture = target;
                    camera.Render();

                    RenderTexture.active = target;
                    image.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
                    image.Apply();
                    File.WriteAllBytes(Path.Combine(folder, shot.Name + ".png"), image.EncodeToPNG());
                }
            }
            finally
            {
                RenderTexture.active = null;
                camera.targetTexture = null;
                lumi.position = lumiStart;
                Object.DestroyImmediate(cameraObject);
                Object.DestroyImmediate(target);
                Object.DestroyImmediate(image);
            }

            Debug.Log($"[ScreenshotTool] {Shots.Length} görüntü: {folder}");
            return folder;
        }
    }
}
