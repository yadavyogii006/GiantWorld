using UnityEngine;

namespace GiantWorld.World
{
    public class HazardZone : MonoBehaviour
    {
        [SerializeField] float damage = 10f;
        [SerializeField] float tickRate = 0.5f;
        [SerializeField] bool instantKill;

        float tickTimer;

        void OnTriggerStay(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            tickTimer -= Time.deltaTime;
            if (tickTimer > 0f) return;
            tickTimer = tickRate;

            var health = other.GetComponent<Player.PlayerHealth>();
            if (health == null) return;

            if (instantKill)
                health.TakeDamage(health.MaxHealth);
            else
                health.TakeDamage(Mathf.RoundToInt(damage));
        }

        public static GameObject SpawnSphere(Vector3 pos, float radius, float dmg, float duration, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "HazardZone";
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * radius * 2f;

            var col = go.GetComponent<SphereCollider>();
            col.isTrigger = true;

            var rend = go.GetComponent<Renderer>();
            rend.material = WorldBuilder.CreateMaterial(color);

            var hz = go.AddComponent<HazardZone>();
            hz.damage = dmg / duration;
            hz.tickRate = 0.2f;

            Destroy(go, duration);
            return go;
        }
    }
}
