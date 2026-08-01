using UnityEngine;

namespace GiantWorld.World
{
    public class BossTrigger : MonoBehaviour
    {
        [SerializeField] Bosses.BossBase boss;
        [SerializeField] string promptText = "Enter boss arena";

        bool triggered;

        public void Bind(Bosses.BossBase b) => boss = b;

        void OnTriggerEnter(Collider other)
        {
            if (triggered || boss == null || boss.IsDefeated) return;
            if (!other.CompareTag("Player")) return;

            triggered = true;
            boss.ActivateBoss();
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            var col = GetComponent<BoxCollider>();
            if (col != null)
                Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(col != null ? col.center : Vector3.zero, col != null ? col.size : Vector3.one);
        }
    }
}
