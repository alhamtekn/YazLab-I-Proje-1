using Lumen.Companion;
using Lumen.Core;
using Lumen.Level;
using Lumen.Player;
using Lumen.Sanctum;
using UnityEngine;
using UnityEngine.AI;

namespace Lumen.EditorTools.SceneBuilding.Sections
{
    /// <summary>
    /// Geliştirici 2 — kutsal bölüm: Arven'in kapısından sonra, tapınağın kalbinden önce Lumi ile birlikte
    /// geçilen iki sınav. Tapınağın kalbi (NpcSection) bu bölümün sonundadır.
    ///
    /// Yerleşim (X sağa, Z ileri, birim metre):
    ///   Giriş        z=58.25 duvarındaki x[-1.5, 1.5] geçidi (EnvironmentSection)
    ///   1. İkiz Mühür  z[58.5, 74]   oyuncu mührü (-5, 64); Lumi mührü doğu duvarındaki alçak girintide (9.9, 63.5);
    ///                                 z=70'te ışık bariyeri
    ///   2. Işığın Yolu z[74, 86]     uçurum; üstünde 1.5 m'lik görünmez taşlardan dolambaçlı yol; fener (1.5, 87.5)
    ///   Kalp odası     z[86, 98]     tapınağın kalbi z=95
    /// </summary>
    public sealed class SanctumSection : ISceneSection
    {
        public const float HeartZ = 95f;
        public const float BarrierZ = 70f;
        public const float ChasmStartZ = 74f;
        public const float ChasmEndZ = 86f;
        private const float StartZ = 58.5f;
        private const float EndZ = 98f;
        private const float CrawlHeight = 1.1f;   // Lumi (0.9 m) sığar, oyuncu (1.8 m) sığmaz
        private const float TileSize = 1.5f;

        public static readonly Vector3 PlayerSealPosition = new Vector3(-5f, 0f, 64f);
        public static readonly Vector3 LumiSealPosition = new Vector3(9.9f, 0f, 63.5f);
        public static readonly Vector3 BeaconPosition = new Vector3(1.5f, 0f, 87.5f);

        // Görünmez yolun taşları: (x, sıra). Sıra k'nın merkezi z = 74.75 + 1.5k. Ardışık taşlar kenardan bitişik.
        private static readonly Vector2[] PathCells =
        {
            new Vector2(0f, 0), new Vector2(0f, 1), new Vector2(-1.5f, 1), new Vector2(-3f, 1), new Vector2(-3f, 2),
            new Vector2(-3f, 3), new Vector2(-1.5f, 3), new Vector2(0f, 3), new Vector2(1.5f, 3), new Vector2(3f, 3),
            new Vector2(3f, 4), new Vector2(3f, 5), new Vector2(1.5f, 5), new Vector2(1.5f, 6), new Vector2(1.5f, 7)
        };

        public int Order => 37;

        public void Build(SceneBuildContext ctx)
        {
            Transform zone = new GameObject("Sanctum").transform;
            zone.SetParent(ctx.Root("Level"), false);
            Transform player = ctx.Get<PlayerController>().transform;
            HudView hud = ctx.Get<HudView>();

            BuildShell(ctx, zone);
            TwinSeal twinSeal = BuildTwinSeal(ctx, zone, player, hud);
            BuildLightPath(ctx, zone, hud, twinSeal);
            BuildDecor(ctx, zone);
        }

