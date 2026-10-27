using Lumen.Core;
using Lumen.Player;
using UnityEngine;

namespace Lumen.EditorTools.SceneBuilding.Sections
{
    /// <summary>Geliştirici 1 — oyuncu karakteri ve üçüncü şahıs kamera.</summary>
    public sealed class PlayerSection : ISceneSection
    {
        public static readonly Vector3 SpawnPoint = new Vector3(0f, 0.05f, 4f);

        public int Order => 10;

        public void Build(SceneBuildContext ctx)
        {
            var player = new GameObject("Player") { layer = GameLayers.PlayerIndex };
            player.transform.position = SpawnPoint;

            CharacterController cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.35f;
            cc.center = new Vector3(0f, 0.9f, 0f);
            cc.stepOffset = 0.3f;

            GameObject body = ctx.Primitive(PrimitiveType.Capsule, "Body", player.transform, new Vector3(0f, 0.9f, 0f), new Vector3(0.7f, 0.9f, 0.7f), ctx.Mat("PlayerCloth", new Color(0.3f, 0.38f, 0.55f)));
            body.layer = GameLayers.PlayerIndex;
            ctx.Primitive(PrimitiveType.Cube, "Visor", body.transform, new Vector3(0f, 0.55f, 0.42f), new Vector3(0.6f, 0.12f, 0.15f), ctx.Mat("Visor", new Color(0.1f, 0.1f, 0.1f), new Color(0.2f, 0.6f, 0.9f))).layer = GameLayers.PlayerIndex;
            ctx.PointLight("FaintGlow", player.transform, new Vector3(0f, 2f, 0f), new Color(0.6f, 0.7f, 1f), 3.5f, 0.35f);

            PlayerController controller = player.AddComponent<PlayerController>();
            player.AddComponent<PlayerRespawn>();
            PlayerInteractor interactor = player.AddComponent<PlayerInteractor>();
            PlayerCommandIssuer issuer = player.AddComponent<PlayerCommandIssuer>();

            var cameraGo = new GameObject("Main Camera") { tag = "MainCamera" };
            Camera cam = cameraGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 120f;
            cameraGo.AddComponent<AudioListener>();
            ThirdPersonCamera follow = cameraGo.AddComponent<ThirdPersonCamera>();
            follow.Configure(player.transform, 1 << 0);
            cameraGo.transform.position = SpawnPoint + new Vector3(0f, 3f, -5f);

            controller.Configure(cameraGo.transform);

            ctx.Set(controller);
            ctx.Set(interactor);
            ctx.Set(issuer);
            ctx.Set(cam);
        }
    }
}
