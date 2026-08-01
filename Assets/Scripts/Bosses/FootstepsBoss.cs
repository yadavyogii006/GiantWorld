using System.Collections;
using UnityEngine;

namespace GiantWorld.Bosses
{
    /// <summary>
    /// Human Footsteps — giant stomps send shockwaves across the kitchen.
    /// </summary>
    public class FootstepsBoss : BossBase
    {
        enum FootState { Waiting, Warning, Stomp, Retreat }

        [SerializeField] float stompInterval = 3.5f;
        [SerializeField] float warningDuration = 1.2f;
        [SerializeField] float stompRadius = 8f;
        [SerializeField] float stompDamage = 40f;
        [SerializeField] float shockwaveSpeed = 25f;
        [SerializeField] float shockwaveMaxRadius = 22f;

        FootState state = FootState.Waiting;
        float stateTimer;
        Vector3 nextStompPos;
        Transform leftFoot;
        Transform rightFoot;
        bool useLeftFoot = true;
        Material warningMat;
        GameObject warningRing;

        protected override void Awake()
        {
            bossName = "Human Footsteps";
            bossType = Core.BossType.Footsteps;
            maxHealth = 180f;
            base.Awake();
        }

        protected override void OnBossFightStart()
        {
            state = FootState.Waiting;
            stateTimer = 1.5f;
        }

        void Update()
        {
            if (!isActive || isDefeated || player == null) return;

            switch (state)
            {
                case FootState.Waiting: UpdateWaiting(); break;
                case FootState.Warning: UpdateWarning(); break;
            }
        }

        void UpdateWaiting()
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
                BeginStompTelegraph();
        }

        void BeginStompTelegraph()
        {
            nextStompPos = player.position;
            nextStompPos.y = 0f;

            state = FootState.Warning;
            stateTimer = warningDuration;

            if (warningRing != null) Destroy(warningRing);
            warningRing = CreateWarningRing(nextStompPos, stompRadius);
        }

        void UpdateWarning()
        {
            stateTimer -= Time.deltaTime;

            Transform foot = useLeftFoot ? leftFoot : rightFoot;
            if (foot != null)
            {
                Vector3 footTarget = nextStompPos + Vector3.up * 15f;
                foot.position = Vector3.Lerp(foot.position, footTarget, Time.deltaTime * 3f);
            }

            if (stateTimer <= 0f)
                StartCoroutine(StompRoutine());
        }

        IEnumerator StompRoutine()
        {
            state = FootState.Stomp;
            Transform foot = useLeftFoot ? leftFoot : rightFoot;
            useLeftFoot = !useLeftFoot;

            if (foot != null)
            {
                Vector3 start = foot.position;
                Vector3 ground = nextStompPos + Vector3.up * 0.5f;
                float t = 0f;
                while (t < 0.25f)
                {
                    t += Time.deltaTime;
                    foot.position = Vector3.Lerp(start, ground, t / 0.25f);
                    yield return null;
                }

                CameraShake.Shake(0.4f, 0.6f);

                if (Vector3.Distance(player.position, nextStompPos) <= stompRadius)
                {
                    var ph = player.GetComponent<Player.PlayerHealth>();
                    ph?.TakeDamage(Mathf.RoundToInt(stompDamage));
                    player.GetComponent<Player.PlayerController>()?.ApplyKnockback(
                        (player.position - nextStompPos).normalized, 15f);
                }

                StartCoroutine(ShockwaveRoutine(nextStompPos));
            }

            if (warningRing != null)
            {
                Destroy(warningRing);
                warningRing = null;
            }

            yield return new WaitForSeconds(0.8f);

            state = FootState.Retreat;
            if (foot != null)
            {
                Vector3 up = foot.position + Vector3.up * 20f;
                float t = 0f;
                while (t < 0.5f)
                {
                    t += Time.deltaTime;
                    foot.position = Vector3.Lerp(foot.position, up, t / 0.5f);
                    yield return null;
                }
            }

            state = FootState.Waiting;
            stateTimer = stompInterval;
        }

        IEnumerator ShockwaveRoutine(Vector3 origin)
        {
            float radius = 1f;
            GameObject ring = CreateWarningRing(origin, 1f);
            var col = ring.GetComponent<SphereCollider>();
            if (col != null) col.radius = 1f;

            while (radius < shockwaveMaxRadius)
            {
                radius += shockwaveSpeed * Time.deltaTime;
                ring.transform.localScale = Vector3.one * radius * 2f;

                if (player != null && Vector3.Distance(player.position, origin) <= radius &&
                    Vector3.Distance(player.position, origin) >= radius - 2f)
                {
                    var ph = player.GetComponent<Player.PlayerHealth>();
                    ph?.TakeDamage(8);
                }

                yield return null;
            }

            Destroy(ring);
        }

        GameObject CreateWarningRing(Vector3 pos, float radius)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "StompWarning";
            Destroy(go.GetComponent<Collider>());
            go.transform.position = pos + Vector3.up * 0.05f;
            go.transform.localScale = new Vector3(radius * 2f, 0.02f, radius * 2f);
            var rend = go.GetComponent<Renderer>();
            rend.material = World.WorldBuilder.CreateMaterial(new Color(1f, 0.2f, 0.1f, 0.6f));
            return go;
        }

        public void SetupVisuals(Transform left, Transform right)
        {
            leftFoot = left;
            rightFoot = right;
        }
    }

    public static class CameraShake
    {
        static float shakeTimer;
        static float shakeIntensity;
        static Transform camTransform;
        static Vector3 originalLocalPos;

        public static void RegisterCamera(Transform cam)
        {
            camTransform = cam;
            originalLocalPos = cam.localPosition;
        }

        public static void Shake(float duration, float intensity)
        {
            shakeTimer = duration;
            shakeIntensity = intensity;
        }

        public static void UpdateShake()
        {
            if (camTransform == null) return;
            if (shakeTimer > 0f)
            {
                shakeTimer -= Time.deltaTime;
                camTransform.localPosition = originalLocalPos + Random.insideUnitSphere * shakeIntensity;
            }
            else
            {
                camTransform.localPosition = originalLocalPos;
            }
        }
    }
}