        private static void BuildShell(SceneBuildContext ctx, Transform zone)
        {
            Material floor = ctx.Mat("Floor", new Color(0.22f, 0.2f, 0.19f));
            Material stone = ctx.Mat("Stone", new Color(0.27f, 0.26f, 0.29f));
            float wallY = (EnvironmentSection.WallHeight + EnvironmentSection.PitDepth) * 0.5f;
            float wallH = EnvironmentSection.WallHeight - EnvironmentSection.PitDepth;

            // Zemin, giriş duvarının (z 58-58.5) altından başlar: geçidin altı boş kalırsa NavMesh kopar, Lumi geçemez
            float floorStart = StartZ - 0.5f;
            ctx.Box("Floor_Seal", zone, new Vector3(0f, -0.25f, (floorStart + ChasmStartZ) * 0.5f), new Vector3(16f, 0.5f, ChasmStartZ - floorStart), floor);
            ctx.Box("Floor_Heart", zone, new Vector3(0f, -0.25f, (ChasmEndZ + EndZ) * 0.5f), new Vector3(16f, 0.5f, EndZ - ChasmEndZ), floor);
            BuildLava(ctx, zone);

            // Batı duvarı tek parça; doğu duvarında Lumi girintisine açılan alçak geçit z[62.5, 64.5]
            ctx.Box("Wall_West", zone, new Vector3(-8.25f, wallY, (StartZ + EndZ) * 0.5f), new Vector3(0.5f, wallH, EndZ - StartZ), stone);
            ctx.Box("Wall_East_0", zone, new Vector3(8.25f, wallY, (StartZ + 62.5f) * 0.5f), new Vector3(0.5f, wallH, 62.5f - StartZ), stone);
            ctx.Box("Wall_East_1", zone, new Vector3(8.25f, wallY, (64.5f + EndZ) * 0.5f), new Vector3(0.5f, wallH, EndZ - 64.5f), stone);
            ctx.Box("Wall_East_Crawl", zone, new Vector3(8.25f, (CrawlHeight + EnvironmentSection.WallHeight) * 0.5f, 63.5f), new Vector3(0.5f, EnvironmentSection.WallHeight - CrawlHeight, 2f), stone);
            ctx.Box("Wall_End", zone, new Vector3(0f, wallY, EndZ + 0.25f), new Vector3(17f, wallH, 0.5f), stone);

            // Lumi girintisi: 2.5 m derin, 2 m geniş küçük oda
            ctx.Box("Alcove_Floor", zone, new Vector3(9.75f, -0.25f, 63.5f), new Vector3(3.5f, 0.5f, 2.5f), floor);
            ctx.Box("Alcove_Back", zone, new Vector3(11.25f, 2f, 63.5f), new Vector3(0.5f, 4f, 3f), stone);
            ctx.Box("Alcove_South", zone, new Vector3(9.75f, 2f, 62.25f), new Vector3(3.5f, 4f, 0.5f), stone);
            ctx.Box("Alcove_North", zone, new Vector3(9.75f, 2f, 64.75f), new Vector3(3.5f, 4f, 0.5f), stone);

            BuildCeiling(ctx, zone);

            // Uçuruma düşen kontrol noktasına döner
            GameObject killZone = ctx.Box("KillZone", zone, new Vector3(0f, -4f, (ChasmStartZ + ChasmEndZ) * 0.5f), new Vector3(16f, 1f, ChasmEndZ - ChasmStartZ), null, true, 2);
            killZone.GetComponent<Collider>().isTrigger = true;
            killZone.AddComponent<KillZone>();
        }

        // Uçurumun dibi: parlayan lav ve aşağıdan duvarları aydınlatan turuncu ışıklar (uçurum boş görünmesin).
        // Malzeme adı "Lava": ilk uçurumda da aynı malzeme kullanılır, doku verilirse ikisi birden değişir.
        private static void BuildLava(SceneBuildContext ctx, Transform zone)
        {
            Material lava = ctx.Mat("Lava", new Color(1f, 0.35f, 0.05f), new Color(2.6f, 0.7f, 0.08f), 0f, 0.6f);
            float zCenter = (ChasmStartZ + ChasmEndZ) * 0.5f;
            ctx.Box("Lava", zone, new Vector3(0f, EnvironmentSection.PitDepth, zCenter), new Vector3(16f, 0.5f, ChasmEndZ - ChasmStartZ), lava, false, 2);
            for (float z = ChasmStartZ + 2f; z < ChasmEndZ; z += 4f)
            {
                ctx.PointLight("LavaGlow", zone, new Vector3(0f, EnvironmentSection.PitDepth + 2f, z), new Color(1f, 0.45f, 0.1f), 10f, 1.6f);
            }
        }

        // Tavan: y = 4 üstünde, "Ceiling" katmanında ve çarpıştırıcısız. Bu katman NavMesh'e girmez, mini harita onu çizmez.
        private static void BuildCeiling(SceneBuildContext ctx, Transform zone)
        {
            Material stone = ctx.Mat("Stone", new Color(0.27f, 0.26f, 0.29f));
            int layer = GameLayers.CeilingIndex;
            float y = EnvironmentSection.WallHeight + 0.25f;
            ctx.Box("Ceiling", zone, new Vector3(0f, y, (StartZ - 0.5f + EndZ + 0.5f) * 0.5f), new Vector3(17f, 0.5f, EndZ - StartZ + 1f), stone, false, layer);
            ctx.Box("Ceiling_Alcove", zone, new Vector3(9.75f, y, 63.5f), new Vector3(3.5f, 0.5f, 3f), stone, false, layer);
        }

