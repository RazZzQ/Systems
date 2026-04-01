using UnityEngine;

namespace ProceduralCreatures
{
    [CreateAssetMenu(menuName = "ProceduralCreatures/Modules/Head Look")]
    public sealed class HeadLookModuleSO : BaseCreatureModuleSO
    {
        [Header("Settings")]
        public float weight = 0.7f;
        public float maxYaw = 35f;
        public float maxPitch = 20f;
        public float smooth = 10f;

        [Header("Target")]
        public Transform targetOverride;

        public override ICreatureModule CreateRuntime() => new Runtime(this);

        private sealed class Runtime : ICreatureModule
        {
            private readonly HeadLookModuleSO so;
            private CreatureContext ctx;

            private Quaternion headBaseLocalRot;
            private Quaternion current;

            public int Order => 200; // después de gait si quieres

            public Runtime(HeadLookModuleSO so) => this.so = so;

            public void Initialize(CreatureContext ctx)
            {
                this.ctx = ctx;
                if (ctx.Rig.Head != null)
                {
                    headBaseLocalRot = ctx.Rig.Head.localRotation;
                    current = headBaseLocalRot;
                }
            }

            public void Tick(float dt) { }

            public void LateTick(float dt)
            {
                var head = ctx.Rig.Head;
                if (head == null) return;

                Transform target = so.targetOverride;
                if (target == null) return;

                Vector3 to = (target.position - head.position);
                if (to.sqrMagnitude < 0.0001f) return;

                // Convertimos dirección a espacio local del padre de la cabeza
                Transform parent = head.parent != null ? head.parent : ctx.CreatureTransform;
                Vector3 localDir = parent.InverseTransformDirection(to.normalized);

                // Calcula yaw/pitch local
                float yaw = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;
                float pitch = -Mathf.Asin(Mathf.Clamp(localDir.y, -1f, 1f)) * Mathf.Rad2Deg;

                yaw = Mathf.Clamp(yaw, -so.maxYaw, so.maxYaw);
                pitch = Mathf.Clamp(pitch, -so.maxPitch, so.maxPitch);

                Quaternion targetLocal = headBaseLocalRot * Quaternion.Euler(pitch * so.weight, yaw * so.weight, 0f);
                current = Quaternion.Slerp(current, targetLocal, 1f - Mathf.Exp(-so.smooth * dt));
                head.localRotation = current;
            }

            public void Shutdown() { }
        }
    }
}
