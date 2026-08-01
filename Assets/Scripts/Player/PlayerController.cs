using UnityEngine;
using UnityEngine.InputSystem;

namespace GiantWorld.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerHealth))]
    public class PlayerController : MonoBehaviour
    {
        CharacterController controller;
        PlayerHealth health;
        PlayerCombat combat;
        Transform cameraTransform;

        Vector3 velocity;
        float coyoteTime;
        float attackLockTimer;

        public bool CanMove => health != null && health.IsAlive && attackLockTimer <= 0f;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            health = GetComponent<PlayerHealth>();
            combat = GetComponent<PlayerCombat>();
        }

        public void BindCamera(Transform cam)
        {
            cameraTransform = cam;
        }

        void Update()
        {
            if (attackLockTimer > 0f)
                attackLockTimer -= Time.deltaTime;

            if (!health.IsAlive) return;

            HandleMovement();
            HandleAttackInput();
        }

        void HandleMovement()
        {
            if (!CanMove) return;

            Keyboard kb = Keyboard.current;
            if (kb == null) return;

            float h = 0f, v = 0f;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) h -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) h += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) v -= 1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) v += 1f;

            Vector3 input = new Vector3(h, 0f, v);
            if (input.sqrMagnitude > 1f) input.Normalize();

            Vector3 moveDir = Vector3.zero;
            if (cameraTransform != null && input.sqrMagnitude > 0.01f)
            {
                Vector3 camForward = cameraTransform.forward;
                Vector3 camRight = cameraTransform.right;
                camForward.y = 0f;
                camRight.y = 0f;
                camForward.Normalize();
                camRight.Normalize();
                moveDir = camForward * input.z + camRight * input.x;
            }
            else if (input.sqrMagnitude > 0.01f)
            {
                moveDir = input;
            }

            bool sprint = kb.leftShiftKey.isPressed;
            float speed = sprint ? Core.GameConstants.PlayerSprintSpeed : Core.GameConstants.PlayerMoveSpeed;

            if (moveDir.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(moveDir),
                    Time.deltaTime * 12f);
            }

            controller.Move(moveDir * speed * Time.deltaTime);

            bool grounded = controller.isGrounded;
            if (grounded)
            {
                coyoteTime = 0.15f;
                if (velocity.y < 0f) velocity.y = -2f;
            }
            else
            {
                coyoteTime -= Time.deltaTime;
            }

            if ((kb.spaceKey.wasPressedThisFrame) && coyoteTime > 0f)
                velocity.y = Core.GameConstants.PlayerJumpForce;

            velocity.y += Physics.gravity.y * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }

        void HandleAttackInput()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.wasPressedThisFrame) return;
            if (combat == null || !combat.TryAttack()) return;
            attackLockTimer = 0.15f;
        }

        public void ApplyKnockback(Vector3 direction, float force)
        {
            if (!health.IsAlive) return;
            velocity += direction.normalized * force;
        }
    }
}
