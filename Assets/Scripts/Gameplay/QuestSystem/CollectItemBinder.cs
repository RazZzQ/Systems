using UnityEngine;

public class CollectItemBinder : IObjectiveBinder
{
    private readonly string itemId;
    private readonly QuestJournal journal;
    private readonly QuestInstance instance;
    private readonly int objectiveIndex;

    public CollectItemBinder(string itemId, QuestJournal journal, QuestInstance instance, int objectiveIndex)
    {
        this.itemId = itemId;
        this.journal = journal;
        this.instance = instance;
        this.objectiveIndex = objectiveIndex;
    }

    public void Bind(GameEventHub hub)
    {
        hub.onItemAdded.AddListener(OnItemAdded);
    }

    public void Unbind(GameEventHub hub)
    {
        hub.onItemAdded.RemoveListener(OnItemAdded);
    }

    private void OnItemAdded(string id, int amount)
    {
        if (instance.status != QuestStatus.Active) return;
        if (id != itemId) return;

        journal.AddObjectiveProgress(instance, objectiveIndex, amount);
    }
}
