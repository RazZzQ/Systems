using UnityEngine;
using UnityEngine.Events;

[System.Serializable] public class StringEvent : UnityEvent<string>{}
[System.Serializable] public class StringIntEvent : UnityEvent<string, int>{}
[System.Serializable] public class TransformStringEvent : UnityEvent<Transform, string>{}
public class GameEventHub : MonoBehaviour
{
    [Header("Gameplays Events")]
    public StringIntEvent onItemAdded;          //itemId, amount
    public StringEvent onEnemyKilled;           //enemyId
    public StringEvent onInteracted;            //InteractableId
    public TransformStringEvent onDialogueEnded;//npcTransform, dialogueId

    //Metodos helper (los llamas desde tu juego)
    public void RaiseItemAdded (string itemId, int amount) => onItemAdded?.Invoke(itemId, amount);
    public void RaiseEnemyKilled (string enemyId) => onEnemyKilled?.Invoke(enemyId);
    public void RaiseInterected (string InteractableId) => onInteracted?.Invoke(InteractableId);
    public void RaiseDialogueEnded (Transform npc, string dialogueId) => onDialogueEnded?.Invoke(npc, dialogueId);
}
