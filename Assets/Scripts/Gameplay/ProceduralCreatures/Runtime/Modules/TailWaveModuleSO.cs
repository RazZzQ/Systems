using UnityEngine;

namespace ProceduralCreatures
{
    [CreateAssetMenu(menuName = "ProceduralCreatures/Modules/Tail Wave")]
    public sealed class TailWaveModuleSO : BaseCreatureModuleSO
    {
        public float yawDegrees = 18f;
        public float waveSpeed = 5f;
        public float speedInfluence = 1f;
        public float turnInfluence = 1.2f;
        public float smooth = 10f;

        public override ICreatureModule CreateRuntime() => new Runtime(this);

        private sealed class Runtime : ICreatureModule
        {
            private readonly TailWaveModuleSO so;
            private CreatureContext ctx;

            private Quaternion baseLocal;
            private Quaternion current;

            public int Order => 140;

            public Runtime(TailWaveModuleSO so) => this.so = so;

            public void Initialize(CreatureContext ctx)
            {
                this.ctx = ctx;
                if (ctx.Rig.TailRoot != null)
                {
                    baseLocal = ctx.Rig.TailRoot.localRotation;
                    current = baseLocal;
                }
            }

            public void Tick(float dt) { }

            public void LateTick(float dt)
            {
                var tail = ctx.Rig.TailRoot;
                if (tail == null) return;

                float k = 1f - Mathf.Exp(-so.smooth * dt);

                float speed = ctx.Speed01;
                float turn = ctx.TurnAmount;

                float wave = Mathf.Sin(Time.time * so.waveSpeed) * so.yawDegrees;
                wave *= (1f + speed * so.speedInfluence);
                wave += (turn * so.yawDegrees * so.turnInfluence);

                Quaternion target = baseLocal * Quaternion.Euler(0f, wave, 0f);
                current = Quaternion.Slerp(current, target, k);
                tail.localRotation = current;
            }

            public void Shutdown() { }
        }
    }
}