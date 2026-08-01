using UnityEngine;

namespace GiantWorld.Player
{
    public class PlayerCombat : MonoBehaviour
    {
        [SerializeField] float damage = Core.GameConstants.AttackDamage;
        [SerializeField] float range = Core.GameConstants.AttackRange;
        [SerializeField] float cooldown = Core.GameConstants.AttackCooldown;
        [SerializeField] LayerMask hitMask = ~0;

        float cooldownTimer;
        Transform attackOrigin;

        void Awake()
        {
            attackOrigin = transform;
        }

        void Update()
        {
            if (cooldownTimer > 0f)
                cooldownTimer -= Time.deltaTime;
        }

        public bool TryAttack()
        {
            if (cooldownTimer > 0f) return false;
            cooldownTimer = cooldown;

            Vector3 origin = attackOrigin.position + Vector3.up * 0.5f;
            Vector3 forward = attackOrigin.forward;

            Collider[] hits = Physics.OverlapSphere(origin + forward * (range * 0.5f), range * 0.65f, hitMask);
            bool hitSomething = false;

            foreach (Collider col in hits)
            {
                if (col.transform.IsChildOf(transform) || col.CompareTag("Player")) continue;

                var boss = col.GetComponentInParent<Bosses.BossBase>();
                if (boss != null)
                {
                    boss.TakeDamage(damage);
                    hitSomething = true;
                    continue;
                }

                var weak = col.GetComponent<Bosses.BossWeakPoint>();
                if (weak != null)
                {
                    weak.ApplyDamage(damage);
                    hitSomething = true;
                }
            }

            return true;
        }

        void OnDrawGizmosSelected()
        {
            if (attackOrigin == null) attackOrigin = transform;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackOrigin.position + attackOrigin.forward * (range * 0.5f), range * 0.65f);
        }
    }
}
