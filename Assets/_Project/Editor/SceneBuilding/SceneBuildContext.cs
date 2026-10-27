using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Lumen.EditorTools.SceneBuilding
{
    /// <summary>
    /// Bölümlerin paylaştığı yardımcılar: malzeme önbelleği, kutu/ışık oluşturma, prefab kaydı
    /// ve bölümler arası referans paylaşımı (örn. eşlikçi bölümü oyuncuyu buradan alır).
    /// </summary>
    public sealed class SceneBuildContext
    {
        public const string MaterialFolder = "Assets/_Project/Materials";
        public const string PrefabFolder = "Assets/_Project/Prefabs";

        private readonly Dictionary<Type, object> shared = new Dictionary<Type, object>();
        private readonly Dictionary<string, Transform> roots = new Dictionary<string, Transform>();
        private readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();

        public Font UiFont { get; } = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        public void Set<T>(T value) where T : class => shared[typeof(T)] = value;

        public T Get<T>() where T : class
        {
            if (shared.TryGetValue(typeof(T), out object value))
            {
                return (T)value;
            }

            throw new InvalidOperationException($"Sahne bağlamında {typeof(T).Name} yok. Bölüm sırasını kontrol edin.");
        }

        public bool TryGet<T>(out T value) where T : class
        {
            bool found = shared.TryGetValue(typeof(T), out object raw);
            value = raw as T;
            return found;
        }

        public Transform Root(string name)
        {
            if (!roots.TryGetValue(name, out Transform root))
            {
                root = new GameObject(name).transform;
                roots[name] = root;
            }

            return root;
        }

        public Material Mat(string name, Color color, Color? emission = null, float metallic = 0f, float smoothness = 0.2f)
        {
            if (materials.TryGetValue(name, out Material cached))
            {
                return cached;
            }

            string path = $"{MaterialFolder}/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Standard"));
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = color;
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Glossiness", smoothness);
            if (emission.HasValue)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emission.Value);
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            else
            {
                material.DisableKeyword("_EMISSION");
            }

            EditorUtility.SetDirty(material);
            materials[name] = material;
            return material;
        }

        /// <summary>Merkez ve boyutla küp oluşturur. collider=false ise yalnızca görseldir.</summary>
        public GameObject Box(string name, Transform parent, Vector3 center, Vector3 size, Material material, bool collider = true, int layer = 0)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.layer = layer;
            box.transform.SetParent(parent, false);
            box.transform.localPosition = center;
            box.transform.localScale = size;

            if (material != null)
            {
                box.GetComponent<Renderer>().sharedMaterial = material;
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(box.GetComponent<Renderer>());
                UnityEngine.Object.DestroyImmediate(box.GetComponent<MeshFilter>());
            }

            if (!collider)
            {
                UnityEngine.Object.DestroyImmediate(box.GetComponent<Collider>());
            }

            return box;
        }

        /// <summary>Görünmez, yalnızca çarpışan duvar. NavMesh pişirmesine dahil edilmez (Ignore Raycast katmanı).</summary>
        public GameObject InvisibleWall(string name, Transform parent, Vector3 center, Vector3 size)
        {
            return Box(name, parent, center, size, null, true, 2);
        }

        public GameObject Primitive(PrimitiveType type, string name, Transform parent, Vector3 localPosition, Vector3 scale, Material material, bool collider = false)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            if (!collider)
            {
                UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
            }

            return go;
        }

        public Light PointLight(string name, Transform parent, Vector3 localPosition, Color color, float range, float intensity, LightShadows shadows = LightShadows.None)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            Light light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.range = range;
            light.intensity = intensity;
            light.shadows = shadows;
            return light;
        }

        /// <summary>Duvar meşalesi: küçük gövde, parlayan alev ve turuncu nokta ışığı.</summary>
        public GameObject Torch(string name, Transform parent, Vector3 position, bool lit = true)
        {
            var torch = new GameObject(name);
            torch.transform.SetParent(parent, false);
            torch.transform.localPosition = position;
            Primitive(PrimitiveType.Cylinder, "Handle", torch.transform, new Vector3(0f, -0.25f, 0f), new Vector3(0.08f, 0.25f, 0.08f), Mat("Wood", new Color(0.3f, 0.18f, 0.1f)));
            GameObject flame = Primitive(PrimitiveType.Sphere, "Flame", torch.transform, Vector3.zero, Vector3.one * 0.18f, Mat("Flame", new Color(1f, 0.55f, 0.15f), new Color(3f, 1.2f, 0.3f)));
            flame.SetActive(lit);
            Light light = PointLight("Light", torch.transform, new Vector3(0f, 0.1f, 0f), new Color(1f, 0.6f, 0.3f), 7f, 1.3f);
            light.enabled = lit;
            return torch;
        }

        public GameObject SavePrefab(GameObject instance, string prefabName)
        {
            string path = $"{PrefabFolder}/{prefabName}.prefab";
            return PrefabUtility.SaveAsPrefabAssetAndConnect(instance, path, InteractionMode.AutomatedAction);
        }

        public static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
