using UnityEngine;

//[CreateAssetMenu(fileName = "ObjectiveDefinitionSO", menuName = "Scriptable Objects/ObjectiveDefinitionSO")]
public abstract class ObjectiveDefinitionSO : ScriptableObject
{
    [Header("UI")]
    public string objectiveTitle = "Objetivo";
    [TextArea(1, 3)] public string objectiveDescription = "Haz algo";

    [Header("Progreso")]
    public int targetCount = 1;

    //Crea estado inicial (runtime) para este objetivo
    public virtual int GetInitialProgress() => 0;

    //Retorna true si esta completo con progreso actual
    public virtual bool IsComplete(int progress) => progress >= targetCount;

    //Logica para reaccionar a eventos del juego (via hub)
    public abstract void Register(GameEventHub Hub, QuestJournal journal, QuestInstance instance, int objectiveIndex);
    public abstract void Unregister(GameEventHub Hub, QuestJournal journal, QuestInstance instance, int objectiveIndex);
}
