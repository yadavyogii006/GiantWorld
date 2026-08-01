using System.Collections;
using UnityEngine;

namespace GiantWorld.Bosses
{
    /// <summary>
    /// The Cat — stalks, pounces, and swipes across the kitchen floor.
    /// </summary>
    public class CatBoss : BossBase
    {
        enum CatState { Stalk, PounceWindup, Pounce, Recover, Swipe }

        [SerializeField] float stalkSpeed = 6f;
        [SerializeField] float pounceRange = 18f;
        [SerializeField] float swipeRange = 5f;
        [SerializeField] float swipeDamage = 25f;

        CatState state = CatState.Stalk;
        Vector3 stalkTarget;
        float stateTimer;
        Transform pawCollider;
        Transform catBody;

        protected override void Awake()
        {
            bossName = "The Cat";
            bossType = Core.BossType.Cat;
            maxHealth = 120f;
            base.Awake();
        }

        protected override void OnBossFightStart()
        {
            state = CatState.Stalk;
            PickNewStalkTarget();
        }

        void Update()
        {
            if (!isActive || isDefeated || player == null) return;

            switch (state)
            {
                case CatState.Stalk: UpdateStalk(); break;
                case CatState.PounceWindup: UpdatePounceWindup(); break;
                case CatState.Pounce: UpdatePounce(); break;
                case CatState.Recover: UpdateRecover(); break;
                case CatState.Swipe: UpdateSwipe(); break;
            }
        }

        void UpdateStalk()
        {
            Vector3 dir = stalkTarget - transform.position;
            dir.y = 0f;
            if (dir.magnitude < 2f)
            {
                if (PlayerInRange(pounceRange))
                    StartPounceWindup();
                else if (PlayerInRange(swipeRange + 2f))
                    StartSwipe();
                else
                    PickNewStalkTarget();
                return;
            }

            transform.position += dir.normalized * stalkSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 4f);

            if (PlayerInRange(pounceRange * 0.85f))
                StartPounceWindup();
        }

        void StartPounceWindup()
        {
            state = CatState.PounceWindup;
            stateTimer = 1.2f;
            if (catBody != null)
                StartCoroutine(WiggleBody(0.15f, 6f));
        }

        void UpdatePounceWindup()
        {
            stateTimer -= Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(DirectionToPlayer()), Time.deltaTime * 6f);
            if (stateTimer <= 0f)
            {
                state = CatState.Pounce;
                stateTimer = 0.35f;
            }
        }

        void UpdatePounce()
        {
            stateTimer -= Time.deltaTime;
            transform.position += DirectionToPlayer() * 28f * Time.deltaTime;

            if (PlayerInRange(swipeRange))
            {
                var ph = player.GetComponent<Player.PlayerHealth>();
                ph?.TakeDamage(30);
                player.GetComponent<Player.PlayerController>()?.ApplyKnockback(DirectionToPlayer(), 12f);
            }

            if (stateTimer <= 0f)
            {
                state = CatState.Recover;
                stateTimer = 1.5f;
            }
        }

        void StartSwipe()
        {
            state = CatState.Swipe;
            stateTimer = 0.6f;
            StartCoroutine(SwipeAttack());
        }

        IEnumerator SwipeAttack()
        {
            yield return new WaitForSeconds(0.25f);
            if (PlayerInRange(swipeRange))
            {
                var ph = player.GetComponent<Player.PlayerHealth>();
                ph?.TakeDamage(Mathf.RoundToInt(swipeDamage));
                player.GetComponent<Player.PlayerController>()?.ApplyKnockback(DirectionToPlayer(), 8f);
            }
        }

        void UpdateSwipe()
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                state = CatState.Recover;
                stateTimer = 1f;
            }
        }

        void UpdateRecover()
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                state = CatState.Stalk;
                PickNewStalkTarget();
            }
        }

        void PickNewStalkTarget()
        {
            if (player == null) return;
            Vector2 rnd = Random.insideUnitCircle * 12f;
            stalkTarget = player.position + new Vector3(rnd.x, 0f, rnd.y);
        }

        IEnumerator WiggleBody(float amount, float speed)
        {
            if (catBody == null) yield break;
            Vector3 baseLocal = catBody.localPosition;
            float t = 0f;
            while (t < 1.2f)
            {
                t += Time.deltaTime;
                catBody.localPosition = baseLocal + Vector3.right * Mathf.Sin(t * speed) * amount;
                yield return null;
            }
            catBody.localPosition = baseLocal;
        }

        public void SetupVisuals(Transform body, Transform paw)
        {
            catBody = body;
            pawCollider = paw;
        }
    }
}
