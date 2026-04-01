using UnityEngine;

namespace ProceduralCreatures
{
    [CreateAssetMenu(menuName = "ProceduralCreatures/Gaits/Quadruped Wolf Gait")]
    public sealed class QuadrupedWolfGaitSO : BaseGaitSO
    {
        [Header("Cycle")]
        [Tooltip("Pasos por segundo al correr (Speed01=1).")]
        public float maxCadence = 3.2f;

        [Tooltip("Pasos por segundo al caminar (Speed01~0.2).")]
        public float minCadence = 1.4f;

        [Range(0.25f, 0.75f)]
        [Tooltip("Porción del ciclo que el pie está en apoyo (más alto = menos swing).")]
        public float stanceFraction = 0.6f;

        [Header("Stride")]
        [Tooltip("Zancada mínima en metros.")]
        public float minStride = 0.25f;

        [Tooltip("Zancada máxima en metros.")]
        public float maxStride = 0.65f;

        [Tooltip("Altura del paso al correr.")]
        public float stepHeight = 0.12f;

        [Tooltip("Cuánto se adelanta el target según velocidad.")]
        public float strideForwardBias = 1.0f;

        [Header("Foot Placement")]
        public LayerMask groundMask = ~0;
        public float raycastHeight = 0.6f;
        public float raycastDistance = 1.4f;

        [Tooltip("Separación lateral extra para estabilidad (0 = usa la del rig).")]
        public float lateralSpread = 0.0f;

        [Tooltip("Suavizado del movimiento del hueso del pie.")]
        public float footPosSmooth = 18f;

        [Tooltip("Suavizado de rotación del pie hacia el suelo (si lo implementas luego).")]
        public float footRotSmooth = 18f;

        [Header("Anti Sliding")]
        [Tooltip("Si el pie está planted, cuánto sigue el cuerpo (0 = totalmente fijo).")]
        [Range(0f, 0.25f)]
        public float plantedFollow = 0.05f;

        [Tooltip("Umbral para forzar un step si el pie se estira demasiado.")]
        public float maxFootStretch = 0.45f;

        [Header("Phase Offsets (Trot default)")]
        [Tooltip("Offsets por pata en el ciclo (0..1). Trot típico: diagonales juntas.")]
        public float phaseFL = 0.0f;
        public float phaseFR = 0.5f;
        public float phaseBL = 0.5f;
        public float phaseBR = 0.0f;

        [Header("Debug")]
        public bool drawDebug;

        public override ICreatureModule CreateRuntime() => new Runtime(this);

        private sealed class Runtime : ICreatureModule
        {
            private readonly QuadrupedWolfGaitSO so;
            private CreatureContext ctx;

            private FootState fl, fr, bl, br;
            private float cycle; // avanza con cadence

            public int Order => 50; // temprano: define pies/base

            public Runtime(QuadrupedWolfGaitSO so) => this.so = so;

            public void Initialize(CreatureContext ctx)
            {
                this.ctx = ctx;

                fl = CreateFoot(ctx.Rig.FrontLeftFoot, so.phaseFL);
                fr = CreateFoot(ctx.Rig.FrontRightFoot, so.phaseFR);
                bl = CreateFoot(ctx.Rig.BackLeftFoot, so.phaseBL);
                br = CreateFoot(ctx.Rig.BackRightFoot, so.phaseBR);

                // Inicializa planted en posición actual al suelo
                SnapFootToGround(ref fl);
                SnapFootToGround(ref fr);
                SnapFootToGround(ref bl);
                SnapFootToGround(ref br);

                // Pega huesos a planted inicial
                ApplyFoot(ref fl, 1f);
                ApplyFoot(ref fr, 1f);
                ApplyFoot(ref bl, 1f);
                ApplyFoot(ref br, 1f);
            }

            public void Tick(float dt)
            {
                // Cadence según speed
                float cadence = Mathf.Lerp(so.minCadence, so.maxCadence, ctx.Speed01);
                cycle += cadence * dt;

                // Stride según speed
                float stride = Mathf.Lerp(so.minStride, so.maxStride, ctx.Speed01);

                // Dirección planar de movimiento (si está casi quieto, usa forward)
                Vector3 vel = ctx.SmoothedVelocity;
                Vector3 planar = Vector3.ProjectOnPlane(vel, Vector3.up);
                Vector3 moveDir = planar.sqrMagnitude > 0.001f ? planar.normalized : ctx.CreatureTransform.forward;

                // Actualiza patas
                UpdateFoot(ref fl, dt, stride, moveDir, so.phaseFL);
                UpdateFoot(ref fr, dt, stride, moveDir, so.phaseFR);
                UpdateFoot(ref bl, dt, stride, moveDir, so.phaseBL);
                UpdateFoot(ref br, dt, stride, moveDir, so.phaseBR);

                if (so.drawDebug) DebugDraw();
            }

            public void LateTick(float dt)
            {
                // Aplica en LateTick para evitar pelear con otros updates
                ApplyFoot(ref fl, dt);
                ApplyFoot(ref fr, dt);
                ApplyFoot(ref bl, dt);
                ApplyFoot(ref br, dt);
            }

            public void Shutdown() { }

