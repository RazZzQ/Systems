using UnityEngine;


namespace ProceduralCreatures
{
    [CreateAssetMenu(menuName = "ProceduralCreatures/Modules/Additive Noise")]
    public sealed class AdditiveNoiseModuleSO : BaseCreatureModuleSO
    {
        public float headNoiseYaw = 2.5f;
        public float headNoisePitch = 1.5f;
        public float noiseSpeed = 1.8f;
        public float speedDamp = 0.7f; // menos ruido cuando corre
        public float smooth = 10f;

        public override ICreatureModule CreateRuntime() => new Runtime(this);

        private sealed class Runtime : ICreatureModule
        {
            private readonly AdditiveNoiseModuleSO so;
            private CreatureContext ctx;

            private Quaternion baseHead;
            private Quaternion currentHead;

            public int Order => 260;

            public Runtime(AdditiveNoiseModuleSO so) => this.so = so;

            public void Initialize(CreatureContext ctx)
            {
                this.ctx = ctx;
                if (ctx.Rig.Head != null)
                {
                    baseHead = ctx.Rig.Head.localRotation;
                    currentHead = baseHead;
                }
            }

            public void Tick(float dt) { }

            public void LateTick(float dt)
            {
                var head = ctx.Rig.Head;
                if (head == null) return;

                float k = 1f - Mathf.Exp(-so.smooth * dt);

                float speed = ctx.Speed01;
                float noiseWeight = Mathf.Lerp(1f, so.speedDamp, speed);

                float t = Time.time * so.noiseSpeed;
                float yaw = (Mathf.PerlinNoise(t, 0.1f) - 0.5f) * 2f * so.headNoiseYaw * noiseWeight;
                float pitch = (Mathf.PerlinNoise(0.2f, t) - 0.5f) * 2f * so.headNoisePitch * noiseWeight;

                Quaternion target = baseHead * Quaternion.Euler(pitch, yaw, 0f);
                currentHead = Quaternion.Slerp(currentHead, target, k);
                head.localRotation = currentHead;
            }

            public void Shutdown() { }
        }
    }
}
