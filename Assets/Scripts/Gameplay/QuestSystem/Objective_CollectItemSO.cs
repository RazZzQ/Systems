using UnityEngine;

[CreateAssetMenu(fileName = "Obj_CollecItem", menuName = "Quests/Objectives/Collect Item")]
public class Objective_CollectItemSO : ObjectiveDefinitionSO
{
    [Header("Item Id")]
    public string itemId = "item_apple";

    public override void Register(GameEventHub hub, QuestJournal journal, QuestInstance instance, int objectiveIndex)
    {
        var binderHost = journal.GetComponent<ObjectiveBinderHost>();
        if (!binderHost) binderHost = journal.gameObject.AddComponent<ObjectiveBinderHost>();

        binderHost.AddBinder(instance.definition.questId, objectiveIndex,
            new CollectItemBinder(itemId, journal, instance, objectiveIndex), hub);
    }

    public override void Unregister(GameEventHub hub, QuestJournal journal, QuestInstance instance, int objectiveIndex)
    {
        var binderHost = journal.GetComponent<ObjectiveBinderHost>();
        if (!binderHost) return;

        binderHost.RemoveBinder(instance.definition.questId, objectiveIndex, hub);
    }
}
