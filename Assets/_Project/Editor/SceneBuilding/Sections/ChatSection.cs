using Lumen.Level;
using Lumen.NPC;
using Lumen.Player;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Lumen.EditorTools.SceneBuilding.Sections
{
    /// <summary>Geliştirici 2 — Arven sohbet paneli, Ollama servisi ve konuşma akışı.</summary>
    public sealed class ChatSection : ISceneSection
    {
        public int Order => 35;

        public void Build(SceneBuildContext ctx)
        {
            Canvas canvas = HudSection.CreateCanvas("Chat", 10);

            // Ekranın altında yarı saydam panel
            var panel = new GameObject("Panel", typeof(RectTransform));
            panel.transform.SetParent(canvas.transform, false);
            Place((RectTransform)panel.transform, new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(1100f, 320f));
            panel.AddComponent<Image>().color = new Color(0.04f, 0.05f, 0.08f, 0.92f);
            panel.AddComponent<Outline>().effectColor = new Color(0.45f, 0.7f, 1f, 0.6f);

            Text title = HudSection.CreateText(ctx, panel.transform, "Title", new Vector2(0f, 1f), new Vector2(30f, -20f), new Vector2(400f, 40f), 30, TextAnchor.UpperLeft);
            title.text = "Arven";
            title.color = new Color(0.6f, 0.82f, 1f);

            Text history = HudSection.CreateText(ctx, panel.transform, "History", new Vector2(0.5f, 1f), new Vector2(0f, -70f), new Vector2(1040f, 140f), 24, TextAnchor.UpperLeft);
            Text thinking = HudSection.CreateText(ctx, panel.transform, "Thinking", new Vector2(0f, 0f), new Vector2(30f, 84f), new Vector2(500f, 30f), 22, TextAnchor.MiddleLeft);
            thinking.color = new Color(1f, 0.85f, 0.5f);

            Text hint = HudSection.CreateText(ctx, panel.transform, "Hint", new Vector2(1f, 0f), new Vector2(-30f, 84f), new Vector2(500f, 30f), 20, TextAnchor.MiddleRight);
            hint.text = "[Enter] Gönder   [Tab] Kapat";
            hint.color = new Color(1f, 1f, 1f, 0.6f);

            Text trust = HudSection.CreateText(ctx, panel.transform, "Trust", new Vector2(1f, 1f), new Vector2(-30f, -20f), new Vector2(500f, 60f), 22, TextAnchor.UpperRight);
            trust.color = new Color(1f, 0.85f, 0.5f);

            InputField input = CreateInput(ctx, panel.transform);

            // Görünmez yönetici nesnesi: Ollama bağlantısı, konuşma akışı ve oyun durumu
            var chat = new GameObject("ArvenChat");
            OllamaService service = chat.AddComponent<OllamaService>();
            ArvenDialogue dialogue = chat.AddComponent<ArvenDialogue>();
            GameStateManager gameState = chat.AddComponent<GameStateManager>();
            MonoBehaviour playerRespawn = ctx.Get<PlayerController>().GetComponent<PlayerRespawn>();
            ctx.TryGet(out LoreJournal journal);       // Onur'un ara bölgesi kurar (sıra 5)
            ctx.TryGet(out CrystalCounter crystals);
            gameState.Configure(dialogue, null, playerRespawn, journal, crystals); // kapıyı NpcSection bağlar

            ChatView view = canvas.gameObject.AddComponent<ChatView>();
            view.Configure(panel, history, thinking, input, trust, gameState);
            dialogue.Configure(view, service);

            ctx.Set(view);
            ctx.Set(dialogue);
            ctx.Set(gameState);
        }

        private static InputField CreateInput(SceneBuildContext ctx, Transform parent)
        {
            var resources = new DefaultControls.Resources
            {
                inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd")
            };
            GameObject go = DefaultControls.CreateInputField(resources);
            go.name = "Input";
            go.transform.SetParent(parent, false);
            Place((RectTransform)go.transform, new Vector2(0.5f, 0f), new Vector2(0f, 20f), new Vector2(1040f, 50f));

            InputField input = go.GetComponent<InputField>();
            input.characterLimit = 200;
            input.textComponent.font = ctx.UiFont;
            input.textComponent.fontSize = 24;
            var placeholder = (Text)input.placeholder;
            placeholder.font = ctx.UiFont;
            placeholder.fontSize = 24;
            placeholder.text = "Arven'e bir şey söyle...";
            return input;
        }

        private static void Place(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }
    }
}
