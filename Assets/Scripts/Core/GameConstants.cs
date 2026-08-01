using UnityEngine;

namespace GiantWorld.Core
{
    public enum GameState
    {
        Exploring,
        BossIntro,
        BossFight,
        BossDefeated,
        Victory,
        PlayerDead
    }

    public enum BossType
    {
        None,
        Cat,
        Vacuum,
        WashingMachine,
        Footsteps
    }

    public static class GameConstants
    {
        public const float PlayerScale = 0.08f;
        public const float WorldScale = 1f;
        public const int MaxPlayerHealth = 100;
        public const float PlayerMoveSpeed = 8f;
        public const float PlayerSprintSpeed = 14f;
        public const float PlayerJumpForce = 6f;
        public const float AttackDamage = 12f;
        public const float AttackCooldown = 0.45f;
        public const float AttackRange = 2.5f;
    }
}
