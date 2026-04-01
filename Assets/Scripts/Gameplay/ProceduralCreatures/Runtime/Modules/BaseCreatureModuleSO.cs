using UnityEngine;

namespace ProceduralCreatures
{
    public abstract class BaseCreatureModuleSO : ScriptableObject
    {
        public abstract ICreatureModule CreateRuntime();
    }
}
