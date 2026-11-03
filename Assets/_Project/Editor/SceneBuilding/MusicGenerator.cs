using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Lumen.EditorTools.SceneBuilding
{
    /// <summary>
    /// Geliştirici 2 — müziği ve kısa ses efektlerini dışarıdan indirmeden kodla üretir ve WAV olarak kaydeder (Assets/_Project/Audio).
    /// Oyun: alçak, uyumsuz bir uğultu + yavaş kalp atışı + ara sıra metal çan (gerilim).
    /// Menü: daha sakin bir akor + daha sık çan.
    /// Tüm sesler döngü süresine tam oturur, böylece müzik takılmadan başa sarar.
    /// </summary>
    public static class MusicGenerator
    {
        public const string AudioFolder = "Assets/_Project/Audio";
        public const string GamePath = AudioFolder + "/TempleTension.wav";
        public const string MenuPath = AudioFolder + "/MenuTheme.wav";

        private const int SampleRate = 22050;
        private const float LoopSeconds = 40f;

        public static AudioClip GameMusic() => Ensure(GamePath, Game);

        public static AudioClip MenuMusic() => Ensure(MenuPath, Menu);

        /// <summary>Kutsal bölüm: daha hızlı kalp atışı ve daha yüksek, gergin bir uğultu (son sınavlar).</summary>
        public static AudioClip SanctumMusic() => Ensure(AudioFolder + "/SanctumTension.wav", Sanctum);

        // Kısa efektler (döngü değil): kristal/yazıt çanı, kapı açılma gümbürtüsü, Arven'in saldırı darbesi
        public static AudioClip ChimeSfx() => Ensure(AudioFolder + "/Chime.wav", Chime);

        public static AudioClip RumbleSfx() => Ensure(AudioFolder + "/Rumble.wav", Rumble);

        public static AudioClip HitSfx() => Ensure(AudioFolder + "/Hit.wav", Hit);

        private static AudioClip Ensure(string path, Func<float[]> generate)
        {
            if (!File.Exists(path))
            {
                SceneBuildContext.EnsureFolder(AudioFolder);
                WriteWav(path, generate());
                AssetDatabase.ImportAsset(path);
            }

            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }

        private static float[] Sanctum()
        {
            var s = new float[(int)(SampleRate * LoopSeconds)];
            // Re (73.4 Hz) ve yarım oktav üstü (triton, "şeytan aralığı"): çözülmeyen gerilim
            Pad(s, new[] { 73.4f, 103.8f, 146.8f, 155.6f }, 0.2f, 8);
            for (int i = 0; i < 40; i++) // 40 atış / 40 sn: kalp hızlanıyor
            {
                float t = i * LoopSeconds / 40f;
                Thump(s, t, 0.55f);
                Thump(s, t + 0.22f, 0.35f);
            }

            Bells(s, 12, 0.14f, new System.Random(23), 1100f);
            return Normalize(s);
        }

        // Kristal çanı: üç kısmi tonlu, 2.5 sn'de sönen parlak ses
        private static float[] Chime()
        {
            var s = new float[SampleRate * 3];
            float[] partials = { 1f, 2.0f, 3.01f };
            for (int i = 0; i < s.Length; i++)
            {
                float t = i / (float)SampleRate;
                for (int p = 0; p < partials.Length; p++)
                {
                    s[i] += Mathf.Sin(2f * Mathf.PI * 1318.5f * partials[p] * t) * Mathf.Exp(-t * (1.8f + p * 1.5f)) / (p + 1);
                }
            }

            return Normalize(s);
        }

        // Taş kapı: alçak, titreyen gümbürtü (1.8 sn), sonda yavaşça söner
        private static float[] Rumble()
        {
            var s = new float[(int)(SampleRate * 1.8f)];
            var random = new System.Random(5);
            float noise = 0f;
            for (int i = 0; i < s.Length; i++)
            {
                float t = i / (float)SampleRate;
                noise = noise * 0.97f + ((float)random.NextDouble() * 2f - 1f) * 0.03f; // alçak geçiren gürültü: taş sürtünmesi
                float envelope = Mathf.Clamp01(t * 6f) * Mathf.Clamp01((1.8f - t) * 1.5f);
                s[i] = envelope * (noise * 6f + 0.5f * Mathf.Sin(2f * Mathf.PI * 42f * t));
            }

            return Normalize(s);
        }

        // Saldırı: tek, sert ve alçak darbe
        private static float[] Hit()
        {
            var s = new float[SampleRate];
            Thump(s, 0f, 1f);
            return Normalize(s);
        }

        private static float[] Game()
        {
            var s = new float[(int)(SampleRate * LoopSeconds)];
            // La (55 Hz) ve beşlisi + yarım ton yukarıdaki Si bemol: kulağa "bir şey yanlış" gelen uyumsuzluk
            Pad(s, new[] { 55f, 82.4f, 110f, 116.5f }, 0.22f, 4);
            // Kalp atışı: 24 atış / 40 sn, her atış çift vuruş ("tum-tum")
            for (int i = 0; i < 24; i++)
            {
                float t = i * LoopSeconds / 24f;
                Thump(s, t, 0.5f);
                Thump(s, t + 0.28f, 0.32f);
            }

            Bells(s, 7, 0.16f, new System.Random(11), 880f);
            return Normalize(s);
        }

        private static float[] Menu()
        {
            var s = new float[(int)(SampleRate * LoopSeconds)];
            // La minör + 9'lu akor: sakin ama gizemli
            Pad(s, new[] { 110f, 164.8f, 246.9f, 261.6f }, 0.18f, 5);
            Bells(s, 12, 0.14f, new System.Random(7), 1320f);
            return Normalize(s);
        }

        // Uzun süren katmanlar; her biri yavaşça kabarıp söner (dalga sayısı döngüye tam oturur)
        private static void Pad(float[] s, float[] freqs, float amp, int swells)
        {
            for (int k = 0; k < freqs.Length; k++)
            {
                float f = Fit(freqs[k]);
                float phase = k * 1.3f;
                for (int i = 0; i < s.Length; i++)
                {
                    float t = i / (float)SampleRate;
                    float swell = 0.55f + 0.45f * Mathf.Sin(2f * Mathf.PI * swells * t / LoopSeconds + phase);
                    float tone = Mathf.Sin(2f * Mathf.PI * f * t) + 0.3f * Mathf.Sin(2f * Mathf.PI * 2f * f * t);
                    s[i] += amp * swell * tone;
                }
            }
        }

        // Alçak, hızla sönen bir vuruş
        private static void Thump(float[] s, float start, float amp)
        {
            int begin = (int)(start * SampleRate);
            for (int j = 0; j < SampleRate / 2; j++)
            {
                float t = j / (float)SampleRate;
                float f = 48f + 40f * Mathf.Exp(-t * 30f); // vuruşun başında perde hafifçe düşer
                Add(s, begin + j, amp * Mathf.Exp(-t * 9f) * Mathf.Sin(2f * Mathf.PI * f * t));
            }
        }

        // Metal çan: uyumsuz kısmi tonlar (1, 2.76, 5.4 kat) ve uzun sönüm; zamanlar rastgele ama tohumla sabit
        private static void Bells(float[] s, int count, float amp, System.Random random, float baseFreq)
        {
            float[] partials = { 1f, 2.76f, 5.4f };
            for (int n = 0; n < count; n++)
            {
                int begin = (int)(random.NextDouble() * s.Length);
                float f0 = baseFreq * (0.5f + (float)random.NextDouble());
                for (int j = 0; j < SampleRate * 4; j++)
                {
                    float t = j / (float)SampleRate;
                    float v = 0f;
                    for (int p = 0; p < partials.Length; p++)
                    {
                        v += Mathf.Sin(2f * Mathf.PI * f0 * partials[p] * t) * Mathf.Exp(-t * (1.2f + p)) / (p + 1);
                    }

                    Add(s, begin + j, amp * v);
                }
            }
        }

        // Döngü sonunu aşan kuyruk başa sarılır: başa dönüşte kesinti olmaz
        private static void Add(float[] s, int index, float value) => s[index % s.Length] += value;

        // Frekansı döngüde tam sayıda dalga olacak şekilde yuvarlar
        private static float Fit(float f) => Mathf.Round(f * LoopSeconds) / LoopSeconds;

        private static float[] Normalize(float[] s)
        {
            float peak = 0.0001f;
            foreach (float v in s)
            {
                peak = Math.Max(peak, Math.Abs(v));
            }

            for (int i = 0; i < s.Length; i++)
            {
                s[i] = s[i] / peak * 0.8f;
            }

            return s;
        }

        // 16 bit mono PCM WAV yazar
        private static void WriteWav(string path, float[] samples)
        {
            using var writer = new BinaryWriter(File.Create(path));
            int dataSize = samples.Length * 2;
            writer.Write("RIFF".ToCharArray());
            writer.Write(36 + dataSize);
            writer.Write("WAVEfmt ".ToCharArray());
            writer.Write(16);
            writer.Write((short)1);  // PCM
            writer.Write((short)1);  // mono
            writer.Write(SampleRate);
            writer.Write(SampleRate * 2);
            writer.Write((short)2);
            writer.Write((short)16);
            writer.Write("data".ToCharArray());
            writer.Write(dataSize);
            foreach (float v in samples)
            {
                writer.Write((short)(Mathf.Clamp(v, -1f, 1f) * short.MaxValue));
            }
        }

        /// <summary>Sahneye döngüde çalan, 2D bir müzik kaynağı ekler. Ses düzeyi ayarlardaki "Ses" ile (AudioListener) değişir.</summary>
        public static AudioSource AddMusic(string name, AudioClip clip, float volume)
        {
            var go = new GameObject(name);
            AudioSource source = go.AddComponent<AudioSource>();
            source.clip = clip;
            source.loop = true;
            source.playOnAwake = true;
            source.spatialBlend = 0f;
            source.volume = volume;
            return source;
        }
    }
}
