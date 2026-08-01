using System;
using UnityEngine;

namespace GiantWorld.Player
{
    public class PlayerHealth : MonoBehaviour
    {
        public int MaxHealth { get; private set; } = Core.GameConstants.MaxPlayerHealth;
        public int CurrentHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0;
        public bool IsInvulnerable { get; private set; }

        public event Action<int, int> OnHealthChanged;
        public event Action OnDeath;

        float invulnTimer;

        void Start()
        {
            CurrentHealth = MaxHealth;
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        void Update()
        {
            if (!IsInvulnerable) return;
            invulnTimer -= Time.deltaTime;
            if (invulnTimer <= 0f)
                IsInvulnerable = false;
        }

        public void TakeDamage(int amount)
        {
            if (!IsAlive || IsInvulnerable || amount <= 0) return;

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

            if (CurrentHealth <= 0)
            {
                OnDeath?.Invoke();
                Core.GameManager.Instance?.NotifyPlayerDeath();
            }
            else
            {
                SetInvulnerable(1.2f);
            }
        }

        public void Heal(int amount)
        {
            if (!IsAlive || amount <= 0) return;
            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        public void SetInvulnerable(float duration)
        {
            IsInvulnerable = true;
            invulnTimer = duration;
        }
    }
}
