using UnityEngine;

namespace GiantWorld.Player
{
    public class FollowCamera : MonoBehaviour
    {
        [SerializeField] Vector3 offset = new Vector3(0f, 4f, -6f);
        [SerializeField] float followSmooth = 8f;
        [SerializeField] float lookSmooth = 10f;
        [SerializeField] float minPitch = -15f;
        [SerializeField] float maxPitch = 55f;

        Transform target;
        float yaw;
        float pitch = 20f;

        public void SetTarget(Transform t)
        {
            target = t;
            if (target != null)
            {
                var pc = target.GetComponent<PlayerController>();
                pc?.BindCamera(transform);
            }
        }

        public void SnapToTarget()
        {
            if (target == null) return;
            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            transform.position = target.position + rot * offset;
            Vector3 lookTarget = target.position + Vector3.up * 1.2f;
            transform.rotation = Quaternion.LookRotation(lookTarget - transform.position);
        }

        void LateUpdate()
        {
            if (target == null) return;

            HandleOrbitInput();

            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 desired = target.position + rot * offset;
            transform.position = Vector3.Lerp(transform.position, desired, Time.deltaTime * followSmooth);

            Vector3 lookTarget = target.position + Vector3.up * 1.2f;
            Quaternion lookRot = Quaternion.LookRotation(lookTarget - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * lookSmooth);
        }

        void HandleOrbitInput()
        {
            if (!Input.GetMouseButton(1)) return;

            float mx = Input.GetAxis("Mouse X");
            float my = Input.GetAxis("Mouse Y");

#if ENABLE_INPUT_SYSTEM
            var mouse = UnityEngine.InputSystem.Mouse.current;
            if (mouse != null && mouse.rightButton.isPressed)
            {
                mx = mouse.delta.x.ReadValue() * 0.15f;
                my = mouse.delta.y.ReadValue() * 0.12f;
            }
#endif

            yaw += mx * 3f;
            pitch -= my * 3f;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }
    }
}
