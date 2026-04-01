using UnityEngine;

namespace ProceduralCreatures
{
    public struct FootState
    {
        public Transform FootBone;         // bone/transform del pie
        public Vector3 PlantedWorldPos;    // donde está “pegado”
        public Vector3 TargetWorldPos;     // destino del swing
        public Vector3 SwingStartWorldPos; // inicio del swing
        public float Phase;                // 0..1 ciclo
        public bool IsSwing;               // swing vs planted
        public float SwingT;               // 0..1 progreso swing
    }

    public static class GaitMath
    {
        public static float ExpDamp(float dt, float sharpness) => 1f - Mathf.Exp(-sharpness * dt);

        public static Vector3 ProjectOnPlane(Vector3 v, Vector3 n)
        {
            return v - Vector3.Dot(v, n) * n;
        }

        // Curva de arco simple para swing
        public static float Parabola01(float t) => 4f * t * (1f - t);

        public static float Repeat01(float t) => t - Mathf.Floor(t); // t mod 1
    }
}