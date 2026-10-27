using Lumen.Companion;
using Lumen.Companion.Sensing;
using Lumen.Core;
using Lumen.Player;
using UnityEngine;
using UnityEngine.AI;

namespace Lumen.EditorTools.SceneBuilding.Sections
{
    /// <summary>Geliştirici 1 — ışık saçan eşlikçi Lumi (NavMeshAgent + RaycastSensor + FSM).</summary>
    public sealed class CompanionSection : ISceneSection
    {
        public static readonly Vector3 SpawnPoint = new Vector3(2f, 0f, 4f);

        public int Order => 20;

        public void Build(SceneBuildContext ctx)
        {
            PlayerController player = ctx.Get<PlayerController>();

            var lumi = new GameObject("Lumi") { layer = GameLayers.CompanionIndex };
            lumi.transform.position = SpawnPoint;

            NavMeshAgent agent = lumi.AddComponent<NavMeshAgent>();
            agent.agentTypeID = 0;
            agent.radius = 0.3f;
            agent.height = 0.9f;
            agent.speed = 3.5f;
            agent.acceleration = 20f;
            agent.angularSpeed = 720f;
            agent.stoppingDistance = 0.05f;

            // Tuzak tetikleyicileriyle temas olayları için kinematik gövde + tetik küre.
            Rigidbody body = lumi.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            SphereCollider sphere = lumi.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
            sphere.radius = 0.3f;
            sphere.center = new Vector3(0f, 0.45f, 0f);

            GameObject orb = ctx.Primitive(PrimitiveType.Sphere, "Orb", lumi.transform, new Vector3(0f, 0.55f, 0f), Vector3.one * 0.42f, ctx.Mat("LumiGlow", new Color(0.9f, 0.95f, 1f), new Color(2.2f, 2.4f, 2.8f)));
            orb.layer = GameLayers.CompanionIndex;
            ctx.Primitive(PrimitiveType.Cube, "Eye", orb.transform, new Vector3(0f, 0.1f, 0.42f), new Vector3(0.5f, 0.18f, 0.2f), ctx.Mat("LumiEye", new Color(0.05f, 0.08f, 0.12f))).layer = GameLayers.CompanionIndex;
            ctx.PointLight("Light", lumi.transform, new Vector3(0f, 0.8f, 0f), new Color(0.92f, 0.96f, 1f), 9f, 2.2f, LightShadows.Soft);

            RaycastSensor sensor = lumi.AddComponent<RaycastSensor>();
            sensor.Configure(GameLayers.HazardMask | (1 << 0));

            CompanionController controller = lumi.AddComponent<CompanionController>();
            controller.Configure(player.transform, sensor);

            ctx.Get<PlayerCommandIssuer>().Configure(controller);
            ctx.Set(controller);

            ctx.SavePrefab(lumi, "Companion_Prefab");
        }
    }
}
