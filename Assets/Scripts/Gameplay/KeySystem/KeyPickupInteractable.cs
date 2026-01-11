using UnityEngine;
using UnityEngine.Events;

public class KeyPickupInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private KeyDefinition key;

    [Header("Prompt")]
    [SerializeField] private string titleFallback = "Llave";
    [SerializeField] private string actionText = "Recoger";

    [Header("Behavior")]
    [SerializeField] private bool destroyOnPickup = true;

    [Header("Events")]
    public UnityEvent onPickedUp;
    public UnityEvent onPickFailed;

    public InteractionPrompt GetPrompt(in InteractionContext ctx)
    {
        bool can = key != null;
        return new InteractionPrompt
        {
            title = key ? key.displayName : titleFallback,
            actionText = actionText,
            type = InteractionType.Press,
            holdDuration = 0f,
            canInteract = can,
            blockedReason = can ? "" : "Llave inválida."
        };
    }

    public bool CanInteract(in InteractionContext ctx, out string reason)
    {
        if (key == null) { reason = "Llave inválida."; return false; }
        reason = "";
        return true;
    }

    public void Interact(in InteractionContext ctx)
    {
        if (key == null) { onPickFailed?.Invoke(); return; }

        var keyring = ctx.interactor.GetComponentInParent<IKeyring>();
        var keyringComp = ctx.interactor.GetComponentInParent<KeyringComponent>();

        if (keyring == null)
        {
            onPickFailed?.Invoke();
            return;
        }

        // Registrar definición para UI
        if (keyringComp != null) keyringComp.RegisterDefinition(key);

        bool added = keyring.TryAddKey(key.keyId);
        if (!added) { onPickFailed?.Invoke(); return; }

        onPickedUp?.Invoke();

        if (destroyOnPickup) Destroy(gameObject);
    }

    public void InteractHoldComplete(in InteractionContext ctx) { }

    public void OnFocusEnter(in InteractionContext ctx) { }
    public void OnFocusExit(in InteractionContext ctx) { }
}