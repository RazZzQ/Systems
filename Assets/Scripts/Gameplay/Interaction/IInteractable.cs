using Unity.Cinemachine;
using UnityEngine;
public enum InteractionType {Press, Hold}

public struct InteractionContext
{
    public Transform interactor;
    public CinemachineCamera camera;
    public Vector3 hitPoint;
    public Vector3 hitNormal;
}

public struct InteractionPrompt
{
    public string title;            //"puerta"
    public string actionText;       //"abrir"
    public InteractionType type;    //Press / Hold
    public float holdDuration;      //si es "Hold"
    public bool canInteract;        //para UI (Gris/Rojo)
    public string blockedReason;    //Opcional
}

public interface IInteractable
{
    //Prompt dinamico (puede cambiar: bloqueado, requiere llave, etc...)
    InteractionPrompt GetPrompt(in InteractionContext ctx);

    //Validacion (distancia, estado, llaves, etc...)
    bool CanInteract(in InteractionContext ctx, out string reason);

    //Ejecutar interaccion PRESS
    void Interact(in InteractionContext ctx);

    //Ejecutar interaccion HOLD completada
    void InteractHoldComplete(in InteractionContext ctx);

    //Feedback de foco (Opcional)
    void OnFocusEnter(in InteractionContext ctx);
    void OnFocusExit(in InteractionContext ctx);
}
