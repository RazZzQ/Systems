using UnityEngine;
using UnityEngine.Events;

public class DoorLockInteractable : MonoBehaviour, IInteractable
{
    public enum ConsumeMode { DoNotConsume, ConsumeOnUnlock }

    [Header("Prompt")]
    [SerializeField] private string title = "Puerta";
    [SerializeField] private string actionOpen = "Abrir";
    [SerializeField] private string actionClose = "Cerrar";

    [Header("Lock")]
    [SerializeField] private bool locked = true;
    [SerializeField] private KeyDefinition requiredKey;
    [SerializeField] private string lockedReason = "Está cerrada con llave.";
    [SerializeField] private ConsumeMode consumeKey = ConsumeMode.DoNotConsume;

    [Header("Door State")]
    [SerializeField] private bool isOpen = false;

    [Header("Events")]
    public UnityEvent onUnlock;          // SFX: click, UI
    public UnityEvent onUnlockFailed;    // SFX: error
    public UnityEvent onOpen;            // animación abrir
    public UnityEvent onClose;           // animación cerrar

    // Si quieres feedback adicional para UI world:
    public UnityEvent<string> onStateMessage;

    public InteractionPrompt GetPrompt(in InteractionContext ctx)
    {
        bool can = CanInteract(ctx, out string reason);

        string action = isOpen ? actionClose : actionOpen;

        // Si está locked, muestra igual "Abrir" pero canInteract false y reason visible
        return new InteractionPrompt
        {
            title = title,
            actionText = action,
            type = InteractionType.Press,
            holdDuration = 0f,
            canInteract = can,
            blockedReason = reason
        };
    }

    public bool CanInteract(in InteractionContext ctx, out string reason)
    {
        if (!locked)
        {
            reason = "";
            return true;
        }

        // locked: solo puedes interactuar si tienes la llave
        if (requiredKey == null || string.IsNullOrEmpty(requiredKey.keyId))
        {
            reason = lockedReason;
            return false;
        }

        var keyring = ctx.interactor.GetComponentInParent<IKeyring>();
        if (keyring != null && keyring.HasKey(requiredKey.keyId))
        {
            reason = "";
            return true;
        }

        reason = lockedReason;
        return false;
    }

    public void Interact(in InteractionContext ctx)
    {
        // Si locked, intentar unlock
        if (locked)
        {
            var keyring = ctx.interactor.GetComponentInParent<IKeyring>();
            if (keyring == null || requiredKey == null || !keyring.HasKey(requiredKey.keyId))
            {
                onUnlockFailed?.Invoke();
                onStateMessage?.Invoke("No tienes la llave.");
                return;
            }

            locked = false;

            if (consumeKey == ConsumeMode.ConsumeOnUnlock)
                keyring.TryConsumeKey(requiredKey.keyId);

            onUnlock?.Invoke();
            onStateMessage?.Invoke("Desbloqueado.");
        }

        // Toggle open/close
        isOpen = !isOpen;
        if (isOpen)
        {
            onOpen?.Invoke();
            onStateMessage?.Invoke("Abierto.");
        }
        else
        {
            onClose?.Invoke();
            onStateMessage?.Invoke("Cerrado.");
        }
    }

    public void InteractHoldComplete(in InteractionContext ctx) { }
    public void OnFocusEnter(in InteractionContext ctx) { }
    public void OnFocusExit(in InteractionContext ctx) { }
}
