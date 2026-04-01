using UnityEngine;

namespace ProceduralCreatures
{
    public interface IMotionSource
    {
        Vector3 VelocityWorld { get; }
        Vector3 ForwardWorld { get; }
        bool IsGrounded { get; }
    }

    public interface ICreatureRigAdapter
    {
        Transform Root { get; }

        // Opcional: para módulos genéricos
        Transform Head { get; }
        Transform[] SpineChain { get; }
        Transform TailRoot { get; }

        // Para cuadrúpedos (si aplica)
        Transform FrontLeftFoot { get; }
        Transform FrontRightFoot { get; }
        Transform BackLeftFoot { get; }
        Transform BackRightFoot { get; }

        // Hook para validar que está bien seteado
        bool Validate(out string error);
    }

    public interface ICreatureModule
    {
        int Order { get; }
        void Initialize(CreatureContext ctx);
        void Tick(float dt);
        void LateTick(float dt);
        void Shutdown();
    }
}
