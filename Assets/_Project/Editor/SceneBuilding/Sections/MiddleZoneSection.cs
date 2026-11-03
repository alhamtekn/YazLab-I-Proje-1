using System.Collections.Generic;
using Lumen.Core;
using Lumen.Level;
using UnityEditor;
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
    ///   Kristal odaları (6×6 m, galerinin yan duvarlarından 2 m'lik kapılarla girilir):
    ///     A Karanlık oda   x[-14, -8] z[33, 39]     kapı x=-8 z[34, 36]
    ///     B Sütun labirenti x[8, 14]  z[26, 32]     kapı x=8  z[27, 29]
    ///     C Diken odası     x[8, 14]  z[33.5, 39.5] kapı x=8  z[35, 37]
    ///
    /// Paylaşılanlar: LoreJournal (NPC okur), CrystalCounter, LoreInscription[].
    /// </summary>
    public sealed class MiddleZoneSection : ISceneSection
    {
        public const float DoorZ = 40f;
        public const float DoorHalfWidth = 1.5f;
        public const float DoorHeight = 3.2f;
        public const float RoomDepth = 6f;
        private const float CrawlHeight = 1.1f;
        private static readonly Vector2[] MazeCrawls = { new Vector2(9f, 10f), new Vector2(11f, 12f) };
        private const int DarkRoomCells = 5;
        private const float DarkRoomCellSize = 1.1f;

        /// <summary>Dış duvarlardaki oda kapılarının z aralıkları; EnvironmentSection duvarı bunlara göre böler.</summary>
        public static readonly Vector2[] WestRoomDoors = { new Vector2(34f, 36f) };
        public static readonly Vector2[] EastRoomDoors = { new Vector2(27f, 29f), new Vector2(35f, 37f) };

        public int Order => 5;

        public void Build(SceneBuildContext ctx)
        {
            Transform zone = new GameObject("MiddleZone").transform;
            zone.SetParent(ctx.Root("Level"), false);

            BuildDarkRoom(ctx, BuildRoomShell(ctx, zone, "Room_A_Dark", false, 33f, 39f, withFloor: false));
            BuildMaze(ctx, BuildRoomShell(ctx, zone, "Room_B_Maze", true, 26f, 32f, northCrawls: MazeCrawls));
            BuildSpikeRoom(ctx, BuildRoomShell(ctx, zone, "Room_C_Spikes", true, 33.5f, 39.5f));
            BuildObstacles(ctx, zone);
            CrystalGate gate = BuildGate(ctx, zone);
            BuildCrystals(ctx, zone, gate);
            BuildInscriptions(ctx, zone);
            BuildLighting(ctx, zone);
        }

        /// <summary>
        /// Galerinin dışındaki oda: zemin ve üç duvar (galeri tarafındaki duvar EnvironmentSection'da, kapı boşluğuyla).
        /// Normal katmanda kalır ki NavMesh'e dahil olsun.
        /// </summary>
        private static Transform BuildRoomShell(SceneBuildContext ctx, Transform zone, string name, bool east, float zMin, float zMax,
            bool withFloor = true, Vector2[] northCrawls = null)
        {
            Material floor = ctx.Mat("Floor", new Color(0.22f, 0.2f, 0.19f));
            Material stone = ctx.Mat("Stone", new Color(0.27f, 0.26f, 0.29f));
            Transform room = new GameObject(name).transform;
            room.SetParent(zone, false);

            float side = east ? 1f : -1f;
            float inner = 8f * side;
            float outer = (8f + RoomDepth) * side;
            float zCenter = (zMin + zMax) * 0.5f;
            float wallY = EnvironmentSection.WallHeight * 0.5f;

            if (withFloor)
            {
                ctx.Box("Floor", room, new Vector3((inner + outer + 0.5f * side) * 0.5f, -0.25f, zCenter), new Vector3(RoomDepth + 0.5f, 0.5f, zMax - zMin + 0.5f), floor);
            }

            ctx.Box("Wall_Outer", room, new Vector3(outer + 0.25f * side, wallY, zCenter), new Vector3(0.5f, EnvironmentSection.WallHeight, zMax - zMin + 0.5f), stone);
            float sideWallX = (inner + 0.5f * side + outer + 0.5f * side) * 0.5f;
            ctx.Box("Wall_South", room, new Vector3(sideWallX, wallY, zMin), new Vector3(RoomDepth, EnvironmentSection.WallHeight, 0.5f), stone);
            if (northCrawls == null)
            {
                ctx.Box("Wall_North", room, new Vector3(sideWallX, wallY, zMax), new Vector3(RoomDepth, EnvironmentSection.WallHeight, 0.5f), stone);
                return room;
            }

            // Kuzey duvarında yalnızca Lumi'nin sığdığı alçak geçitler (x aralıkları, doğu odası için).
            float x = inner + 0.5f * side;
            float wallEnd = outer + 0.5f * side;
            float h = EnvironmentSection.WallHeight;
            foreach (Vector2 crawl in northCrawls)
            {
                ctx.Box("Wall_North", room, new Vector3((x + crawl.x) * 0.5f, wallY, zMax), new Vector3(crawl.x - x, h, 0.5f), stone);
                ctx.Box("CrawlLintel", room, new Vector3((crawl.x + crawl.y) * 0.5f, (CrawlHeight + h) * 0.5f, zMax), new Vector3(crawl.y - crawl.x, h - CrawlHeight, 0.5f), stone);
                x = crawl.y;
            }

            ctx.Box("Wall_North", room, new Vector3((x + wallEnd) * 0.5f, wallY, zMax), new Vector3(wallEnd - x, h, 0.5f), stone);
            return room;
        }

        /// <summary>
        /// A odası (karanlık oda): zemin 5×5 karodan oluşur, karoların bir kısmı yoktur (çukur). Oda zifiri karanlıktır;
        /// kristale giden güvenli karolar yalnızca NavMesh'te vardır. F ile Lumi işarete (CompanionBeacon) gönderilir,
        /// güvenli yoldan gider ve ışığıyla yolu gösterir; oyuncu onu izler. Çukura düşen odanın girişine döner.
        ///
        /// Karo (i, j): i galeriden batıya, j güneyden kuzeye. Güvenli yol: giriş (0,1)-(0,2) → (1,2) → (1,3) → (1,4)
        /// → (2,4) → (3,4) işaret → (3,3) kristal. (2,2)-(2,1)-(3,1) çıkmaz sokak.
        /// </summary>
        private static void BuildDarkRoom(SceneBuildContext ctx, Transform room)
        {
            Material floor = ctx.Mat("Floor", new Color(0.22f, 0.2f, 0.19f));
            var safe = new HashSet<Vector2Int>
            {
                new Vector2Int(0, 1), new Vector2Int(0, 2), new Vector2Int(1, 2), new Vector2Int(1, 3), new Vector2Int(1, 4),
                new Vector2Int(2, 4), new Vector2Int(3, 4), new Vector2Int(3, 3),
                new Vector2Int(2, 2), new Vector2Int(2, 1), new Vector2Int(3, 1),
            };

            for (int i = 0; i < DarkRoomCells; i++)
            {
                for (int j = 0; j < DarkRoomCells; j++)
                {
                    if (safe.Contains(new Vector2Int(i, j)))
                    {
                        ctx.Box($"Tile_{i}_{j}", room, DarkRoomCell(i, j) + Vector3.down * 0.25f, new Vector3(DarkRoomCellSize, 0.5f, DarkRoomCellSize), floor);
                    }
                }
            }

            // Kapı eşiği: duvarın altındaki boşluk.
            ctx.Box("Threshold", room, new Vector3(-8.25f, -0.25f, 35f), new Vector3(0.5f, 0.5f, 2f), floor);

            Transform resetPoint = new GameObject("DarkRoomResetPoint").transform;
            resetPoint.SetParent(room, false);
            resetPoint.localPosition = new Vector3(-7.4f, 0.05f, 35f);

            float roomCenterX = -8.5f - DarkRoomCells * DarkRoomCellSize * 0.5f;
            float roomCenterZ = 33.25f + DarkRoomCells * DarkRoomCellSize * 0.5f;
            float roomSize = DarkRoomCells * DarkRoomCellSize;
            ctx.Box("PitBottom", room, new Vector3(roomCenterX, -3f, roomCenterZ), new Vector3(roomSize, 0.2f, roomSize), ctx.Mat("DarkStone", new Color(0.1f, 0.1f, 0.12f)), false);
            GameObject fall = ctx.Box("FallZone", room, new Vector3(roomCenterX, -1.5f, roomCenterZ), new Vector3(roomSize, 1f, roomSize), null, true, 2);
            fall.GetComponent<Collider>().isTrigger = true;
            fall.AddComponent<FallZone>().Configure(resetPoint);

            var beacon = new GameObject("Beacon");
            beacon.transform.SetParent(room, false);
            beacon.transform.position = DarkRoomCell(3, 4);
            Light beaconLight = ctx.PointLight("BeaconLight", beacon.transform, new Vector3(0f, 1.5f, -0.6f), new Color(0.55f, 0.85f, 1f), 2f, 0.15f);
            beacon.AddComponent<CompanionBeacon>().Configure(beacon.transform, beaconLight);
        }

        /// <summary>Karanlık odadaki (i, j) karosunun zemin hizasındaki merkezi.</summary>
        private static Vector3 DarkRoomCell(int i, int j)
        {
            return new Vector3(-8.5f - DarkRoomCellSize * (i + 0.5f), 0f, 33.25f + DarkRoomCellSize * (j + 0.5f));
        }

        /// <summary>
        /// B odası (sütun labirenti): iki iç duvar yılan biçimli bir yol çizer; duvarların uçlarındaki geçitler
        /// kapılarla kapalıdır. Her kapı, kuzey duvardaki alçak geçidin (1.1 m, yalnızca Lumi sığar) ardındaki
        /// nişte bir şalterle açılır. F ile Lumi en yakın kapalı kapının şalterine gider.
        ///   Kapı 1: batı koridorundan orta koridora (kalıcı), şalteri batı nişinde.
        ///   Kapı 2: orta koridordan doğu koridoruna, 15 sn açık kalır; şalteri orta koridorun nişinde.
        /// Kristal doğu koridorunun kuzey ucunda.
        /// </summary>
        private static void BuildMaze(SceneBuildContext ctx, Transform room)
        {
            Material stone = ctx.Mat("PillarStone", new Color(0.33f, 0.31f, 0.3f));
            Material floor = ctx.Mat("Floor", new Color(0.22f, 0.2f, 0.19f));
            Material gateMat = ctx.Mat("GateDoor", new Color(0.2f, 0.22f, 0.26f), null, 0.6f, 0.4f);
            float h = EnvironmentSection.WallHeight;

            ctx.Box("MazeWall_1", room, new Vector3(10.6f, h * 0.5f, 28.125f), new Vector3(0.3f, h, 3.75f), stone);
            ctx.Box("MazeWall_2", room, new Vector3(12.4f, h * 0.5f, 29.875f), new Vector3(0.3f, h, 3.75f), stone);
            ctx.Box("MazePillar", room, new Vector3(13.5f, h * 0.5f, 26.8f), new Vector3(0.5f, h, 0.5f), stone);
            ctx.Torch("Torch_Maze", room, new Vector3(13.8f, 2.4f, 28.5f));

            MazeGate gate1 = BuildMazeGate(ctx, room, "MazeGate_1", new Vector3(10.6f, 0f, 30.875f), 0f, gateMat);
            MazeGate gate2 = BuildMazeGate(ctx, room, "MazeGate_2", new Vector3(12.4f, 0f, 27.125f), 15f, gateMat);

            // Nişler: B'nin kuzey duvarı ile C'nin güney duvarı arasındaki 1 m'lik şerit, ara duvarla ikiye bölünür.
            ctx.Box("NicheFloor", room, new Vector3(10.4f, -0.25f, 32.75f), new Vector3(3.8f, 0.5f, 1f), floor);
            ctx.Box("NicheDivider", room, new Vector3(10.45f, h * 0.5f, 32.75f), new Vector3(0.3f, h, 1f), stone);
            ctx.Box("NicheEnd", room, new Vector3(12.45f, h * 0.5f, 32.75f), new Vector3(0.3f, h, 1f), stone);

            BuildCompanionLever(ctx, room, "MazeLever_1", new Vector3(9.5f, 0f, 32.75f), gate1);
            BuildCompanionLever(ctx, room, "MazeLever_2", new Vector3(11.5f, 0f, 32.75f), gate2);
        }

        private static MazeGate BuildMazeGate(SceneBuildContext ctx, Transform room, string name, Vector3 position, float openSeconds, Material material)
        {
            float h = EnvironmentSection.WallHeight;
            Transform root = new GameObject(name).transform;
            root.SetParent(room, false);
            root.localPosition = position;
            GameObject door = ctx.Box("Door", root, new Vector3(0f, h * 0.5f, 0f), new Vector3(0.25f, h, 1.75f), material, true, 2);
            NavMeshObstacle obstacle = door.AddComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.carving = true;

            MazeGate gate = root.gameObject.AddComponent<MazeGate>();
            gate.Configure(door.transform, openSeconds);
            return gate;
        }

        private static void BuildCompanionLever(SceneBuildContext ctx, Transform room, string name, Vector3 approach, MazeGate gate)
        {
            Transform root = new GameObject(name).transform;
            root.SetParent(room, false);
            root.localPosition = approach;
            ctx.Box("Base", root, new Vector3(0f, 0.3f, 0.35f), new Vector3(0.5f, 0.6f, 0.2f), ctx.Mat("Metal", new Color(0.32f, 0.32f, 0.36f), null, 0.8f, 0.45f), false);
            Light indicator = ctx.PointLight("Indicator", root, new Vector3(0f, 0.9f, 0f), new Color(1f, 0.2f, 0.15f), 2.5f, 1.2f);
            root.gameObject.AddComponent<CompanionLever>().Configure(root, gate, indicator);
        }

        /// <summary>
        /// C odası: kapı ile kristal arasında 2×3 diken tuzağı (tuzak odasıyla aynı sınıflar).
        /// Tuzaklar odanın tüm genişliğini kapladığından içlerinden geçmek gerekir; yakalanan oyuncu
        /// ya da Lumi oda kapısına geri döner. Bu alan tuzak odasınınkinden ayrıdır ve ctx'e konmaz.
        /// </summary>
        private static void BuildSpikeRoom(SceneBuildContext ctx, Transform room)
        {
            Transform resetPoint = new GameObject("SpikeRoomResetPoint").transform;
            resetPoint.SetParent(room, false);
            resetPoint.localPosition = new Vector3(9f, 0f, 36f);

            var traps = new List<SpikeTrap>();
            foreach (float x in new[] { 10.4f, 12f })
            {
                foreach (float z in new[] { 35f, 36.6f, 38.2f })
                {
                    traps.Add(EnvironmentSection.BuildSpikeTrap(ctx, room, new Vector3(x, 0f, z)));
                }
            }

            room.gameObject.AddComponent<SpikeTrapField>().Configure(traps.ToArray(), resetPoint);
            ctx.Torch("Torch_Spikes", room, new Vector3(13.8f, 2.4f, 34.3f));
        }

        /// <summary>Sütunlar ve alçak duvarlar. Aralıklar oyuncu ve Lumi için en az 2 m.</summary>
        private static void BuildObstacles(SceneBuildContext ctx, Transform zone)
        {
            Material stone = ctx.Mat("Stone", new Color(0.27f, 0.26f, 0.29f));
            Material pillarMat = ctx.Mat("PillarStone", new Color(0.33f, 0.31f, 0.3f));
            Transform obstacles = new GameObject("Obstacles").transform;
            obstacles.SetParent(zone, false);

            // İlk sütun Obstacle_Prefab olarak kaydedilir, diğerleri bu prefab'dan üretilir.
            GameObject pillarPrefab = null;
            foreach (Vector2 p in new[] { new Vector2(-3f, 29f), new Vector2(3f, 29f), new Vector2(-3f, 35f), new Vector2(3f, 35f) })
            {
                var position = new Vector3(p.x, EnvironmentSection.WallHeight * 0.5f, p.y);
                if (pillarPrefab == null)
                {
                    GameObject first = ctx.Box("Pillar", obstacles, position, new Vector3(1f, EnvironmentSection.WallHeight, 1f), pillarMat);
                    pillarPrefab = ctx.SavePrefab(first, "Obstacle_Prefab");
                    continue;
                }

                var pillar = (GameObject)PrefabUtility.InstantiatePrefab(pillarPrefab, obstacles);
                pillar.transform.localPosition = position;
            }

            ctx.Box("LowWall_Left", obstacles, new Vector3(-5.5f, 0.6f, 32f), new Vector3(4f, 1.2f, 0.5f), stone);
            // Sağdaki alçak duvarlar oda kapılarının önünde 2 m boşluk bırakacak kadar kısa.
            ctx.Box("LowWall_Right", obstacles, new Vector3(4.5f, 0.6f, 27.5f), new Vector3(3f, 1.2f, 0.5f), stone);
            ctx.Box("LowWall_Back", obstacles, new Vector3(4.25f, 0.6f, 37.5f), new Vector3(3.5f, 1.2f, 0.5f), stone);
        }

        /// <summary>
        /// Kristaller karanlık köşelerde, alçak duvarların arkasında; kendi ışıklarıyla bulunur.
        /// Sayaç tamamlanınca kapıyı açar (şalter–köprü ilişkisiyle aynı yapı).
        /// </summary>
        private static void BuildCrystals(SceneBuildContext ctx, Transform zone, CrystalGate gate)
        {
            Transform crystalsRoot = new GameObject("Crystals").transform;
            crystalsRoot.SetParent(zone, false);

            var crystals = new List<CrystalPickup>
            {
                // A: Karanlık oda. Işığı neredeyse yok; ancak Lumi'nin ışığıyla görülür (G ile Lumi'yi çağır).
                BuildCrystal(ctx, crystalsRoot, DarkRoomCell(3, 3), dark: true),
            };

            // B: Sütun labirentinin en uç köşesi.
            crystals.Add(BuildCrystal(ctx, crystalsRoot, new Vector3(13.3f, 0f, 30.8f)));

            // C: Diken odasında, tuzak şeridinin arkasında.
            crystals.Add(BuildCrystal(ctx, crystalsRoot, new Vector3(13.2f, 0f, 38.2f)));

            CrystalCounter counter = crystalsRoot.gameObject.AddComponent<CrystalCounter>();
            counter.Configure(crystals.ToArray(), crystals.Count, new MonoBehaviour[] { gate });
            ctx.Set(counter);
        }

        private static CrystalPickup BuildCrystal(SceneBuildContext ctx, Transform parent, Vector3 position, bool dark = false)
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
            // Karanlık odadaki kristal kendi kendine parlamaz; ışık vurunca görünür.
            Material crystalMat = dark
                ? ctx.Mat("CrystalDim", new Color(0.45f, 0.85f, 1f), new Color(0.02f, 0.05f, 0.07f), 0.1f, 0.9f)
                : ctx.Mat("Crystal", new Color(0.45f, 0.85f, 1f), new Color(0.6f, 1.6f, 2.2f), 0.1f, 0.9f);
            GameObject shard = ctx.Primitive(PrimitiveType.Cube, "Shard", visual, Vector3.zero, new Vector3(0.3f, 0.55f, 0.3f), crystalMat);
            shard.transform.localRotation = Quaternion.Euler(45f, 0f, 45f);

            ctx.Box("Pedestal", root.transform, new Vector3(0f, 0.2f, 0f), new Vector3(0.6f, 0.4f, 0.6f), ctx.Mat("DarkStone", new Color(0.1f, 0.1f, 0.12f)), false);

            Light glow = ctx.PointLight("Glow", root.transform, new Vector3(0f, 1f, 0f), new Color(0.45f, 0.85f, 1f), dark ? 1.5f : 4f, dark ? 0.3f : 1.2f);

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
        private static CrystalGate BuildGate(SceneBuildContext ctx, Transform zone)
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
            var sockets = new List<Renderer>();
            for (int i = -1; i <= 1; i++)
            {
                GameObject socket = ctx.Primitive(PrimitiveType.Sphere, "Socket", door.transform, new Vector3(i * 0.25f, 0.2f, -0.55f), new Vector3(0.12f, 0.08f, 0.3f), socketMat);
                sockets.Add(socket.GetComponent<Renderer>());
            }

            NavMeshObstacle obstacle = door.AddComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.carving = true;

            Material litSocketMat = ctx.Mat("CrystalSocketLit", new Color(0.45f, 0.85f, 1f), new Color(0.6f, 1.6f, 2.2f), 0.1f, 0.9f);
            Light gateLight = ctx.PointLight("GateLight", gate, new Vector3(0f, 2.2f, DoorZ - 1f), new Color(0.45f, 0.85f, 1f), 6f, 1.6f);

            CrystalGate crystalGate = gate.gameObject.AddComponent<CrystalGate>();
            crystalGate.Configure(door.transform, sockets.ToArray(), litSocketMat, gateLight);
            ctx.Set(crystalGate);
            return crystalGate;
        }

        /// <summary>Galeri kasıtlı olarak karanlık: az sayıda meşale, bazıları sönük. Asıl ışık Lumi ve kristallerden gelir.</summary>
        private static void BuildLighting(SceneBuildContext ctx, Transform zone)
        {
            Transform lights = new GameObject("Lights").transform;
            lights.SetParent(zone, false);
            ctx.Torch("Torch_Gallery_A", lights, new Vector3(7.8f, 2.4f, 29.5f));
            ctx.Torch("Torch_Gallery_Dead_A", lights, new Vector3(-7.8f, 2.4f, 31.5f), false);
            ctx.Torch("Torch_Gallery_Dead_B", lights, new Vector3(7.8f, 2.4f, 33.8f), false);
            ctx.Torch("Torch_Gate_L", lights, new Vector3(-2.2f, 2.6f, DoorZ - 0.45f));
            ctx.Torch("Torch_Gate_R", lights, new Vector3(2.2f, 2.6f, DoorZ - 0.45f));
        }
    }
}
