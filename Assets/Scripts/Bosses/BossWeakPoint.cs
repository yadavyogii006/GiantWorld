using UnityEngine;

namespace GiantWorld.Bosses
{
    public class BossWeakPoint : MonoBehaviour
    {
        [SerializeField] BossBase owner;
        [SerializeField] float damageMultiplier = 1.5f;

        public void Bind(BossBase boss) => owner = boss;

        public void ApplyDamage(float damage)
        {
            if (owner == null) return;
            owner.TakeDamage(damage * damageMultiplier);
        }
    }
}
