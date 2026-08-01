using UnityEngine;
using UnityEngine.UI;

namespace GiantWorld.UI
{
    public class UIManager : MonoBehaviour
    {
        Text healthText;
        Text bossText;
        Text stateText;
        Text hintText;
        Slider bossHealthBar;
        Slider playerHealthBar;
        GameObject victoryPanel;
        GameObject deathPanel;

        Bosses.BossBase trackedBoss;

        void Start()
        {
            var gm = Core.GameManager.Instance;
            if (gm != null)
            {
                gm.OnStateChanged += HandleStateChanged;
                gm.OnBossStarted += HandleBossStarted;
                gm.OnBossDefeated += HandleBossDefeated;
                gm.OnVictory += ShowVictory;
                gm.OnPlayerDied += ShowDeath;
            }
        }

        public void BuildUI(Canvas canvas, Player.PlayerHealth playerHealth)
        {
            healthText = CreateText(canvas.transform, "HealthText", new Vector2(-20f, -20f), TextAnchor.LowerRight, 18);
            bossText = CreateText(canvas.transform, "BossText", new Vector2(0f, -30f), TextAnchor.UpperCenter, 22);
            stateText = CreateText(canvas.transform, "StateText", new Vector2(20f, -20f), TextAnchor.LowerLeft, 16);
            hintText = CreateText(canvas.transform, "HintText", new Vector2(0f, 40f), TextAnchor.LowerCenter, 14);

            playerHealthBar = CreateBar(canvas.transform, "PlayerBar", new Vector2(20f, -50f), new Vector2(200f, 16f), Color.green);
            bossHealthBar = CreateBar(canvas.transform, "BossBar", new Vector2(0f, -70f), new Vector2(300f, 20f), Color.red);
            bossHealthBar.gameObject.SetActive(false);

            hintText.text = "WASD: Move | Shift: Sprint | Space: Jump | LMB: Attack | RMB+Drag: Camera";
            hintText.color = new Color(1f, 1f, 1f, 0.7f);

            playerHealth.OnHealthChanged += UpdatePlayerHealth;
            UpdatePlayerHealth(playerHealth.MaxHealth, playerHealth.MaxHealth);

            victoryPanel = CreateOverlayPanel(canvas.transform, "VictoryPanel", "VICTORY!\nYou survived the Giant Kitchen.", new Color(0.1f, 0.5f, 0.2f, 0.9f));
            deathPanel = CreateOverlayPanel(canvas.transform, "DeathPanel", "SQUASHED!\nPress R to restart.", new Color(0.5f, 0.1f, 0.1f, 0.9f));
            victoryPanel.SetActive(false);
            deathPanel.SetActive(false);
        }

        Player.PlayerHealth playerHealth;

        public void BindPlayer(Player.PlayerHealth ph)
        {
            playerHealth = ph;
        }

        void UpdatePlayerHealth(int current, int max)
        {
            healthText.text = $"HP: {current}/{max}";
            if (playerHealthBar != null)
                playerHealthBar.value = (float)current / max;
        }

        void HandleStateChanged(Core.GameState state)
        {
            stateText.text = state switch
            {
                Core.GameState.Exploring => "Exploring the Kitchen...",
                Core.GameState.BossIntro => "BOSS APPROACHING!",
                Core.GameState.BossFight => "BOSS FIGHT!",
                Core.GameState.BossDefeated => "Boss Defeated!",
                Core.GameState.Victory => "All bosses defeated!",
                Core.GameState.PlayerDead => "You died.",
                _ => ""
            };
        }

        void HandleBossStarted(Core.BossType boss)
        {
            bossText.text = $"⚠ {boss} ⚠";
            bossHealthBar.gameObject.SetActive(true);
        }

        void HandleBossDefeated(Core.BossType boss)
        {
            bossText.text = $"{boss} defeated!";
            bossHealthBar.gameObject.SetActive(false);
            trackedBoss = null;
        }

        public void TrackBoss(Bosses.BossBase boss)
        {
            if (trackedBoss != null)
                trackedBoss.OnHealthChanged -= OnBossHealthChanged;

            trackedBoss = boss;
            if (trackedBoss != null)
                trackedBoss.OnHealthChanged += OnBossHealthChanged;
        }

        void OnBossHealthChanged(Bosses.BossBase boss)
        {
            if (bossHealthBar != null)
                bossHealthBar.value = boss.HealthPercent;
            bossText.text = $"{boss.BossName}  {Mathf.CeilToInt(boss.HealthPercent * 100f)}%";
        }

        void ShowVictory() => victoryPanel?.SetActive(true);
        void ShowDeath() => deathPanel?.SetActive(true);

        void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current?.rKey.wasPressedThisFrame == true)
            {
                var state = Core.GameManager.Instance?.State;
                if (state == Core.GameState.PlayerDead || state == Core.GameState.Victory)
                    Core.GameManager.Instance?.RestartGame();
            }

            if (trackedBoss != null && trackedBoss.gameObject.activeInHierarchy)
                OnBossHealthChanged(trackedBoss);
        }

        Text CreateText(Transform parent, string name, Vector2 anchoredPos, TextAnchor anchor, int fontSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchor == TextAnchor.UpperCenter ? new Vector2(0.5f, 1f) :
                             anchor == TextAnchor.LowerCenter ? new Vector2(0.5f, 0f) :
                             anchor == TextAnchor.LowerRight ? new Vector2(1f, 0f) : new Vector2(0f, 0f);
            rect.anchorMax = rect.anchorMin;
            rect.pivot = anchor == TextAnchor.UpperCenter ? new Vector2(0.5f, 1f) :
                         anchor == TextAnchor.LowerCenter ? new Vector2(0.5f, 0f) :
                         anchor == TextAnchor.LowerRight ? new Vector2(1f, 0f) : new Vector2(0f, 0f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = new Vector2(600f, 40f);

            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = Color.white;
            text.supportRichText = true;
            return text;
        }

        Slider CreateBar(Transform parent, string name, Vector2 pos, Vector2 size, Color fillColor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = pos.x > 0 && pos.y < 0 ? new Vector2(0f, 0f) : new Vector2(0.5f, pos.y < 0 ? 0f : 1f);
            rect.anchorMax = rect.anchorMin;
            rect.pivot = new Vector2(pos.x > 0 ? 0f : 0.5f, pos.y < 0 ? 0f : 1f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            var bg = new GameObject("Background");
            bg.transform.SetParent(go.transform, false);
            var bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            bg.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(go.transform, false);
            var fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = new Vector2(2f, 2f);
            fillAreaRect.offsetMax = new Vector2(-2f, -2f);

            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            var fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            fill.AddComponent<Image>().color = fillColor;

            var slider = go.AddComponent<Slider>();
            slider.fillRect = fillRect;
            slider.maxValue = 1f;
            slider.value = 1f;
            return slider;
        }

        GameObject CreateOverlayPanel(Transform parent, string name, string message, Color bgColor)
        {
            var panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            var rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            panel.AddComponent<Image>().color = bgColor;

            var textGo = new GameObject("Message");
            textGo.transform.SetParent(panel.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            var text = textGo.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 32;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = message;
            return panel;
        }
    }
}
