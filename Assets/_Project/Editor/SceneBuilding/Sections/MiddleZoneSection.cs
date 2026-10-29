using System.Collections.Generic;
using Lumen.Core;
using Lumen.Level;
using UnityEngine;
using UnityEngine.AI;

namespace Lumen.EditorTools.SceneBuilding.Sections
{
    /// <summary>
    /// Geliştirici 1 — 2. bölüm (kristal galerisi): köprüden sonra, bekçinin kapısından önce.
    /// Üç kristal, duvar yazıtları, sütunlar, alçak duvarlar, seyrek meşaleler ve z=40'ta kapalı kapı.
    /// Zemin ve dış duvarlar EnvironmentSection'dadır.
    ///
    /// Yerleşim (X sağa, Z ileri, birim metre):
    ///   Galeri  x[-8, 8]  z[24, 40]
    ///   Kapı    x[-1.5, 1.5] z=40  (ötesi bekçi bölgesi, Geliştirici 2)
    ///
    /// Paylaşılanlar: LoreJournal (NPC okur), CrystalCounter, LoreInscription[].
    /// </summary>
    public sealed class MiddleZoneSection : ISceneSection
    {
        public const float DoorZ = 40f;
        public const float DoorHalfWidth = 1.5f;
        public const float DoorHeight = 3.2f;

        public int Order => 5;

        public void Build(SceneBuildContext ctx)
        {
            Transform zone = new GameObject("MiddleZone").transform;
            zone.SetParent(ctx.Root("Level"), false);

            BuildObstacles(ctx, zone);
            BuildCrystals(ctx, zone);
            BuildInscriptions(ctx, zone);
            BuildGate(ctx, zone);
            BuildLighting(ctx, zone);
        }

        /// <summary>Sütunlar ve alçak duvarlar. Aralıklar oyuncu ve Lumi için en az 2 m.</summary>
        private static void BuildObstacles(SceneBuildContext ctx, Transform zone)
        {
            Material stone = ctx.Mat("Stone", new Color(0.27f, 0.26f, 0.29f));
            Material pillarMat = ctx.Mat("PillarStone", new Color(0.33f, 0.31f, 0.3f));
            Transform obstacles = new GameObject("Obstacles").transform;
            obstacles.SetParent(zone, false);

            foreach (Vector2 p in new[] { new Vector2(-3f, 29f), new Vector2(3f, 29f), new Vector2(-3f, 35f), new Vector2(3f, 35f) })
            {
                ctx.Box("Pillar", obstacles, new Vector3(p.x, EnvironmentSection.WallHeight * 0.5f, p.y), new Vector3(1f, EnvironmentSection.WallHeight, 1f), pillarMat);
            }

            ctx.Box("LowWall_Left", obstacles, new Vector3(-5.5f, 0.6f, 32f), new Vector3(4f, 1.2f, 0.5f), stone);
            ctx.Box("LowWall_Right", obstacles, new Vector3(5.5f, 0.6f, 27.5f), new Vector3(4f, 1.2f, 0.5f), stone);
            ctx.Box("LowWall_Back", obstacles, new Vector3(5f, 0.6f, 37.5f), new Vector3(5f, 1.2f, 0.5f), stone);
        }

        /// <summary>Kristaller karanlık köşelerde, alçak duvarların arkasında; kendi ışıklarıyla bulunur.</summary>
        private static void BuildCrystals(SceneBuildContext ctx, Transform zone)
        {
            Transform crystalsRoot = new GameObject("Crystals").transform;
            crystalsRoot.SetParent(zone, false);

            var crystals = new List<CrystalPickup>();
            foreach (Vector2 p in new[] { new Vector2(-6.5f, 34f), new Vector2(6.5f, 25.8f), new Vector2(6.5f, 39f) })
            {
                crystals.Add(BuildCrystal(ctx, crystalsRoot, new Vector3(p.x, 0f, p.y)));
            }

            CrystalCounter counter = crystalsRoot.gameObject.AddComponent<CrystalCounter>();
            counter.Configure(crystals.ToArray(), crystals.Count);
            ctx.Set(counter);
        }

