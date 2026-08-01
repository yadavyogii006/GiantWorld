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
            var mouse = UnityEngine.InputSystem.Mouse.current;
            if (mouse == null) return;

            if (mouse.rightButton.isPressed)
            {
                yaw += mouse.delta.x.ReadValue() * 0.15f;
                pitch -= mouse.delta.y.ReadValue() * 0.12f;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            }
        }
    }
}
