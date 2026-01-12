using UnityEngine;

[System.Serializable]
public class QuestInstance
{
    public QuestDefinitionSO definition;
    public QuestStatus status;

    //progreso por objetivo
    public int[] progress;

    public QuestInstance(QuestDefinitionSO def, QuestStatus initialStatus)
    {
        definition = def;
        status = initialStatus;
        progress = new int[def.objectives.Count];
        for(int i = 0; i < progress.Length; i++)
            progress[i] = def.objectives[i].GetInitialProgress();
    }

    public bool IsAllObjectivesComplete()
    {
        for(int i = 0; i < definition.objectives.Count; i++)
        {
            if(!definition.objectives[i].IsComplete(progress[i]))
                return false;
        }
        return true;
    }
}
