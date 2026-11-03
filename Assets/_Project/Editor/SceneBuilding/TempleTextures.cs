using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Lumen.EditorTools.SceneBuilding
{
    /// <summary>
    /// Tapınak teması: "tarihi, kutsal güç ve teknolojiyle korunan bir tapınak". Dokular kodla üretilir
    /// (Perlin gürültüsü + çizgiler → PNG) ve malzeme adına göre SceneBuildContext.Mat tarafından uygulanır.
    /// Böylece bölüm sınıflarında hiçbir çağrı değişmeden tüm sahne aynı temaya girer.
    /// Taş ve zemin altın rün / mavi devre izleri taşır; ışıyan katmanlar bilerek sönüktür.
    /// </summary>
    public static class TempleTextures
    {
        public const string Folder = "Assets/_Project/Textures/Temple";
        private const int Size = 256;

        public static readonly Color StoneColor = new Color(0.42f, 0.39f, 0.35f);
        public static readonly Color Gold = new Color(0.9f, 0.7f, 0.35f);
        public static readonly Color Blue = new Color(0.4f, 0.75f, 1f);

        /// <summary>Bir malzemenin tema ayarları. Tiling metrede bir tekrardır.</summary>
        public sealed class Theme
        {
            public Func<Texture2D> Main;
            public Func<Texture2D> Emission;
            public Color Tint = Color.white;
            public Color EmissionColor = Color.black;
            public float Tiling = 2f;
            public bool ObjectSpace;
        }

        private static readonly Dictionary<string, Theme> Themes = new Dictionary<string, Theme>
        {
            ["Stone"] = new Theme { Main = () => Get("stone_blocks", StoneBlocks), Emission = () => Get("stone_runes", StoneRunes), EmissionColor = Gold * 0.9f, Tiling = 2.5f },
            ["PillarStone"] = new Theme { Main = () => Get("stone_blocks", StoneBlocks), Emission = () => Get("stone_runes", StoneRunes), Tint = new Color(1.05f, 1.02f, 1f), EmissionColor = Gold * 0.7f, Tiling = 2f },
            ["CeilingStone"] = new Theme { Main = () => Get("stone_blocks", StoneBlocks), Emission = () => Get("stone_runes", StoneRunes), Tint = new Color(0.55f, 0.53f, 0.52f), EmissionColor = Gold * 0.35f, Tiling = 3f },
            ["Floor"] = new Theme { Main = () => Get("basalt_tiles", BasaltTiles), Emission = () => Get("floor_circuits", FloorCircuits), EmissionColor = Blue * 0.45f, Tiling = 2f },
            ["Metal"] = new Theme { Main = () => Get("metal_panels", MetalPanels), Emission = () => Get("metal_scanlines", MetalScanlines), EmissionColor = Blue * 0.5f, Tiling = 1.5f },
            ["GateDoor"] = new Theme { Main = () => Get("metal_panels", MetalPanels), Emission = () => Get("metal_scanlines", MetalScanlines), EmissionColor = Blue * 0.9f, Tiling = 1.5f },
            ["ArvenDoor"] = new Theme { Main = () => Get("metal_panels", MetalPanels), Emission = () => Get("metal_scanlines", MetalScanlines), Tint = new Color(1.1f, 0.95f, 0.8f), EmissionColor = Blue * 0.9f, Tiling = 1.5f },
            // Köprü kalkıp indiği için doku nesneyle birlikte döner; kalaslar köprünün boyunca 0.5 m aralıklı.
            ["Lava"] = new Theme { Main = () => Get("lava_crust", LavaCrust), Emission = () => Get("lava_glow", LavaGlowMap), Tint = new Color(1f, 0.5f, 0.2f), EmissionColor = new Color(3f, 0.9f, 0.15f), Tiling = 4f },
            ["BridgeWood"] = new Theme { Main = () => Get("bridge_planks", BridgePlanks), Tiling = 1f, ObjectSpace = true },
            // Hareket eden ya da tek parça nesneler nesne uzayında, nesne başına bir kez döşenir (Tiling = 1).
            ["LumiGlow"] = new Theme { Main = () => Get("lumi_panels", LumiPanels), Emission = () => Get("lumi_glow", LumiGlow), EmissionColor = new Color(1.2f, 1.35f, 1.6f), Tiling = 0.5f, ObjectSpace = true },
            ["LumiEye"] = new Theme { Emission = () => Get("lumi_eye_strip", LumiEyeStrip), Tint = new Color(0.05f, 0.08f, 0.12f), EmissionColor = Blue * 2.2f, Tiling = 1f, ObjectSpace = true },
            ["ArvenStone"] = new Theme { Main = () => Get("statue_stone", StatueStone), Emission = () => Get("chest_rune", ChestRune), EmissionColor = Gold * 0.7f, Tiling = 1f, ObjectSpace = true },
            ["Seal"] = new Theme { Main = () => Get("seal_brass", SealBrass), Emission = () => Get("seal_rings", SealRings), EmissionColor = Gold * 1.2f, Tiling = 1f, ObjectSpace = true },
            ["EnergyBarrier"] = new Theme { Main = () => Get("metal_panels", MetalPanels), Emission = () => Get("metal_scanlines", MetalScanlines), EmissionColor = Blue * 1.4f, Tiling = 1f },
        };

        /// <summary>Malzeme adına ait tema varsa döndürür.</summary>
        public static bool TryGet(string materialName, out Theme theme) => Themes.TryGetValue(materialName, out theme);

        /// <summary>
        /// PNG'yi üretir (içerik değiştiyse yazar) ve içe aktarma ayarlarını her seferinde açıkça kurar.
        /// Ayarlar her seferinde yazıldığı için boyut değişince .meta'da eski sprite/kesim ayarı kalmaz.
        /// </summary>
        private static Texture2D Get(string name, Func<Color[]> generate)
        {
            SceneBuildContext.EnsureFolder(Folder);
            string path = $"{Folder}/{name}.png";

            var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            texture.SetPixels(generate());
            byte[] png = texture.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(texture);

            string fullPath = Path.GetFullPath(path);
            if (!File.Exists(fullPath) || !File.ReadAllBytes(fullPath).AsSpan().SequenceEqual(png))
            {
                File.WriteAllBytes(fullPath, png);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            }

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer.textureType != TextureImporterType.Default || importer.wrapMode != TextureWrapMode.Repeat ||
                importer.maxTextureSize != Size || !importer.mipmapEnabled)
            {
                importer.textureType = TextureImporterType.Default;
                importer.spriteImportMode = SpriteImportMode.None;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.mipmapEnabled = true;
                importer.maxTextureSize = Size;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        // ---------- Üreticiler ----------

        /// <summary>Yaşlı, çatlak kum-gri taş bloklar (şaşırtmalı örgü).</summary>
        private static Color[] StoneBlocks()
        {
            var pixels = new Color[Size * Size];
            var random = new System.Random(11);
            const int rowHeight = 64;
            const int blockWidth = 128;
            const int mortar = 3;
            float[] blockShade = RandomShades(random, 16, 0.82f, 1.12f);

            for (int y = 0; y < Size; y++)
            {
                int row = y / rowHeight;
                int offset = row % 2 == 0 ? 0 : blockWidth / 2;
                for (int x = 0; x < Size; x++)
                {
                    int bx = (x + offset) % Size;
                    int block = row * 2 + bx / blockWidth;
                    bool isMortar = y % rowHeight < mortar || bx % blockWidth < mortar;
                    float grain = 0.85f + 0.3f * TileNoise(x, y, 18f, 1) + 0.08f * TileNoise(x, y, 4f, 2);
                    Color c = StoneColor * blockShade[block % blockShade.Length] * grain;
                    pixels[y * Size + x] = isMortar ? StoneColor * 0.45f : c;
                }
            }

            for (int i = 0; i < 7; i++)
            {
                DrawCrack(pixels, random, 0.6f);
            }

            return Opaque(pixels);
        }

        /// <summary>Derzlerde seyrek altın rün çizgileri: kısa çentikli tireler.</summary>
        private static Color[] StoneRunes()
        {
            var pixels = new Color[Size * Size];
            var random = new System.Random(12);
            for (int row = 0; row < Size / 64; row++)
            {
                if (random.NextDouble() < 0.4)
                {
                    continue;
                }

                int y = row * 64 + 1;
                int start = random.Next(0, Size);
                int length = random.Next(24, 56);
                for (int i = 0; i < length; i++)
                {
                    int x = (start + i) % Size;
                    Set(pixels, x, y, Color.white);
                    if (i % 7 == 0)
                    {
                        Set(pixels, x, y - 2, Color.white);
                        Set(pixels, x, y + 2, Color.white);
                    }
                }
            }

            return Opaque(pixels);
        }

        /// <summary>Koyu bazalt karolar (2×2), ince derz.</summary>
        private static Color[] BasaltTiles()
        {
            var pixels = new Color[Size * Size];
            var basalt = new Color(0.13f, 0.13f, 0.15f);
            float[] shades = RandomShades(new System.Random(21), 4, 0.85f, 1.15f);
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    bool grout = x % 128 < 3 || y % 128 < 3;
                    int tile = (y / 128) * 2 + x / 128;
                    float grain = 0.8f + 0.4f * TileNoise(x, y, 10f, 3) + 0.15f * TileNoise(x, y, 3f, 4);
                    pixels[y * Size + x] = grout ? basalt * 0.5f : basalt * shades[tile] * grain;
                }
            }

            return Opaque(pixels);
        }

        /// <summary>Karolarda ince mavi devre hatları: dik açılı izler ve uçlarında düğüm noktaları.</summary>
        private static Color[] FloorCircuits()
        {
            var pixels = new Color[Size * Size];
            var random = new System.Random(22);
            for (int tile = 0; tile < 4; tile++)
            {
                int ox = (tile % 2) * 128;
                int oy = (tile / 2) * 128;
                for (int trace = 0; trace < 3; trace++)
                {
                    int x = ox + 8;
                    int y = oy + random.Next(16, 112);
                    int bend = ox + random.Next(30, 100);
                    int targetY = oy + random.Next(16, 112);
                    for (; x < bend; x++)
                    {
                        Set(pixels, x, y, Color.white);
                    }

                    int step = targetY > y ? 1 : -1;
                    for (; y != targetY; y += step)
                    {
                        Set(pixels, x, y, Color.white);
                    }

                    int end = ox + random.Next(104, 120);
                    for (; x < end; x++)
                    {
                        Set(pixels, x, y, Color.white);
                    }

                    DrawDot(pixels, x, y, 2, Color.white);
                }
            }

            return Opaque(pixels);
        }

        /// <summary>Koyu metal paneller: fırçalanmış yüzey, panel derzleri ve köşe perçinleri.</summary>
        private static Color[] MetalPanels()
        {
            var pixels = new Color[Size * Size];
            var metal = new Color(0.22f, 0.24f, 0.28f);
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    bool seam = x % 128 < 2 || y % 128 < 2;
                    float brushed = 0.9f + 0.2f * TileNoise(x, y * 0.08f, 2f, 5);
                    pixels[y * Size + x] = seam ? metal * 0.4f : metal * brushed;
                }
            }

            for (int px = 0; px < 2; px++)
            {
                for (int py = 0; py < 2; py++)
                {
                    foreach (Vector2Int corner in new[] { new Vector2Int(10, 10), new Vector2Int(118, 10), new Vector2Int(10, 118), new Vector2Int(118, 118) })
                    {
                        DrawDot(pixels, px * 128 + corner.x, py * 128 + corner.y, 3, metal * 1.8f);
                    }
                }
            }

            return Opaque(pixels);
        }

        /// <summary>Mavi tarama çizgileri: aralıklı, kesik yatay hatlar.</summary>
        private static Color[] MetalScanlines()
        {
            var pixels = new Color[Size * Size];
            var random = new System.Random(31);
            for (int y = 16; y < Size; y += 32)
            {
                int x = 0;
                while (x < Size)
                {
                    int length = random.Next(20, 70);
                    int gap = random.Next(8, 30);
                    for (int i = 0; i < length && x + i < Size; i++)
                    {
                        Set(pixels, x + i, y, Color.white);
                        Set(pixels, x + i, y + 1, Color.white * 0.6f);
                    }

                    x += length + gap;
                }
            }

            return Opaque(pixels);
        }

        /// <summary>Lav kabuğu: koyu, soğumuş kayadan adacıklar; aralarında akan lav (ışıma katmanında).</summary>
        private static Color[] LavaCrust()
        {
            var pixels = new Color[Size * Size];
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    float n = TileNoise(x, y, 12f, 61) * 0.7f + TileNoise(x, y, 4f, 62) * 0.3f;
                    pixels[y * Size + x] = n > 0.52f ? new Color(0.12f, 0.07f, 0.05f) * (0.7f + n * 0.5f) : new Color(1f, 0.6f, 0.2f);
                }
            }

            return Opaque(pixels);
        }

        /// <summary>Lavın ışıması: kabuğun olmadığı yerlerde parlak, kabuk kenarlarında sönerek.</summary>
        private static Color[] LavaGlowMap()
        {
            var pixels = new Color[Size * Size];
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    float n = TileNoise(x, y, 12f, 61) * 0.7f + TileNoise(x, y, 4f, 62) * 0.3f;
                    float glow = Mathf.Clamp01((0.56f - n) / 0.12f);
                    pixels[y * Size + x] = Color.white * glow;
                }
            }

            return Opaque(pixels);
        }

        /// <summary>
        /// Eski köprü kalasları: v yönünde 16 kalas (köprünün 8 m'lik boyunda 0.5 m), aralarında koyu boşluk,
        /// u yönünde iki kenarda boydan boya metal kuşak ve perçinler.
        /// </summary>
        private static Color[] BridgePlanks()
        {
            var pixels = new Color[Size * Size];
            var wood = new Color(0.36f, 0.23f, 0.13f);
            var iron = new Color(0.2f, 0.2f, 0.22f);
            float[] plankShade = RandomShades(new System.Random(51), 16, 0.75f, 1.15f);
            const int plank = Size / 16;
            for (int y = 0; y < Size; y++)
            {
                int index = y / plank;
                bool gap = y % plank < 2;
                for (int x = 0; x < Size; x++)
                {
                    bool band = (x > 30 && x < 44) || (x > Size - 44 && x < Size - 30);
                    float grain = 0.8f + 0.25f * TileNoise(x * 0.15f, y * 2.5f, 3f, 52) + 0.1f * TileNoise(x, y, 5f, 53);
                    Color c = gap ? wood * 0.3f : wood * plankShade[index] * grain;
                    if (band)
                    {
                        c = iron * (0.85f + 0.3f * TileNoise(x, y, 4f, 54));
                        if (y % plank == plank / 2 && (x == 37 || x == Size - 37))
                        {
                            c = iron * 2.2f;
                        }
                    }

                    pixels[y * Size + x] = c;
                }
            }

            for (int i = 0; i < 16; i++)
            {
                DrawDot(pixels, 37, i * plank + plank / 2, 2, iron * 2f);
                DrawDot(pixels, Size - 37, i * plank + plank / 2, 2, iron * 2f);
            }

            return Opaque(pixels);
        }

        /// <summary>Lumi'nin gövdesi: beyaz-gümüş, ince panel çizgileri.</summary>
        private static Color[] LumiPanels()
        {
            var pixels = new Color[Size * Size];
            var silver = new Color(0.86f, 0.88f, 0.92f);
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    bool seam = x % 64 < 2 || y % 64 < 2;
                    pixels[y * Size + x] = seam ? silver * 0.55f : silver * (0.95f + 0.08f * TileNoise(x, y, 6f, 6));
                }
            }

            return Opaque(pixels);
        }

        /// <summary>Lumi'nin ışıması: paneller parlar, derzler karanlık kalır.</summary>
        private static Color[] LumiGlow()
        {
            var pixels = new Color[Size * Size];
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    bool seam = x % 64 < 3 || y % 64 < 3;
                    pixels[y * Size + x] = seam ? Color.black : Color.white;
                }
            }

            return Opaque(pixels);
        }

        /// <summary>Lumi'nin gözü: ortada yatay mavi şerit, kenarları yumuşak.</summary>
        private static Color[] LumiEyeStrip()
        {
            var pixels = new Color[Size * Size];
            for (int y = 0; y < Size; y++)
            {
                float band = Mathf.Clamp01(1f - Mathf.Abs(y - Size * 0.5f) / (Size * 0.18f));
                for (int x = 0; x < Size; x++)
                {
                    float edge = Mathf.Clamp01(Mathf.Min(x, Size - 1 - x) / (Size * 0.12f));
                    pixels[y * Size + x] = Color.white * Mathf.SmoothStep(0f, 1f, band) * edge;
                }
            }

            return Opaque(pixels);
        }

        /// <summary>Arven: yıpranmış heykel taşı, koyu lekeler ve aşınma.</summary>
        private static Color[] StatueStone()
        {
            var pixels = new Color[Size * Size];
            var random = new System.Random(41);
            var stone = new Color(0.4f, 0.38f, 0.35f);
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    float wear = 0.75f + 0.35f * TileNoise(x, y, 14f, 7) + 0.1f * TileNoise(x, y, 3f, 8);
                    float stain = TileNoise(x, y, 40f, 9) > 0.62f ? 0.75f : 1f;
                    pixels[y * Size + x] = stone * wear * stain;
                }
            }

            for (int i = 0; i < 10; i++)
            {
                DrawCrack(pixels, random, 0.55f);
            }

            return Opaque(pixels);
        }

        /// <summary>Arven'in göğsündeki sönük altın rün: halka, içinde dikey çizgi ve iki kol.</summary>
        private static Color[] ChestRune()
        {
            var pixels = new Color[Size * Size];
            const int cx = Size / 2;
            const int cy = Size / 2 + 24;
            DrawRing(pixels, cx, cy, 34, 3, 0f, 1f);
            for (int i = -26; i <= 26; i++)
            {
                Set(pixels, cx, cy + i, Color.white);
                Set(pixels, cx + 1, cy + i, Color.white);
            }

            for (int i = 0; i < 18; i++)
            {
                Set(pixels, cx - i, cy + 8 + i / 2, Color.white);
                Set(pixels, cx + i, cy + 8 + i / 2, Color.white);
            }

            return Opaque(pixels);
        }

        /// <summary>Mühür: altın-pirinç, iç içe dairesel oymalar.</summary>
        private static Color[] SealBrass()
        {
            var pixels = new Color[Size * Size];
            Color brass = Gold * 0.85f;
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    float r = Vector2.Distance(new Vector2(x, y), new Vector2(Size * 0.5f, Size * 0.5f));
                    bool groove = Mathf.Abs(r % 30f - 15f) < 1.5f;
                    float shine = 0.85f + 0.2f * TileNoise(x, y, 8f, 10);
                    pixels[y * Size + x] = groove ? brass * 0.45f : brass * shine;
                }
            }

            return Opaque(pixels);
        }

        /// <summary>Mühür üzerindeki halka rünler: iki halka boyunca aralıklı yaylar.</summary>
        private static Color[] SealRings()
        {
            var pixels = new Color[Size * Size];
            for (int segment = 0; segment < 12; segment++)
            {
                float start = segment / 12f;
                DrawRing(pixels, Size / 2, Size / 2, 60, 2, start, start + 0.05f);
                DrawRing(pixels, Size / 2, Size / 2, 105, 2, start + 0.04f, start + 0.07f);
            }

            return Opaque(pixels);
        }

        // ---------- Yardımcılar ----------

        /// <summary>Halka ya da halka yayı çizer; from/to turun kesri (0-1).</summary>
        private static void DrawRing(Color[] pixels, int cx, int cy, int radius, int thickness, float from, float to)
        {
            for (int y = cy - radius - thickness; y <= cy + radius + thickness; y++)
            {
                for (int x = cx - radius - thickness; x <= cx + radius + thickness; x++)
                {
                    float r = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    if (Mathf.Abs(r - radius) > thickness * 0.5f)
                    {
                        continue;
                    }

                    float turn = (Mathf.Atan2(y - cy, x - cx) / (2f * Mathf.PI) + 1f) % 1f;
                    if (turn >= from && turn <= to)
                    {
                        Set(pixels, x, y, Color.white);
                    }
                }
            }
        }

        /// <summary>Kenarları birleşen (tekrarlanabilir) Perlin gürültüsü, 0-1 aralığında.</summary>
        private static float TileNoise(float x, float y, float scale, int seed)
        {
            float u = x / Size;
            float v = y / Size;
            float s = Size / scale;
            float o = seed * 37.13f;
            float a = Mathf.PerlinNoise(o + u * s, o + v * s);
            float b = Mathf.PerlinNoise(o + (u - 1f) * s, o + v * s);
            float c = Mathf.PerlinNoise(o + u * s, o + (v - 1f) * s);
            float d = Mathf.PerlinNoise(o + (u - 1f) * s, o + (v - 1f) * s);
            return Mathf.Lerp(Mathf.Lerp(a, b, u), Mathf.Lerp(c, d, u), v);
        }

        private static float[] RandomShades(System.Random random, int count, float min, float max)
        {
            var shades = new float[count];
            for (int i = 0; i < count; i++)
            {
                shades[i] = min + (float)random.NextDouble() * (max - min);
            }

            return shades;
        }

        private static void DrawCrack(Color[] pixels, System.Random random, float darken)
        {
            float x = random.Next(Size);
            float y = random.Next(Size);
            float angle = (float)(random.NextDouble() * Math.PI * 2);
            int length = random.Next(20, 60);
            for (int i = 0; i < length; i++)
            {
                angle += (float)(random.NextDouble() - 0.5) * 0.9f;
                x += Mathf.Cos(angle);
                y += Mathf.Sin(angle);
                int ix = Wrap((int)x);
                int iy = Wrap((int)y);
                pixels[iy * Size + ix] *= darken;
            }
        }

        private static void DrawDot(Color[] pixels, int cx, int cy, int radius, Color color)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    if (dx * dx + dy * dy <= radius * radius)
                    {
                        Set(pixels, cx + dx, cy + dy, color);
                    }
                }
            }
        }

        private static void Set(Color[] pixels, int x, int y, Color color)
        {
            pixels[Wrap(y) * Size + Wrap(x)] = color;
        }

        private static int Wrap(int value) => ((value % Size) + Size) % Size;

        private static Color[] Opaque(Color[] pixels)
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i].a = 1f;
            }

            return pixels;
        }
    }
}
