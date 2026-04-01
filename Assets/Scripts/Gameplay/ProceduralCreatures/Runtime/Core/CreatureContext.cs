using UnityEngine;

namespace ProceduralCreatures
{
    public sealed class CreatureContext
    {
        public readonly Transform CreatureTransform;
        public readonly ICreatureRigAdapter Rig;
        public readonly IMotionSource Motion;

        // Parámetros runtime (se pueden alimentar de varios SO)
        public float Speed01;
        public float TurnAmount;
        public Vector3 SmoothedVelocity;
        public float DeltaTime;

        public CreatureContext(Transform creatureTransform, ICreatureRigAdapter rig, IMotionSource motion)
        {
            CreatureTransform = creatureTransform;
            Rig = rig;
            Motion = motion;
        }
    }
}
