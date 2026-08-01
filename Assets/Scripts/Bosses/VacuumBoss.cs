using System.Collections;
using UnityEngine;

namespace GiantWorld.Bosses
{
    /// <summary>
    /// Vacuum Cleaner — roams and creates a deadly suction vortex.
    /// </summary>
    public class VacuumBoss : BossBase
    {
        enum VacuumState { Patrol, Charge, Suck, Overheat }

        [SerializeField] float patrolSpeed = 5f;
        [SerializeField] float suckRadius = 14f;
        [SerializeField] float suckForce = 18f;
        [SerializeField] float suckDamagePerSecond = 12f;

        VacuumState state = VacuumState.Patrol;
        Vector3[] patrolPoints;
        int patrolIndex;
        float stateTimer;
        Transform nozzle;
        ParticleSystem dustParticles;

        protected override void Awake()
        {
            bossName = "Vacuum Cleaner";
            bossType = Core.BossType.Vacuum;
            maxHealth = 140f;
            base.Awake();
        }

        public void SetPatrolPoints(Vector3[] points) => patrolPoints = points;

        protected override void OnBossFightStart()
        {
            state = VacuumState.Patrol;
            patrolIndex = 0;
            if (dustParticles != null) dustParticles.Play();
        }

        void Update()
        {
            if (!isActive || isDefeated || player == null) return;

            switch (state)
            {
                case VacuumState.Patrol: UpdatePatrol(); break;
                case VacuumState.Charge: UpdateCharge(); break;
                case VacuumState.Suck: UpdateSuck(); break;
                case VacuumState.Overheat: UpdateOverheat(); break;
            }
        }

        void UpdatePatrol()
        {
            if (patrolPoints == null || patrolPoints.Length == 0) return;

            Vector3 target = patrolPoints[patrolIndex];
            Vector3 dir = target - transform.position;
            dir.y = 0f;

            if (dir.magnitude < 1.5f)
            {
                patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
                if (PlayerInRange(suckRadius * 1.2f))
                {
                    state = VacuumState.Charge;
                    stateTimer = 1f;
                }
                return;
            }

            transform.position += dir.normalized * patrolSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 5f);
        }

        void UpdateCharge()
        {
            stateTimer -= Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(DirectionToPlayer()), Time.deltaTime * 8f);
            if (stateTimer <= 0f)
            {
                state = VacuumState.Suck;
                stateTimer = 4f;
            }
        }

        void UpdateSuck()
        {
            stateTimer -= Time.deltaTime;

            Vector3 toNozzle = nozzle != null ? nozzle.position : transform.position + transform.forward * 2f;
            Vector3 pullDir = (toNozzle - player.position);
            float dist = pullDir.magnitude;

            if (dist < suckRadius)
            {
                pullDir.y = 0f;
                var pc = player.GetComponent<Player.PlayerController>();
                pc?.ApplyKnockback(pullDir.normalized, suckForce * Time.deltaTime);

                if (dist < suckRadius * 0.45f)
                {
                    var ph = player.GetComponent<Player.PlayerHealth>();
                    ph?.TakeDamage(Mathf.RoundToInt(suckDamagePerSecond * Time.deltaTime));
                }
            }

            transform.position += DirectionToPlayer() * (patrolSpeed * 0.4f) * Time.deltaTime;

            if (stateTimer <= 0f)
            {
                state = VacuumState.Overheat;
                stateTimer = 2f;
                if (dustParticles != null) dustParticles.Stop();
            }
        }

        void UpdateOverheat()
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                state = VacuumState.Patrol;
                if (dustParticles != null) dustParticles.Play();
            }
        }

        public void SetupVisuals(Transform nozzleTransform, ParticleSystem particles)
        {
            nozzle = nozzleTransform;
            dustParticles = particles;
        }
    }
}
