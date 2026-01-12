using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "QuestDefinition", menuName = "Quests/Quest Definition")]
public class QuestDefinitionSO : ScriptableObject
{
    [Header("ID")]
    public string questId = "quest_01";

    [Header("UI")]
    public string title = "Mision";
    [TextArea(2, 6)] public string description;

    [Header("Objetivos")]
    public List<ObjectiveDefinitionSO> objectives = new();

    [Header("Opcional")]
    public bool autoAcceptWhenOffered = false;
}
