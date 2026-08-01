using UnityEngine;

namespace GiantWorld.World
{
    public class Collectible : MonoBehaviour
    {
        [SerializeField] int healAmount = 15;
        [SerializeField] float bobSpeed = 2f;
        [SerializeField] float bobHeight = 0.3f;

        Vector3 startPos;

        void Start() => startPos = transform.position;

        void Update()
        {
            float y = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(startPos.x, y, startPos.z);
            transform.Rotate(Vector3.up, 90f * Time.deltaTime);
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            var health = other.GetComponent<Player.PlayerHealth>();
            if (health == null || !health.IsAlive) return;

            health.Heal(healAmount);
            Destroy(gameObject);
        }
    }
}
