using UnityEngine;

public class QuestGiverTrigger : MonoBehaviour
{
    [SerializeField] private QuestGiver giver;
    [SerializeField] private QuestJournal playerJournal;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        giver.OfferTo(playerJournal);
    }
}
