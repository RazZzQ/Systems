using UnityEngine;

namespace ProceduralCreatures
{
    public sealed class RigidbodyMotionSource : MonoBehaviour, IMotionSource
    {
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Transform forwardReference;
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField] private float groundCheckDistance = 0.2f;

        public Vector3 VelocityWorld => rb != null ? rb.linearVelocity : Vector3.zero;

        public Vector3 ForwardWorld =>
            forwardReference != null ? forwardReference.forward : transform.forward;

        public bool IsGrounded
        {
            get
            {
                var origin = transform.position + Vector3.up * 0.05f;
                return Physics.Raycast(origin, Vector3.down, groundCheckDistance + 0.05f, groundMask, QueryTriggerInteraction.Ignore);
            }
        }

        void Reset()
        {
            rb = GetComponent<Rigidbody>();
            forwardReference = transform;
        }
    }
}