using UnityEngine;

namespace ProceduralCreatures
{
    [CreateAssetMenu(menuName = "ProceduralCreatures/Modules/Breathing")]
    public sealed class BreathingModuleSO : BaseCreatureModuleSO
    {
        public float baseScale = 1.0f;
        public float breatheAmount = 0.015f;
        public float breatheSpeed = 1.2f;
        public float speedInfluence = 1.5f;

        public override ICreatureModule CreateRuntime() => new Runtime(this);

        private sealed class Runtime : ICreatureModule
        {
            private readonly BreathingModuleSO so;
            private CreatureContext ctx;
            private Vector3 baseLocalScale;

            public int Order => 300; // tarde, aditivo

            public Runtime(BreathingModuleSO so) => this.so = so;

            public void Initialize(CreatureContext ctx)
            {
                this.ctx = ctx;
                baseLocalScale = ctx.CreatureTransform.localScale;
            }

            public void Tick(float dt)
            {
                float speed = ctx.Speed01;
                float s = so.breatheSpeed * (1f + speed * so.speedInfluence);
                float a = so.breatheAmount * (1f + speed * 0.8f);

                float breathe = Mathf.Sin(Time.time * s) * a;
                Vector3 target = baseLocalScale * so.baseScale + new Vector3(0f, breathe, 0f);

                // Suave, sin “pompear”
                ctx.CreatureTransform.localScale = Vector3.Lerp(ctx.CreatureTransform.localScale, target, 1f - Mathf.Exp(-6f * dt));
            }

            public void LateTick(float dt) { }
            public void Shutdown() { }
        }
    }
}

