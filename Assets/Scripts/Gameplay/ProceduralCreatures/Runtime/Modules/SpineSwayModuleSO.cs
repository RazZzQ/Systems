using UnityEngine;

namespace ProceduralCreatures
{
    [CreateAssetMenu(menuName = "ProceduralCreatures/Modules/Spine Sway")]
    public sealed class SpineSwayModuleSO : BaseCreatureModuleSO
    {
        [Header("Sway")]
        public float yawSwayDegrees = 8f;      // sway lateral al correr
        public float pitchSwayDegrees = 6f;    // inclinación al acelerar/frenar
        public float turnInfluence = 10f;      // extra por giro
        public float smooth = 10f;

        public override ICreatureModule CreateRuntime() => new Runtime(this);

        private sealed class Runtime : ICreatureModule
        {
            private readonly SpineSwayModuleSO so;
            private CreatureContext ctx;
            private Quaternion[] baseLocal;
            private Quaternion[] currentLocal;

            public int Order => 120;

            public Runtime(SpineSwayModuleSO so) => this.so = so;

            public void Initialize(CreatureContext ctx)
            {
                this.ctx = ctx;
                var chain = ctx.Rig.SpineChain;
                if (chain == null || chain.Length == 0) return;

                baseLocal = new Quaternion[chain.Length];
                currentLocal = new Quaternion[chain.Length];

                for (int i = 0; i < chain.Length; i++)
                {
                    baseLocal[i] = chain[i].localRotation;
                    currentLocal[i] = baseLocal[i];
                }
            }

            public void Tick(float dt) { }

            public void LateTick(float dt)
            {
                var chain = ctx.Rig.SpineChain;
                if (chain == null || chain.Length == 0) return;

                float k = 1f - Mathf.Exp(-so.smooth * dt);

                // Cantidades base
                float speed = ctx.Speed01;
                float turn = ctx.TurnAmount;

                // Distribuye a lo largo de la cadena (más en torso que en cadera)
                for (int i = 0; i < chain.Length; i++)
                {
                    float t = (chain.Length <= 1) ? 1f : (i / (float)(chain.Length - 1));
                    float w = Mathf.SmoothStep(0.2f, 1f, t);

                    float yaw = (so.yawSwayDegrees * speed * Mathf.Sin(Time.time * 8f)) + (so.turnInfluence * turn);
                    float pitch = (so.pitchSwayDegrees * speed * Mathf.Sin(Time.time * 6f + 1.2f));

                    Quaternion target = baseLocal[i] * Quaternion.Euler(pitch * w, yaw * w, 0f);
                    currentLocal[i] = Quaternion.Slerp(currentLocal[i], target, k);
                    chain[i].localRotation = currentLocal[i];
                }
            }

            public void Shutdown() { }
        }
    }
}
