using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable] public class QuestEvent : UnityEvent<QuestDefinitionSO> { }
[System.Serializable] public class QuestToastEvent : UnityEvent<string, string> { } 
public class QuestJournal : MonoBehaviour
{
    // (type, message) e.g. ("NEW", "Nueva misión: ...")
    [Header("References")]
    [SerializeField] private GameEventHub hub;

    [Header("Events")]
    public QuestEvent onQuestOffered;
    public QuestEvent onQuestAccepted;
    public QuestEvent onQuestRejected;
    public QuestEvent onQuestUpdated;
    public QuestEvent onQuestCompleted;

    public QuestToastEvent onToast; // UI “Nueva misión”, “Actualizada”, etc.

    // Runtime storage
    private readonly Dictionary<string, QuestInstance> all = new();
    private readonly List<string> activeIds = new();
    private readonly List<string> completedIds = new();
    private readonly List<string> offeredIds = new();

    public IReadOnlyList<string> ActiveIds => activeIds;
    public IReadOnlyList<string> CompletedIds => completedIds;
    public IReadOnlyList<string> OfferedIds => offeredIds;

    public bool HasQuest(string questId) => all.ContainsKey(questId);

    public QuestInstance GetInstance(string questId)
    {
        all.TryGetValue(questId, out var inst);
        return inst;
    }

    private void Awake()
    {
        if (!hub)
            Debug.LogWarning("[QuestJournal] No GameEventHub assigned.");
    }

    // ===== Offer / Accept / Reject =====

    public void OfferQuest(QuestDefinitionSO quest)
    {
        if (quest == null || string.IsNullOrEmpty(quest.questId)) return;

        // si ya existe (active/completed/rejected), no duplicar
        if (all.TryGetValue(quest.questId, out var existing))
        {
            // si está offered, re-emit UI
            if (existing.status == QuestStatus.Offered)
            {
                onQuestOffered?.Invoke(quest);
                onToast?.Invoke("OFFER", $"Misión disponible: {quest.title}");
            }
            return;
        }

        var inst = new QuestInstance(quest, QuestStatus.Offered);
        all.Add(quest.questId, inst);
        offeredIds.Add(quest.questId);

        onQuestOffered?.Invoke(quest);
        onToast?.Invoke("OFFER", $"Misión disponible: {quest.title}");

        if (quest.autoAcceptWhenOffered)
        {
            AcceptQuest(quest.questId);
        }
    }

    public void AcceptQuest(string questId)
    {
        if (!all.TryGetValue(questId, out var inst)) return;
        if (inst.status != QuestStatus.Offered) return;

        inst.status = QuestStatus.Active;
        offeredIds.Remove(questId);
        activeIds.Add(questId);

        // registrar objetivos
        RegisterObjectives(inst);

        onQuestAccepted?.Invoke(inst.definition);
        onToast?.Invoke("NEW", $"Nueva misión: {inst.definition.title}");
    }

    public void RejectQuest(string questId)
    {
        if (!all.TryGetValue(questId, out var inst)) return;
        if (inst.status != QuestStatus.Offered) return;

        inst.status = QuestStatus.Rejected;
        offeredIds.Remove(questId);

        onQuestRejected?.Invoke(inst.definition);
        onToast?.Invoke("REJECT", $"Misión rechazada: {inst.definition.title}");
    }

    // ===== Progress manipulation (called by objectives) =====

    public void AddObjectiveProgress(QuestInstance inst, int objectiveIndex, int amount)
    {
        if (inst == null || inst.status != QuestStatus.Active) return;
        if (objectiveIndex < 0 || objectiveIndex >= inst.progress.Length) return;

        int before = inst.progress[objectiveIndex];
        inst.progress[objectiveIndex] = Mathf.Max(0, before + amount);

        // clamp opcional
        var obj = inst.definition.objectives[objectiveIndex];
        if (inst.progress[objectiveIndex] > obj.targetCount)
            inst.progress[objectiveIndex] = obj.targetCount;

        // evento update
        onQuestUpdated?.Invoke(inst.definition);
        onToast?.Invoke("UPDATE", $"Misión actualizada: {inst.definition.title}");

        // completar si aplica
        if (inst.IsAllObjectivesComplete())
        {
            CompleteQuest(inst.definition.questId);
        }
    }

    private void CompleteQuest(string questId)
    {
        if (!all.TryGetValue(questId, out var inst)) return;
        if (inst.status != QuestStatus.Active) return;

        inst.status = QuestStatus.Completed;
        activeIds.Remove(questId);
        completedIds.Add(questId);

        UnregisterObjectives(inst);

        onQuestCompleted?.Invoke(inst.definition);
        onToast?.Invoke("COMPLETE", $"Misión completada: {inst.definition.title}");
    }

    private void RegisterObjectives(QuestInstance inst)
    {
        if (hub == null) return;

        for (int i = 0; i < inst.definition.objectives.Count; i++)
        {
            var obj = inst.definition.objectives[i];
            obj.Register(hub, this, inst, i);
        }
    }

    private void UnregisterObjectives(QuestInstance inst)
    {
        if (hub == null) return;

        for (int i = 0; i < inst.definition.objectives.Count; i++)
        {
            var obj = inst.definition.objectives[i];
            obj.Unregister(hub, this, inst, i);
        }
    }
}