        private static CrystalPickup BuildCrystal(SceneBuildContext ctx, Transform parent, Vector3 position)
        {
            var root = new GameObject("Crystal") { layer = GameLayers.InteractableIndex };
            root.transform.SetParent(parent, false);
            root.transform.localPosition = position;

            SphereCollider trigger = root.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 0.9f, 0f);
            trigger.radius = 0.8f;

            Transform visual = new GameObject("Visual").transform;
            visual.SetParent(root.transform, false);
            visual.localPosition = new Vector3(0f, 0.9f, 0f);
            Material crystalMat = ctx.Mat("Crystal", new Color(0.45f, 0.85f, 1f), new Color(0.6f, 1.6f, 2.2f), 0.1f, 0.9f);
            GameObject shard = ctx.Primitive(PrimitiveType.Cube, "Shard", visual, Vector3.zero, new Vector3(0.3f, 0.55f, 0.3f), crystalMat);
            shard.transform.localRotation = Quaternion.Euler(45f, 0f, 45f);

            ctx.Box("Pedestal", root.transform, new Vector3(0f, 0.2f, 0f), new Vector3(0.6f, 0.4f, 0.6f), ctx.Mat("DarkStone", new Color(0.1f, 0.1f, 0.12f)), false);

            Light glow = ctx.PointLight("Glow", root.transform, new Vector3(0f, 1f, 0f), new Color(0.45f, 0.85f, 1f), 4f, 1.2f);

            CrystalPickup pickup = root.AddComponent<CrystalPickup>();
            pickup.Configure(visual, glow);
            return pickup;
        }

        /// <summary>Yan duvarlardaki yazıtlar. Okunanlar LoreJournal'a yazılır; NPC bölümü ctx.Get&lt;LoreJournal&gt;() ile alır.</summary>
        private static void BuildInscriptions(SceneBuildContext ctx, Transform zone)
        {
            Transform inscriptionsRoot = new GameObject("Inscriptions").transform;
            inscriptionsRoot.SetParent(zone, false);

            LoreJournal journal = inscriptionsRoot.gameObject.AddComponent<LoreJournal>();
            ctx.Set(journal);

            // Duvar x=±8; yazıt yüzü galeriye bakar.
            var placements = new[]
            {
                (position: new Vector3(-7.9f, 0f, 28f), facingRight: true),
                (position: new Vector3(7.9f, 0f, 32f), facingRight: false),
                (position: new Vector3(-7.9f, 0f, 38f), facingRight: true),
            };

            var inscriptions = new List<LoreInscription>();
            for (int i = 0; i < placements.Length && i < LoreTexts.All.Length; i++)
            {
                inscriptions.Add(BuildInscription(ctx, inscriptionsRoot, placements[i].position, placements[i].facingRight, LoreTexts.All[i], journal));
            }

            ctx.Set(inscriptions.ToArray());
        }

        private static LoreInscription BuildInscription(SceneBuildContext ctx, Transform parent, Vector3 position, bool facingRight, LoreTexts.Entry entry, LoreJournal journal)
        {
            var root = new GameObject("Inscription") { layer = GameLayers.InteractableIndex };
            root.transform.SetParent(parent, false);
            root.transform.localPosition = position;
            root.transform.localRotation = Quaternion.Euler(0f, facingRight ? 90f : -90f, 0f);

            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, 1.5f, 0.1f);
            collider.size = new Vector3(1.6f, 1.4f, 0.2f);

            ctx.Box("Slab", root.transform, new Vector3(0f, 1.5f, 0.05f), new Vector3(1.6f, 1.4f, 0.1f), ctx.Mat("InscriptionStone", new Color(0.36f, 0.34f, 0.32f)), false);
            Material glyphMat = ctx.Mat("Glyph", new Color(0.5f, 0.75f, 1f), new Color(0.35f, 0.6f, 1.2f));
            for (int row = 0; row < 3; row++)
            {
                ctx.Box("Glyphs", root.transform, new Vector3(0f, 1.85f - row * 0.3f, 0.11f), new Vector3(1.1f - row * 0.2f, 0.06f, 0.02f), glyphMat, false);
            }