            private FootState CreateFoot(Transform bone, float phaseOffset)
            {
                return new FootState
                {
                    FootBone = bone,
                    Phase = phaseOffset,
                    IsSwing = false,
                    SwingT = 0f,
                    PlantedWorldPos = bone.position,
                    TargetWorldPos = bone.position,
                    SwingStartWorldPos = bone.position
                };
            }

            private void UpdateFoot(ref FootState f, float dt, float stride, Vector3 moveDir, float phaseOffset)
            {
                if (f.FootBone == null) return;

                float p = GaitMath.Repeat01(cycle + phaseOffset);
                f.Phase = p;

                bool shouldSwingNow = p > so.stanceFraction; // stance primero, luego swing
                float swingWindow = Mathf.Max(0.0001f, 1f - so.stanceFraction);

                // Distancia de “estiramiento” para forzar step
                float stretch = Vector3.Distance(f.PlantedWorldPos, f.FootBone.position);

                // Transición planted -> swing
                if (!f.IsSwing && (shouldSwingNow || stretch > so.maxFootStretch))
                {
                    f.IsSwing = true;
                    f.SwingT = 0f;
                    f.SwingStartWorldPos = f.PlantedWorldPos;

                    // Calcula candidato de target hacia adelante
                    Vector3 desired = ComputeDesiredFootTarget(f, stride, moveDir);

                    // Raycast al suelo
                    f.TargetWorldPos = RaycastToGround(desired, out _);
                }

                if (f.IsSwing)
                {
                    // progreso swing 0..1 dentro de la ventana
                    float localT = (p - so.stanceFraction) / swingWindow;
                    f.SwingT = Mathf.Clamp01(localT);

                    // Si termina swing -> planted
                    if (f.SwingT >= 1f - 0.0001f)
                    {
                        f.IsSwing = false;
                        f.SwingT = 0f;
                        f.PlantedWorldPos = f.TargetWorldPos; // “planta”
                    }
                }
                else
                {
                    // Anti sliding: el pie planted puede seguir un poquito al cuerpo
                    Vector3 follow = ctx.SmoothedVelocity * (so.plantedFollow * dt);
                    f.PlantedWorldPos += follow;

                    // Asegura que planted siga el suelo (por slopes)
                    f.PlantedWorldPos = RaycastToGround(f.PlantedWorldPos, out _);
                }
            }

            private Vector3 ComputeDesiredFootTarget(in FootState f, float stride, Vector3 moveDir)
            {
                // Base: posición actual planted
                Vector3 basePos = f.PlantedWorldPos;

                // Predicción forward
                float forward = stride * so.strideForwardBias;

                // Separación lateral opcional
                float side = 0f;
                if (so.lateralSpread > 0.0001f)
                {
                    // Determina “derecha” del cuerpo
                    Vector3 right = ctx.CreatureTransform.right;
                    // heurística: pies derechos se van a +right, izquierdos a -right
                    bool isRight = (f.FootBone == ctx.Rig.FrontRightFoot || f.FootBone == ctx.Rig.BackRightFoot);
                    side = so.lateralSpread * (isRight ? 1f : -1f);
                    basePos += right * side;
                }

                // target adelante
                Vector3 desired = basePos + moveDir * forward;

                return desired;
            }

            private Vector3 RaycastToGround(Vector3 worldPos, out Vector3 normal)
            {
                Vector3 origin = worldPos + Vector3.up * so.raycastHeight;
                if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, so.raycastDistance, so.groundMask, QueryTriggerInteraction.Ignore))
                {
                    normal = hit.normal;
                    return hit.point;
                }

                normal = Vector3.up;
                return worldPos;
            }

            private void SnapFootToGround(ref FootState f)
            {
                if (f.FootBone == null) return;
                f.PlantedWorldPos = RaycastToGround(f.FootBone.position, out _);
                f.TargetWorldPos = f.PlantedWorldPos;
                f.SwingStartWorldPos = f.PlantedWorldPos;
            }

            private void ApplyFoot(ref FootState f, float dt)
            {
                if (f.FootBone == null) return;

                Vector3 desired;

                if (f.IsSwing)
                {
                    // Interp + arco
                    float t = f.SwingT;
                    Vector3 a = f.SwingStartWorldPos;
                    Vector3 b = f.TargetWorldPos;

                    desired = Vector3.Lerp(a, b, t);
                    desired += Vector3.up * (so.stepHeight * GaitMath.Parabola01(t));
                }
                else
                {
                    desired = f.PlantedWorldPos;
                }

                // Suavizado al hueso
                float k = GaitMath.ExpDamp(dt, so.footPosSmooth);
                f.FootBone.position = Vector3.Lerp(f.FootBone.position, desired, k);
            }

            private void DebugDraw()
            {
                DrawFoot(fl, Color.cyan);
                DrawFoot(fr, Color.magenta);
                DrawFoot(bl, Color.yellow);
                DrawFoot(br, Color.green);
            }

            private void DrawFoot(in FootState f, Color c)
            {
                if (f.FootBone == null) return;
                Debug.DrawLine(f.FootBone.position, f.PlantedWorldPos, c);
                Debug.DrawLine(f.PlantedWorldPos, f.TargetWorldPos, c);
            }
        }
    }
}
