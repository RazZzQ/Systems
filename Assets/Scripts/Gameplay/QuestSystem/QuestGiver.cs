using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class QuestGiver : MonoBehaviour
{
    [Header("Quests to offer")]
    public List<QuestDefinitionSO> quests = new();

    [Header("Behavior")]
    public bool offerAllAtOnce = false;

    [Header("Events")]
    public UnityEvent onOfferStarted;
    public UnityEvent onOfferFinished;

    // Para diálogo: si termina dialogueId X, ofrecer misión Y
    [System.Serializable]
    public class DialogueQuestLink
    {
        public string dialogueId;
        public QuestDefinitionSO quest;
        public bool autoAccept;
    }
    public List<DialogueQuestLink> dialogueLinks = new();

    public void OfferTo(QuestJournal journal)
    {
        if (!journal) return;

        onOfferStarted?.Invoke();

        if (offerAllAtOnce)
        {
            foreach (var q in quests)
                journal.OfferQuest(q);
        }
        else
        {
            if (quests.Count > 0)
                journal.OfferQuest(quests[0]);
        }

        onOfferFinished?.Invoke();
    }

    // Hook para diálogos: lo llamas cuando el diálogo termina
    public void OnDialogueEnded(string dialogueId, QuestJournal journal)
    {
        if (!journal) return;

        for (int i = 0; i < dialogueLinks.Count; i++)
        {
            var link = dialogueLinks[i];
            if (link.quest == null) continue;
            if (link.dialogueId != dialogueId) continue;

            // Offer
            journal.OfferQuest(link.quest);

            // Auto-accept opcional
            if (link.autoAccept)
                journal.AcceptQuest(link.quest.questId);

            break;
        }
    }
}
