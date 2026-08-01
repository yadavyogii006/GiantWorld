using System.Collections;
using UnityEngine;

namespace GiantWorld.Bosses
{
    /// <summary>
    /// Washing Machine — spin cycle, door slams, and water splash attacks.
    /// </summary>
    public class WashingMachineBoss : BossBase
    {
        enum WashState { Idle, SpinUp, Spinning, DoorSlam, Splash }

        [SerializeField] float spinDamageRadius = 10f;
        [SerializeField] float slamDamage = 35f;
        [SerializeField] float splashDamage = 20f;

        WashState state = WashState.Idle;
        float stateTimer;
        Transform drum;
        Transform door;
        Transform splashOrigin;
        float spinAngle;

        protected override void Awake()
        {
            bossName = "Washing Machine";
            bossType = Core.BossType.WashingMachine;
            maxHealth = 160f;
            base.Awake();
        }

        protected override void OnBossFightStart()
        {
            state = WashState.SpinUp;
            stateTimer = 1.5f;
        }

        void Update()
        {
            if (!isActive || isDefeated || player == null) return;

            switch (state)
            {
                case WashState.Idle: UpdateIdle(); break;
                case WashState.SpinUp: UpdateSpinUp(); break;
                case WashState.Spinning: UpdateSpinning(); break;
                case WashState.DoorSlam: break;
                case WashState.Splash: UpdateSplash(); break;
            }
        }

        void UpdateIdle()
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                state = WashState.SpinUp;
                stateTimer = 1.2f;
            }
        }

        void UpdateSpinUp()
        {
            stateTimer -= Time.deltaTime;
            spinAngle += Time.deltaTime * 120f;
            if (drum != null)
                drum.localRotation = Quaternion.Euler(spinAngle, 0f, 0f);

            if (stateTimer <= 0f)
            {
                state = WashState.Spinning;
                stateTimer = 5f;
            }
        }

        void UpdateSpinning()
        {
            stateTimer -= Time.deltaTime;
            spinAngle += Time.deltaTime * 720f;
            if (drum != null)
                drum.localRotation = Quaternion.Euler(spinAngle, 0f, 0f);

            if (PlayerInRange(spinDamageRadius))
            {
                var ph = player.GetComponent<Player.PlayerHealth>();
                ph?.TakeDamage(Mathf.RoundToInt(8f * Time.deltaTime));
                player.GetComponent<Player.PlayerController>()?.ApplyKnockback(
                    (player.position - transform.position).normalized, 6f * Time.deltaTime);
            }

            if (stateTimer <= 0f)
                StartCoroutine(DoorSlamRoutine());
        }

        IEnumerator DoorSlamRoutine()
        {
            state = WashState.DoorSlam;
            if (door != null)
            {
                Quaternion open = door.localRotation;
                Quaternion closed = Quaternion.Euler(0f, -90f, 0f);
                float t = 0f;
                while (t < 0.4f)
                {
                    t += Time.deltaTime;
                    door.localRotation = Quaternion.Slerp(open, closed, t / 0.4f);
                    yield return null;
                }

                if (PlayerInRange(6f))
                {
                    var ph = player.GetComponent<Player.PlayerHealth>();
                    ph?.TakeDamage(Mathf.RoundToInt(slamDamage));
                }

                yield return new WaitForSeconds(0.3f);

                t = 0f;
                while (t < 0.5f)
                {
                    t += Time.deltaTime;
                    door.localRotation = Quaternion.Slerp(closed, open, t / 0.5f);
                    yield return null;
                }
            }

            state = WashState.Splash;
            stateTimer = 0.5f;
            SpawnSplash();
        }

        void UpdateSplash()
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                state = WashState.Idle;
                stateTimer = 2f;
            }
        }

        void SpawnSplash()
        {
            Vector3 origin = splashOrigin != null ? splashOrigin.position : transform.position + Vector3.up * 3f;
            var hazard = World.HazardZone.SpawnSphere(origin, 5f, splashDamage, 1.2f, new Color(0.2f, 0.5f, 1f, 0.5f));
            Destroy(hazard, 1.5f);
        }

        public void SetupVisuals(Transform drumTransform, Transform doorTransform, Transform splash)
        {
            drum = drumTransform;
            door = doorTransform;
            splashOrigin = splash;
        }
    }
}
