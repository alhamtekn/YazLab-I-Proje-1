using Lumen.Core;
using Lumen.NPC;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace Lumen.EditorTools.SceneBuilding.Sections
{
    /// <summary>
    /// Geliştirici 2 — bekçi bölgesi (z 40-58, Onur'un kristal kapısının ardı): Arven heykeli,
    /// Arven'in kapısı, tapınağın kalbi ve bitiş ekranı. ChatSection'dan sonra çalışır (sohbet ve oyun durumu hazır olmalı).
    /// </summary>
    public sealed class NpcSection : ISceneSection
    {
        public const float DoorZ = 52f;
        private const float DoorHalfWidth = 1.5f;
        private const float DoorHeight = 3.2f;

        public int Order => 36;

        public void Build(SceneBuildContext ctx)
        {
            GameStateManager gameState = ctx.Get<GameStateManager>();
            Transform zone = new GameObject("GuardianZone").transform;

            BuildArven(ctx, zone, ctx.Get<ChatView>(), gameState);
            gameState.SetDoor(BuildDoor(ctx, zone));
            BuildHeart(ctx, zone, gameState);
            BuildEndScreen(ctx, gameState);

            ctx.Torch("Torch_Guardian_L", zone, new Vector3(-7.8f, 2.4f, 45f));
            ctx.Torch("Torch_Guardian_R", zone, new Vector3(7.8f, 2.4f, 45f));
            ctx.Torch("Torch_ArvenDoor_L", zone, new Vector3(-2.2f, 2.6f, DoorZ - 0.45f));
            ctx.Torch("Torch_ArvenDoor_R", zone, new Vector3(2.2f, 2.6f, DoorZ - 0.45f));
        }

        // Kapının solunda duran taş bekçi: yaklaşınca "[E] Arven ile konuş"
        private static void BuildArven(SceneBuildContext ctx, Transform zone, ChatView chat, GameStateManager gameState)
        {
            Material stone = ctx.Mat("ArvenStone", new Color(0.36f, 0.34f, 0.32f));
            Material eyes = ctx.Mat("ArvenEyes", new Color(0.5f, 0.8f, 1f), new Color(0.8f, 1.6f, 2.4f));

            var arven = new GameObject("Arven") { layer = GameLayers.InteractableIndex };
            arven.transform.SetParent(zone, false);
            arven.transform.localPosition = new Vector3(-3f, 0f, 49.5f);
            BoxCollider body = arven.AddComponent<BoxCollider>(); // hem çarpışma hem etkileşim alanı
            body.center = new Vector3(0f, 1.3f, 0f);
            body.size = new Vector3(1.4f, 2.6f, 1.2f);

            ctx.Primitive(PrimitiveType.Cube, "Base", arven.transform, new Vector3(0f, 0.25f, 0f), new Vector3(1.4f, 0.5f, 1.2f), stone);
            ctx.Primitive(PrimitiveType.Cube, "Body", arven.transform, new Vector3(0f, 1.3f, 0f), new Vector3(1f, 1.6f, 0.7f), stone);
            ctx.Primitive(PrimitiveType.Sphere, "Head", arven.transform, new Vector3(0f, 2.35f, 0f), Vector3.one * 0.6f, stone);
            ctx.Primitive(PrimitiveType.Sphere, "Eye_L", arven.transform, new Vector3(-0.12f, 2.4f, -0.27f), Vector3.one * 0.09f, eyes);
            ctx.Primitive(PrimitiveType.Sphere, "Eye_R", arven.transform, new Vector3(0.12f, 2.4f, -0.27f), Vector3.one * 0.09f, eyes);
            ctx.PointLight("EyeGlow", arven.transform, new Vector3(0f, 2.4f, -0.8f), new Color(0.5f, 0.8f, 1f), 3f, 0.8f);

            arven.AddComponent<ArvenNpc>().Configure(chat, gameState);
        }

        // z=52'deki duvar ve ortasındaki kapı (Onur'un kristal kapısıyla aynı yapı: kanat NavMesh'e dahil değil, engel keser)
        private static ArvenDoor BuildDoor(SceneBuildContext ctx, Transform zone)
        {
            Material stone = ctx.Mat("Stone", new Color(0.27f, 0.26f, 0.29f));
            Transform gate = new GameObject("ArvenGate").transform;
            gate.SetParent(zone, false);

            float sideWidth = 8f - DoorHalfWidth;
            float sideCenterX = DoorHalfWidth + sideWidth * 0.5f;
            float wallY = EnvironmentSection.WallHeight * 0.5f;
            ctx.Box("Wall_L", gate, new Vector3(-sideCenterX, wallY, DoorZ), new Vector3(sideWidth, EnvironmentSection.WallHeight, 0.6f), stone);
            ctx.Box("Wall_R", gate, new Vector3(sideCenterX, wallY, DoorZ), new Vector3(sideWidth, EnvironmentSection.WallHeight, 0.6f), stone);
            float lintelHeight = EnvironmentSection.WallHeight - DoorHeight;
            ctx.Box("Lintel", gate, new Vector3(0f, DoorHeight + lintelHeight * 0.5f, DoorZ), new Vector3(DoorHalfWidth * 2f, lintelHeight, 0.6f), stone);

            Material doorMat = ctx.Mat("ArvenDoor", new Color(0.3f, 0.26f, 0.2f), new Color(0.15f, 0.1f, 0.02f), 0.3f, 0.3f);
            GameObject door = ctx.Box("Door", gate, new Vector3(0f, DoorHeight * 0.5f, DoorZ), new Vector3(DoorHalfWidth * 2f, DoorHeight, 0.4f), doorMat, true, 2);
            NavMeshObstacle obstacle = door.AddComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.carving = true;

            Light light = ctx.PointLight("DoorLight", gate, new Vector3(0f, 2.2f, DoorZ + 1.5f), new Color(1f, 0.85f, 0.5f), 7f, 2f);
            ArvenDoor arvenDoor = gate.gameObject.AddComponent<ArvenDoor>();
            arvenDoor.Configure(door.transform, light);
            return arvenDoor;
        }

        // Kapının ardındaki oda: parlayan kristal ve içine girince oyunu bitiren tetik
        private static void BuildHeart(SceneBuildContext ctx, Transform zone, GameStateManager gameState)
        {
            Material heartMat = ctx.Mat("TempleHeart", new Color(1f, 0.9f, 0.6f), new Color(3f, 2.4f, 1.2f), 0.1f, 0.9f);
            var heart = new GameObject("TempleHeart");
            heart.transform.SetParent(zone, false);
            heart.transform.localPosition = new Vector3(0f, 0f, SanctumSection.HeartZ); // iki sınavın ardında, en sonda

            ctx.Primitive(PrimitiveType.Cylinder, "Pedestal", heart.transform, new Vector3(0f, 0.4f, 0f), new Vector3(1f, 0.4f, 1f), ctx.Mat("Stone", new Color(0.27f, 0.26f, 0.29f)));
            ctx.Primitive(PrimitiveType.Sphere, "Crystal", heart.transform, new Vector3(0f, 1.4f, 0f), new Vector3(0.6f, 0.9f, 0.6f), heartMat);
            ctx.PointLight("HeartLight", heart.transform, new Vector3(0f, 1.6f, 0f), new Color(1f, 0.85f, 0.55f), 8f, 2.5f);

            GameObject trigger = ctx.Box("EndTrigger", heart.transform, new Vector3(0f, 1f, -1.5f), new Vector3(10f, 2f, 2f), null, true, 2);
            trigger.GetComponent<BoxCollider>().isTrigger = true;
            trigger.AddComponent<TempleHeart>().Configure(gameState);
        }

        private static void BuildEndScreen(SceneBuildContext ctx, GameStateManager gameState)
        {
            Canvas canvas = HudSection.CreateCanvas("EndScreen", 50);
            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvas.transform, false);
            var rect = (RectTransform)panel.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.8f);

            Text title = HudSection.CreateText(ctx, panel.transform, "Title", new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(1200f, 80f), 56, TextAnchor.MiddleCenter);
            title.text = "Tapınağın kalbine ulaştın";
            title.color = new Color(1f, 0.88f, 0.6f);
            Text body = HudSection.CreateText(ctx, panel.transform, "Body", new Vector2(0.5f, 0.5f), new Vector2(0f, -40f), new Vector2(1200f, 100f), 28, TextAnchor.MiddleCenter);
            body.text = "Lumen'in ilk ışığı yeniden yandı. Arven yeminini tamamladı.";

            Text stats = HudSection.CreateText(ctx, panel.transform, "Stats", new Vector2(0.5f, 0.5f), new Vector2(0f, -110f), new Vector2(1200f, 40f), 26, TextAnchor.MiddleCenter);
            stats.color = new Color(0.85f, 0.92f, 1f);
            Button mainMenu = PauseMenuSection.CreateButton(ctx, PauseMenuSection.LoadUiResources(), panel.transform, "MainMenu", "Ana Menü", 0f);
            var menuRect = (RectTransform)mainMenu.transform;
            menuRect.anchorMin = menuRect.anchorMax = new Vector2(0.5f, 0.5f);
            menuRect.anchoredPosition = new Vector2(0f, -190f);

            canvas.gameObject.AddComponent<EndScreen>().Configure(panel, gameState, stats, mainMenu);
        }
    }
}
