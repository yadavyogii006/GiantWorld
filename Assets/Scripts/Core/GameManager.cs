using System;
using UnityEngine;

namespace GiantWorld.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState State { get; private set; } = GameState.Exploring;
        public BossType CurrentBoss { get; private set; } = BossType.None;
        public int BossesDefeated { get; private set; }

        public event Action<GameState> OnStateChanged;
        public event Action<BossType> OnBossStarted;
        public event Action<BossType> OnBossDefeated;
        public event Action OnVictory;
        public event Action OnPlayerDied;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void SetState(GameState newState)
        {
            if (State == newState) return;
            State = newState;
            OnStateChanged?.Invoke(newState);
        }

        public void StartBossFight(BossType boss)
        {
            CurrentBoss = boss;
            SetState(GameState.BossIntro);
            OnBossStarted?.Invoke(boss);
        }

        public void EnterBossCombat()
        {
            SetState(GameState.BossFight);
        }

        public void RegisterBossDefeat(BossType boss)
        {
            if (CurrentBoss != boss) return;

            BossesDefeated++;
            SetState(GameState.BossDefeated);
            OnBossDefeated?.Invoke(boss);
            CurrentBoss = BossType.None;

            if (BossesDefeated >= 4)
            {
                SetState(GameState.Victory);
                OnVictory?.Invoke();
            }
            else
            {
                SetState(GameState.Exploring);
            }
        }

        public void NotifyPlayerDeath()
        {
            SetState(GameState.PlayerDead);
            OnPlayerDied?.Invoke();
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
    }
}