            Light glyphLight = ctx.PointLight("GlyphLight", root.transform, new Vector3(0f, 1.5f, 0.6f), new Color(0.5f, 0.7f, 1f), 2.5f, 0.6f);

            LoreInscription inscription = root.AddComponent<LoreInscription>();
            inscription.Configure(entry.Title, entry.Text, journal, glyphLight);
            return inscription;
        }

        /// <summary>
        /// Bekçi bölgesine açılan kapı. Kapı kanadı Ignore Raycast katmanında olduğu için NavMesh'e
        /// dahil edilmez; yol pişirilir ama kapı kapalıyken NavMeshObstacle geçişi keser.
        /// </summary>
        private static void BuildGate(SceneBuildContext ctx, Transform zone)
        {
            Material stone = ctx.Mat("Stone", new Color(0.27f, 0.26f, 0.29f));
            Transform gate = new GameObject("CrystalGate").transform;
            gate.SetParent(zone, false);

            float sideWidth = 8f - DoorHalfWidth;
            float sideCenterX = DoorHalfWidth + sideWidth * 0.5f;
            ctx.Box("GateWall_L", gate, new Vector3(-sideCenterX, EnvironmentSection.WallHeight * 0.5f, DoorZ), new Vector3(sideWidth, EnvironmentSection.WallHeight, 0.6f), stone);
            ctx.Box("GateWall_R", gate, new Vector3(sideCenterX, EnvironmentSection.WallHeight * 0.5f, DoorZ), new Vector3(sideWidth, EnvironmentSection.WallHeight, 0.6f), stone);
            float lintelHeight = EnvironmentSection.WallHeight - DoorHeight;
            ctx.Box("GateLintel", gate, new Vector3(0f, DoorHeight + lintelHeight * 0.5f, DoorZ), new Vector3(DoorHalfWidth * 2f, lintelHeight, 0.6f), stone);

            Material doorMat = ctx.Mat("GateDoor", new Color(0.2f, 0.22f, 0.26f), null, 0.6f, 0.4f);
            GameObject door = ctx.Box("GateDoor", gate, new Vector3(0f, DoorHeight * 0.5f, DoorZ), new Vector3(DoorHalfWidth * 2f, DoorHeight, 0.4f), doorMat, true, 2);

            Material socketMat = ctx.Mat("CrystalSocket", new Color(0.12f, 0.15f, 0.2f), new Color(0.05f, 0.1f, 0.15f));
            for (int i = -1; i <= 1; i++)
            {
                ctx.Primitive(PrimitiveType.Sphere, "Socket", door.transform, new Vector3(i * 0.25f, 0.2f, -0.55f), new Vector3(0.12f, 0.08f, 0.3f), socketMat);
            }

            NavMeshObstacle obstacle = door.AddComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.carving = true;
        }

        /// <summary>Galeri kasıtlı olarak karanlık: az sayıda meşale, bazıları sönük. Asıl ışık Lumi ve kristallerden gelir.</summary>
        private static void BuildLighting(SceneBuildContext ctx, Transform zone)
        {
            Transform lights = new GameObject("Lights").transform;
            lights.SetParent(zone, false);
            ctx.Torch("Torch_Gallery_A", lights, new Vector3(7.8f, 2.4f, 29.5f));
            ctx.Torch("Torch_Gallery_Dead_A", lights, new Vector3(-7.8f, 2.4f, 31.5f), false);
            ctx.Torch("Torch_Gallery_Dead_B", lights, new Vector3(7.8f, 2.4f, 35.5f), false);
            ctx.Torch("Torch_Gate_L", lights, new Vector3(-2.2f, 2.6f, DoorZ - 0.45f));
            ctx.Torch("Torch_Gate_R", lights, new Vector3(2.2f, 2.6f, DoorZ - 0.45f));
        }
    }
}
