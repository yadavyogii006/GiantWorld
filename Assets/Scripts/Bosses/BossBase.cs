using System;
using System.Collections;
using UnityEngine;

namespace GiantWorld.Bosses
{
    public abstract class BossBase : MonoBehaviour
    {
        [SerializeField] protected string bossName = "Boss";
        [SerializeField] protected float maxHealth = 100f;
        [SerializeField] protected Core.BossType bossType = Core.BossType.None;

        protected float currentHealth;
        protected bool isActive;
        protected bool isDefeated;
        protected Transform player;

        public string BossName => bossName;
        public float HealthPercent => maxHealth <= 0 ? 0f : currentHealth / maxHealth;
        public bool IsDefeated => isDefeated;
        public Core.BossType BossType => bossType;

        public event Action<BossBase> OnHealthChanged;
        public event Action<BossBase> OnDefeated;

        protected virtual void Awake()
        {
            currentHealth = maxHealth;
            gameObject.SetActive(false);
        }

        public virtual void Initialize(Transform playerTransform)
        {
            player = playerTransform;
            currentHealth = maxHealth;
            isDefeated = false;
            isActive = false;
        }

        public void ActivateBoss()
        {
            if (isDefeated) return;
            gameObject.SetActive(true);
            StartCoroutine(BossIntroRoutine());
        }

        protected virtual IEnumerator BossIntroRoutine()
        {
            Core.GameManager.Instance?.StartBossFight(bossType);
            yield return new WaitForSeconds(2f);
            isActive = true;
            Core.GameManager.Instance?.EnterBossCombat();
            OnBossFightStart();
        }

        protected abstract void OnBossFightStart();

        public virtual void TakeDamage(float amount)
        {
            if (!isActive || isDefeated || amount <= 0f) return;

            currentHealth = Mathf.Max(0f, currentHealth - amount);
            OnHealthChanged?.Invoke(this);

            if (currentHealth <= 0f)
                Defeat();
        }

        protected virtual void Defeat()
        {
            if (isDefeated) return;
            isDefeated = true;
            isActive = false;
            OnDefeated?.Invoke(this);
            Core.GameManager.Instance?.RegisterBossDefeat(bossType);
            StartCoroutine(DefeatRoutine());
        }

        protected virtual IEnumerator DefeatRoutine()
        {
            yield return new WaitForSeconds(1.5f);
            gameObject.SetActive(false);
        }

        protected bool PlayerInRange(float range)
        {
            if (player == null) return false;
            return Vector3.Distance(transform.position, player.position) <= range;
        }

        protected Vector3 DirectionToPlayer()
        {
            if (player == null) return transform.forward;
            Vector3 dir = player.position - transform.position;
            dir.y = 0f;
            return dir.sqrMagnitude > 0.01f ? dir.normalized : transform.forward;
        }
    }
}
