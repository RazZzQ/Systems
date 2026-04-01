using UnityEngine;

namespace ProceduralCreatures
{
    public abstract class BaseGaitSO : ScriptableObject
    {
        public float MaxSpeed = 6f;
        public abstract ICreatureModule CreateRuntime();
    }
}
