using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lumen.EditorTools.SceneBuilding
{
    /// <summary>
    /// "Lumen/Sahneyi Yeniden Oluştur" menüsü: tüm ISceneSection bölümlerini sırayla çalıştırır,
    /// NavMesh'i pişirir ve sahneyi Assets/_Project/Scenes/Lumen.unity olarak kaydeder.
    /// Komut satırı: -executeMethod Lumen.EditorTools.SceneBuilding.LumenSceneBuilder.BuildFromCommandLine
    /// </summary>
    public static class LumenSceneBuilder
    {
        public const string SceneFolder = "Assets/_Project/Scenes";
        public const string ScenePath = SceneFolder + "/Lumen.unity";
        private const string NavMeshDataPath = SceneFolder + "/Lumen_NavMesh.asset";

        [MenuItem("Lumen/Sahneyi Yeniden Oluştur")]
        public static void BuildFromMenu()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Build();
            }
        }

        public static void BuildFromCommandLine()
        {
            try
            {
                Build();
                EditorApplication.Exit(0);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                EditorApplication.Exit(1);
            }
        }

        public static void Build()
        {
            SceneBuildContext.EnsureFolder(SceneFolder);
            SceneBuildContext.EnsureFolder(SceneBuildContext.MaterialFolder);
            SceneBuildContext.EnsureFolder(SceneBuildContext.PrefabFolder);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var context = new SceneBuildContext();

            foreach (ISceneSection section in DiscoverSections())
            {
                Debug.Log($"[LumenSceneBuilder] {section.GetType().Name} (sıra {section.Order})");
                section.Build(context);
            }

            ApplyLighting();
            EditorSceneManager.SaveScene(scene, ScenePath);
            BakeNavMesh(context);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);

            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("[LumenSceneBuilder] Sahne oluşturuldu: " + ScenePath);
        }

        private static IEnumerable<ISceneSection> DiscoverSections()
        {
            return TypeCache.GetTypesDerivedFrom<ISceneSection>()
                .Where(t => !t.IsAbstract && t.GetConstructor(Type.EmptyTypes) != null)
                .Select(t => (ISceneSection)Activator.CreateInstance(t))
                .OrderBy(s => s.Order);
        }

        private static void ApplyLighting()
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.05f, 0.055f, 0.075f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogColor = new Color(0.01f, 0.01f, 0.02f);
            RenderSettings.fogDensity = 0.035f;
            QualitySettings.pixelLightCount = 8;
        }

        private static void BakeNavMesh(SceneBuildContext context)
        {
            NavMeshSurface surface = context.Get<NavMeshSurface>();
            surface.BuildNavMesh();

            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(NavMeshDataPath) != null)
            {
                AssetDatabase.DeleteAsset(NavMeshDataPath);
            }

            AssetDatabase.CreateAsset(surface.navMeshData, NavMeshDataPath);
            EditorUtility.SetDirty(surface);
        }
    }
}
