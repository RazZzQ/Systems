using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ProceduralCreatures
{
    public sealed class ProceduralCreature : MonoBehaviour
    {
        [Header("Sources")]
        [SerializeField] private MonoBehaviour rigAdapterBehaviour;   // ICreatureRigAdapter
        [SerializeField] private MonoBehaviour motionSourceBehaviour; // IMotionSource

        [Header("Config")]
        [SerializeField] private List<BaseCreatureModuleSO> modules = new();
        [SerializeField] private BaseGaitSO gait;

        [Header("Diagnostics")]
        [SerializeField] private bool diagnostics = true;

        [Header("Events")]
        public UnityEvent OnInitialized;
        public UnityEvent<string> OnValidationError;

        private ICreatureRigAdapter rig;
        private IMotionSource motion;
        private CreatureContext ctx;

        private readonly List<ICreatureModule> runtimeModules = new();
        private bool initialized;

        void OnEnable()
        {
            initialized = false;
            runtimeModules.Clear();

            if (rigAdapterBehaviour == null || motionSourceBehaviour == null)
            {
                DisableWithError("RigAdapterBehaviour o MotionSourceBehaviour no están asignados.");
                return;
            }

            rig = rigAdapterBehaviour as ICreatureRigAdapter;
            motion = motionSourceBehaviour as IMotionSource;

            if (rig == null)
            {
                DisableWithError(
                    $"RigAdapter no implementa ICreatureRigAdapter. " +
                    $"Asignado: {(rigAdapterBehaviour ? rigAdapterBehaviour.GetType().FullName : "NULL")}"
                );
                return;
            }

            if (motion == null)
            {
                DisableWithError(
                    $"MotionSource no implementa IMotionSource. " +
                    $"Asignado: {(motionSourceBehaviour ? motionSourceBehaviour.GetType().FullName : "NULL")}"
                );
                return;
            }

            ctx = new CreatureContext(transform, rig, motion);

            BuildModules();
            initialized = true;

            if (diagnostics)
            {
                Debug.Log(
                    $"[ProceduralCreature] Init OK on '{name}'. " +
                    $"Modules running: {runtimeModules.Count}. " +
                    $"TailRoot={(rig.TailRoot ? rig.TailRoot.name : "NULL")}, " +
                    $"Head={(rig.Head ? rig.Head.name : "NULL")}"
                );
            }

            OnInitialized?.Invoke();
        }

        void Update()
        {
            if (!initialized) return;

            float dt = Time.deltaTime;
            ctx.DeltaTime = dt;

            ctx.SmoothedVelocity = Vector3.Lerp(
                ctx.SmoothedVelocity,
                motion.VelocityWorld,
                1f - Mathf.Exp(-10f * dt)
            );

            float speed = ctx.SmoothedVelocity.magnitude;

            float maxSpeed = gait != null ? Mathf.Max(0.01f, gait.MaxSpeed) : 5f;
            ctx.Speed01 = Mathf.Clamp01(speed / maxSpeed);

            Vector3 fwd = motion.ForwardWorld.sqrMagnitude > 0.001f ? motion.ForwardWorld : transform.forward;
            float turn = Vector3.SignedAngle(transform.forward, fwd, Vector3.up);
            ctx.TurnAmount = Mathf.Clamp(turn / 90f, -1f, 1f);

            for (int i = 0; i < runtimeModules.Count; i++)
                runtimeModules[i].Tick(dt);
        }

        void LateUpdate()
        {
            if (!initialized) return;

            float dt = Time.deltaTime;
            for (int i = 0; i < runtimeModules.Count; i++)
                runtimeModules[i].LateTick(dt);
        }

        void OnDisable()
        {
            for (int i = 0; i < runtimeModules.Count; i++)
            {
                try { runtimeModules[i].Shutdown(); }
                catch { }
            }

            runtimeModules.Clear();
            ctx = null;
            rig = null;
            motion = null;
            initialized = false;
        }

        private void BuildModules()
        {
            runtimeModules.Clear();

            if (gait != null)
            {
                var gaitRuntime = gait.CreateRuntime();
                gaitRuntime.Initialize(ctx);
                runtimeModules.Add(gaitRuntime);
            }

            foreach (var so in modules)
            {
                if (so == null) continue;
                var m = so.CreateRuntime();
                m.Initialize(ctx);
                runtimeModules.Add(m);
            }

            runtimeModules.Sort((a, b) => a.Order.CompareTo(b.Order));
        }

        private void DisableWithError(string msg)
        {
            if (diagnostics) Debug.LogWarning($"[ProceduralCreature] Disabled: {msg}", this);
            OnValidationError?.Invoke(msg);
            enabled = false;
        }
    }
}
