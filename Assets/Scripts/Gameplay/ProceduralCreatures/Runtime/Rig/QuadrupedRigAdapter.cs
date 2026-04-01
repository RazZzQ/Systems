using UnityEngine;


namespace ProceduralCreatures
{
    public sealed class QuadrupedRigAdapter : MonoBehaviour, ICreatureRigAdapter
    {
        [Header("Bones")]
        [SerializeField] private Transform root;
        [SerializeField] private Transform head;
        [SerializeField] private Transform[] spineChain;
        [SerializeField] private Transform tailRoot;

        [Header("Feet")]
        [SerializeField] private Transform frontLeftFoot;
        [SerializeField] private Transform frontRightFoot;
        [SerializeField] private Transform backLeftFoot;
        [SerializeField] private Transform backRightFoot;

        public Transform Root => root;
        public Transform Head => head;
        public Transform[] SpineChain => spineChain;
        public Transform TailRoot => tailRoot;

        public Transform FrontLeftFoot => frontLeftFoot;
        public Transform FrontRightFoot => frontRightFoot;
        public Transform BackLeftFoot => backLeftFoot;
        public Transform BackRightFoot => backRightFoot;

        public bool Validate(out string error)
        {
            if (root == null) { error = "Root no asignado en QuadrupedRigAdapter."; return false; }
            if (frontLeftFoot == null || frontRightFoot == null || backLeftFoot == null || backRightFoot == null)
            {
                error = "Faltan feet transforms (FL/FR/BL/BR).";
                return false;
            }
            error = null;
            return true;
        }
    }
}