        private static TwinSeal BuildTwinSeal(SceneBuildContext ctx, Transform zone, Transform player, HudView hud)
        {
            Material seal = ctx.Mat("Seal", new Color(0.85f, 0.7f, 0.4f), new Color(0.9f, 0.65f, 0.25f), 0.6f, 0.6f);
            Material energy = ctx.Mat("EnergyBarrier", new Color(0.3f, 0.6f, 1f), new Color(0.6f, 1.4f, 2.6f), 0f, 0.9f);
            Transform root = new GameObject("TwinSeal").transform;
            root.SetParent(zone, false);

            Transform playerPlate = ctx.Primitive(PrimitiveType.Cylinder, "PlayerSeal", root, PlayerSealPosition + Vector3.up * 0.03f, new Vector3(2.4f, 0.03f, 2.4f), seal).transform;
            Light playerLight = ctx.PointLight("PlayerSealLight", root, PlayerSealPosition + Vector3.up * 1f, new Color(1f, 0.8f, 0.45f), 5f, 0.3f);
            Transform lumiPlate = ctx.Primitive(PrimitiveType.Cylinder, "LumiSeal", root, LumiSealPosition + Vector3.up * 0.03f, new Vector3(1.2f, 0.03f, 1.2f), seal).transform;
            Light lumiLight = ctx.PointLight("LumiSealLight", root, LumiSealPosition + Vector3.up * 0.8f, new Color(0.6f, 0.85f, 1f), 4f, 0.3f);

            // Işık bariyeri: NavMesh'e dahil değil (katman 2), kapalıyken NavMeshObstacle Lumi'nin geçişini keser
            GameObject barrier = ctx.Box("Barrier", root, new Vector3(0f, EnvironmentSection.WallHeight * 0.5f, BarrierZ), new Vector3(16f, EnvironmentSection.WallHeight, 0.3f), energy, true, 2);
            NavMeshObstacle obstacle = barrier.AddComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.size = Vector3.one;
            obstacle.carving = true;
            ctx.PointLight("BarrierGlow", root, new Vector3(0f, 2f, BarrierZ - 1f), new Color(0.5f, 0.75f, 1f), 9f, 1.2f);

            TwinSeal twinSeal = root.gameObject.AddComponent<TwinSeal>();
            twinSeal.Configure(player, playerPlate, lumiPlate, playerLight, lumiLight, barrier, StartZ);

            Hint(ctx, zone, hud, 60.5f, "İkiz Mühür: Bariyer, iki mühür aynı anda yanınca açılır.\nAltın mühre bas ve Lumi'den yardım iste (F). Onun mührüne sen sığmazsın.");
            return twinSeal;
        }

        private static void BuildLightPath(SceneBuildContext ctx, Transform zone, HudView hud, TwinSeal twinSeal)
        {
            Material tileMat = ctx.Mat("LightTile", new Color(0.5f, 0.7f, 0.9f), new Color(0.6f, 0.85f, 1.6f), 0.2f, 0.8f);
            Transform root = new GameObject("LightPath").transform;
            root.SetParent(zone, false);

            var tiles = new Renderer[PathCells.Length];
            for (int i = 0; i < PathCells.Length; i++)
            {
                Vector3 center = new Vector3(PathCells[i].x, -0.15f, ChasmStartZ + TileSize * (PathCells[i].y + 0.5f));
                tiles[i] = ctx.Box("Tile", root, center, new Vector3(TileSize, 0.3f, TileSize), tileMat).GetComponent<Renderer>();
            }

            // Fener: Lumi'nin hedefi, uçurumun karşısında
            var beacon = new GameObject("Beacon").transform;
            beacon.SetParent(root, false);
            beacon.localPosition = BeaconPosition;
            ctx.Primitive(PrimitiveType.Cylinder, "Pillar", beacon, new Vector3(0.9f, 0.6f, 0.6f), new Vector3(0.4f, 0.6f, 0.4f), ctx.Mat("Stone", new Color(0.27f, 0.26f, 0.29f)));
            ctx.Primitive(PrimitiveType.Sphere, "Crystal", beacon, new Vector3(0.9f, 1.4f, 0.6f), Vector3.one * 0.35f, tileMat);
            Light beaconLight = ctx.PointLight("BeaconLight", beacon, new Vector3(0.9f, 1.6f, 0.6f), new Color(0.6f, 0.85f, 1f), 10f, 0.4f);

            root.gameObject.AddComponent<LightPath>().Configure(ctx.Get<CompanionController>().transform, twinSeal, tiles, beacon, beaconLight);

            // Uçurumun başında kontrol noktası: düşen oyuncu buraya döner
            GameObject checkpoint = ctx.Box("Checkpoint", root, new Vector3(0f, 0.05f, ChasmStartZ - 1.5f), new Vector3(16f, 2f, 1f), null, true, 2);
            checkpoint.GetComponent<Collider>().isTrigger = true;
            checkpoint.AddComponent<Checkpoint>();

            Hint(ctx, zone, hud, ChasmStartZ - 2.5f, "Işığın Yolu: Taşlar yalnızca Lumi'nin ışığında görünür.\nLumi'yi karşıdaki fenere gönder (F) ve aydınlattığı taşlardan yürü.");
        }

        private static void BuildDecor(SceneBuildContext ctx, Transform zone)
        {
            ctx.Torch("Torch_Seal_L", zone, new Vector3(-7.8f, 2.4f, 61f));
            ctx.Torch("Torch_Seal_R", zone, new Vector3(7.8f, 2.4f, 67f));
            ctx.Torch("Torch_Heart_L", zone, new Vector3(-7.8f, 2.4f, 92f));
            ctx.Torch("Torch_Heart_R", zone, new Vector3(7.8f, 2.4f, 92f));
        }

        // Bölge girişinde bir kez gösterilen ipucu
        private static void Hint(SceneBuildContext ctx, Transform zone, HudView hud, float z, string message)
        {
            GameObject trigger = ctx.Box("Hint", zone, new Vector3(0f, 1f, z), new Vector3(16f, 2f, 1f), null, true, 2);
            trigger.GetComponent<Collider>().isTrigger = true;
            trigger.AddComponent<ZoneHint>().Configure(hud, message);
        }
    }
}
