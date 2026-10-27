using System.Collections.Generic;
using Lumen.Core;
using Lumen.Level;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace Lumen.EditorTools.SceneBuilding.Sections
{
    /// <summary>
    /// Geliştirici 1 — 1. bölüm: başlangıç salonu, kafesli tuzak odası (yalnızca Lumi'nin sığdığı geçitle),
    /// şalter, uçurum ve kalkık köprü. Ayrıca ileri bölgelerin zemini ve dış duvarları.
    ///
    /// Yerleşim (X sağa, Z ileri, birim metre):
    ///   Salon        x[-8, 8]   z[0, 16]
    ///   Tuzak odası  x[-18, -8] z[0, 16]   (salondan demir parmaklıkla ayrılır; z=2'de alçak geçit)
    ///   Uçurum       x[-8, 8]   z[16, 24]  (köprü x[-1.5, 1.5])
    ///   İleri bölge  x[-8, 8]   z[24, 58]  (galeri ve bekçi bölgeleri diğer bölümlerce doldurulur)
    /// </summary>
    public sealed class EnvironmentSection : ISceneSection
    {
        public const float WallHeight = 4f;
        public const float PitDepth = -8f;

        public int Order => 0;

        public void Build(SceneBuildContext ctx)
        {
            Transform level = ctx.Root("Level");
            NavMeshSurface surface = level.gameObject.AddComponent<NavMeshSurface>();
            surface.agentTypeID = 0;
            surface.collectObjects = CollectObjects.All;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.layerMask = 1 << 0;
            ctx.Set(surface);

            BuildShell(ctx, level);
            BuildCageFence(ctx, level);
            BuildTrapRoom(ctx, level);
            BuildChasmAndBridge(ctx, level);
            BuildStartLighting(ctx, level);
        }

        private static void BuildShell(SceneBuildContext ctx, Transform level)
        {
            Material floor = ctx.Mat("Floor", new Color(0.22f, 0.2f, 0.19f));
            Material stone = ctx.Mat("Stone", new Color(0.27f, 0.26f, 0.29f));
            Transform shell = new GameObject("Shell").transform;
            shell.SetParent(level, false);

            ctx.Box("Floor_Hall", shell, new Vector3(0f, -0.25f, 8f), new Vector3(16f, 0.5f, 16f), floor);
            ctx.Box("Floor_TrapRoom", shell, new Vector3(-13f, -0.25f, 8f), new Vector3(10f, 0.5f, 16f), floor);
            ctx.Box("Floor_Far", shell, new Vector3(0f, -0.25f, 41f), new Vector3(16f, 0.5f, 34f), floor);
            ctx.Box("Floor_Pit", shell, new Vector3(0f, PitDepth, 20f), new Vector3(16f, 0.5f, 8f), ctx.Mat("DarkStone", new Color(0.1f, 0.1f, 0.12f)));

            float wallCenterY = (WallHeight + PitDepth) * 0.5f;
            float wallSizeY = WallHeight - PitDepth;
            ctx.Box("Wall_Back", shell, new Vector3(-5f, wallCenterY, -0.25f), new Vector3(26.5f, wallSizeY, 0.5f), stone);
            ctx.Box("Wall_Right", shell, new Vector3(8.25f, wallCenterY, 29f), new Vector3(0.5f, wallSizeY, 59f), stone);
            ctx.Box("Wall_TrapRoom_Left", shell, new Vector3(-18.25f, wallCenterY, 8f), new Vector3(0.5f, wallSizeY, 16f), stone);
            ctx.Box("Wall_TrapRoom_Front", shell, new Vector3(-13.25f, wallCenterY, 16.25f), new Vector3(10.5f, wallSizeY, 0.5f), stone);
            ctx.Box("Wall_Left_Far", shell, new Vector3(-8.25f, wallCenterY, 37.25f), new Vector3(0.5f, wallSizeY, 42.5f), stone);
            ctx.Box("Wall_End", shell, new Vector3(0f, wallCenterY, 58.25f), new Vector3(17f, wallSizeY, 0.5f), stone);

            GameObject killZone = ctx.Box("KillZone", shell, new Vector3(0f, -4f, 20f), new Vector3(40f, 1f, 12f), null, true, 2);
            killZone.GetComponent<Collider>().isTrigger = true;
            killZone.AddComponent<KillZone>();
        }

        /// <summary>
        /// Salon ile tuzak odası arasındaki parmaklıklar. Aralıklar (≈0.22 m) hem oyuncudan hem Lumi'den dardır.
        /// z∈[1.4, 2.6] arasında üstü 1.1 m'de kapanan alçak bir geçit vardır: Lumi (0.9 m) sığar, oyuncu (1.8 m) sığmaz.
        /// </summary>
        private static void BuildCageFence(SceneBuildContext ctx, Transform level)
        {
            Material metal = ctx.Mat("Metal", new Color(0.32f, 0.32f, 0.36f), null, 0.8f, 0.45f);
            Material stone = ctx.Mat("Stone", new Color(0.27f, 0.26f, 0.29f));
            Transform fence = new GameObject("CageFence").transform;
            fence.SetParent(level, false);

            for (float z = 0.15f; z < 16f; z += 0.3f)
            {
                if (z > 1.35f && z < 2.65f)
                {
                    continue;
                }

                ctx.Box("Bar", fence, new Vector3(-8f, 1.8f, z), new Vector3(0.08f, 3.6f, 0.08f), metal);
            }

            ctx.Box("TopBeam", fence, new Vector3(-8f, 3.75f, 8f), new Vector3(0.25f, 0.3f, 16f), metal);
            ctx.Box("CrawlLintel", fence, new Vector3(-8f, 2.35f, 2f), new Vector3(0.4f, 2.5f, 1.4f), stone);
        }

        private static void BuildTrapRoom(SceneBuildContext ctx, Transform level)
        {
            Material stone = ctx.Mat("Stone", new Color(0.27f, 0.26f, 0.29f));
            Transform room = new GameObject("TrapRoom").transform;
            room.SetParent(level, false);

            // İki sıra alçak duvar; her sırada tuzaklı geçitler. Lumi açık geçidi NavMesh ile seçer.
            foreach (Vector2 span in new[] { new Vector2(-18f, -16.8f), new Vector2(-15.2f, -13.8f), new Vector2(-12.2f, -10.8f), new Vector2(-9.2f, -8f) })
            {
                ctx.Box("Row1_Block", room, new Vector3((span.x + span.y) * 0.5f, 0.75f, 6f), new Vector3(span.y - span.x, 1.5f, 0.6f), stone);
            }

            foreach (Vector2 span in new[] { new Vector2(-18f, -15.8f), new Vector2(-14.2f, -11.8f), new Vector2(-10.2f, -8f) })
            {
                ctx.Box("Row2_Block", room, new Vector3((span.x + span.y) * 0.5f, 0.75f, 11f), new Vector3(span.y - span.x, 1.5f, 0.6f), stone);
            }

            Transform resetPoint = new GameObject("TrapResetPoint").transform;
            resetPoint.SetParent(room, false);
            resetPoint.localPosition = new Vector3(-9.3f, 0f, 2f);

            var traps = new List<SpikeTrap>();
            foreach (Vector2 p in new[] { new Vector2(-16f, 6f), new Vector2(-13f, 6f), new Vector2(-10f, 6f), new Vector2(-15f, 11f), new Vector2(-11f, 11f) })
            {
                traps.Add(BuildSpikeTrap(ctx, room, new Vector3(p.x, 0f, p.y)));
            }

            SpikeTrapField field = room.gameObject.AddComponent<SpikeTrapField>();
            field.Configure(traps.ToArray(), resetPoint);
            ctx.Set(field);

            ctx.Torch("Torch_TrapRoom_A", room, new Vector3(-17.8f, 2.4f, 3f));
            ctx.Torch("Torch_TrapRoom_B", room, new Vector3(-17.8f, 2.4f, 13f));
        }

        private static SpikeTrap BuildSpikeTrap(SceneBuildContext ctx, Transform parent, Vector3 position)
        {
            const float tile = 1.6f;
            var root = new GameObject("SpikeTrap") { layer = GameLayers.HazardIndex };
            root.transform.SetParent(parent, false);
            root.transform.localPosition = position;

            BoxCollider trigger = root.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 0.3f, 0f);
            trigger.size = new Vector3(tile, 0.6f, tile);

            GameObject plate = ctx.Box("Plate", root.transform, new Vector3(0f, 0.015f, 0f), new Vector3(tile, 0.03f, tile),
                ctx.Mat("TrapPlate", new Color(0.18f, 0.12f, 0.1f), new Color(0.05f, 0.05f, 0.05f)), false, GameLayers.HazardIndex);

            Transform spikes = new GameObject("Spikes").transform;
            spikes.SetParent(root.transform, false);
            Material spikeMat = ctx.Mat("SpikeMetal", new Color(0.55f, 0.55f, 0.6f), null, 0.9f, 0.6f);
            for (int x = -1; x <= 1; x++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    GameObject spike = ctx.Box("Spike", spikes, new Vector3(x * 0.5f, 0f, z * 0.5f), new Vector3(0.12f, 0.5f, 0.12f), spikeMat, false, GameLayers.HazardIndex);
                    spike.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
                }
            }

            NavMeshObstacle obstacle = root.AddComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.center = new Vector3(0f, 0.5f, 0f);
            obstacle.size = new Vector3(tile, 1f, tile);
            obstacle.carving = true;
            obstacle.enabled = false;

            SpikeTrap trap = root.AddComponent<SpikeTrap>();
            trap.Configure(spikes, plate.GetComponent<Renderer>(), obstacle);
            return trap;
        }

        private static void BuildChasmAndBridge(SceneBuildContext ctx, Transform level)
        {
            Material wood = ctx.Mat("Wood", new Color(0.3f, 0.18f, 0.1f));
            Material metal = ctx.Mat("Metal", new Color(0.32f, 0.32f, 0.36f), null, 0.8f, 0.45f);
            Transform chasm = new GameObject("ChasmAndBridge").transform;
            chasm.SetParent(level, false);

            // Köprü uçurumun karşı kenarındaki menteşe etrafında döner; NavMesh inik hâliyle pişirilir.
            Transform pivot = new GameObject("BridgePivot").transform;
            pivot.SetParent(chasm, false);
            pivot.localPosition = new Vector3(0f, 0f, 24f);
            ctx.Box("Deck", pivot, new Vector3(0f, -0.15f, -4f), new Vector3(3f, 0.3f, 8f), wood);
            ctx.Box("Rail_L", pivot, new Vector3(-1.45f, 0.5f, -4f), new Vector3(0.1f, 1f, 8f), wood);
            ctx.Box("Rail_R", pivot, new Vector3(1.45f, 0.5f, -4f), new Vector3(0.1f, 1f, 8f), wood);

            ctx.InvisibleWall("EdgeBarrier_L", chasm, new Vector3(-4.75f, 1.5f, 15.85f), new Vector3(6.5f, 3f, 0.3f));
            ctx.InvisibleWall("EdgeBarrier_R", chasm, new Vector3(4.75f, 1.5f, 15.85f), new Vector3(6.5f, 3f, 0.3f));
            GameObject centerBarrier = ctx.InvisibleWall("EdgeBarrier_Bridge", chasm, new Vector3(0f, 1.5f, 15.85f), new Vector3(3f, 3f, 0.3f));
            ctx.InvisibleWall("FarEdgeBarrier_L", chasm, new Vector3(-4.75f, 1.5f, 24.15f), new Vector3(6.5f, 3f, 0.3f));
            ctx.InvisibleWall("FarEdgeBarrier_R", chasm, new Vector3(4.75f, 1.5f, 24.15f), new Vector3(6.5f, 3f, 0.3f));

            var navBlocker = new GameObject("BridgeNavBlocker");
            navBlocker.transform.SetParent(chasm, false);
            navBlocker.transform.localPosition = new Vector3(0f, 0.5f, 20f);
            NavMeshObstacle obstacle = navBlocker.AddComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.size = new Vector3(3f, 1f, 8f);
            obstacle.carving = true;

            Drawbridge bridge = chasm.gameObject.AddComponent<Drawbridge>();
            bridge.Configure(pivot, new[] { centerBarrier, navBlocker });
            ctx.Set(bridge);

            // Şalter: tuzak odasının arka duvarında; yalnızca Lumi ulaşabilir.
            Transform switchRoot = new GameObject("Switch").transform;
            switchRoot.SetParent(level, false);
            switchRoot.localPosition = new Vector3(-13f, 0f, 15.75f);
            ctx.Box("Base", switchRoot, new Vector3(0f, 0.6f, 0f), new Vector3(0.8f, 1.2f, 0.4f), metal);
            Transform lever = new GameObject("LeverPivot").transform;
            lever.SetParent(switchRoot, false);
            lever.localPosition = new Vector3(0f, 0.9f, -0.2f);
            lever.localRotation = Quaternion.Euler(-35f, 0f, 0f);
            ctx.Box("Handle", lever, new Vector3(0f, 0.3f, 0f), new Vector3(0.08f, 0.6f, 0.08f), wood, false);
            Light indicator = ctx.PointLight("Indicator", switchRoot, new Vector3(0f, 1.4f, -0.4f), new Color(1f, 0.2f, 0.15f), 3f, 1.5f);
            Transform approach = new GameObject("ApproachPoint").transform;
            approach.SetParent(switchRoot, false);
            approach.localPosition = new Vector3(0f, 0f, -0.85f);

            SwitchMechanism mechanism = switchRoot.gameObject.AddComponent<SwitchMechanism>();
            mechanism.Configure(approach, lever, indicator, new MonoBehaviour[] { bridge });
            ctx.Set(mechanism);

            GameObject checkpoint = ctx.Box("Checkpoint_AfterBridge", chasm, new Vector3(0f, 1f, 26f), new Vector3(16f, 2f, 1f), null, true, 2);
            checkpoint.GetComponent<Collider>().isTrigger = true;
            checkpoint.AddComponent<Checkpoint>();
        }

        private static void BuildStartLighting(SceneBuildContext ctx, Transform level)
        {
            Transform lights = new GameObject("StartLights").transform;
            lights.SetParent(level, false);
            ctx.Torch("Torch_Hall_A", lights, new Vector3(7.8f, 2.4f, 3f));
            ctx.Torch("Torch_Hall_B", lights, new Vector3(7.8f, 2.4f, 13f));
            ctx.Torch("Torch_FarSide", lights, new Vector3(-7.8f, 2.4f, 25.5f));
        }
    }
}
