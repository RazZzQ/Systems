using UnityEngine;

public class DoorInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string title;
    [SerializeField] private string action;
    [SerializeField] private bool useHold;
    [SerializeField] private float holdSeconds;

    [SerializeField] private bool locked;
    [SerializeField] private string lockedReason;

    [SerializeField] private FocusHighlight highlight;

    public InteractionPrompt GetPrompt(in InteractionContext ctx)
    {
        bool can = !locked;
        return new InteractionPrompt
        {
            title = title,
            actionText = action,
            type = useHold? InteractionType.Hold : InteractionType.Press,
            holdDuration = useHold ? holdSeconds : 0f,
            canInteract = can,
            blockedReason = can ? "" : lockedReason
        };
    }
    public bool CanInteract(in InteractionContext ctx, out string reason)
    {
        if(locked) { reason =  lockedReason; return false;}
        reason = "";
        return true;
    }

    public void Interact (in InteractionContext ctx)
    {
        //logica que necesites
        Debug.Log("door: interact Press");
    }

    public void InteractHoldComplete(in InteractionContext ctx)
    {
        //HoldInteraction
        Debug.Log("Door; Hold complete");
    }

    public void OnFocusEnter(in InteractionContext ctx) { if (highlight) highlight.Set(true); }
    public void OnFocusExit(in InteractionContext ctx) { if (highlight) highlight.Set(false); }
}
