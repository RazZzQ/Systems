using UnityEngine;
using UnityEngine.InputSystem;

public class UIControlGate : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputRouter inputRouter;
    [SerializeField] private PlayerInput playerInput; // opcional para ActionMaps

    [Header("Action Maps (optional)")]
    [SerializeField] private bool switchActionMaps = true;
    [SerializeField] private string gameplayMap = "Player";
    [SerializeField] private string uiMap = "UI";

    public void OpenUI()
    {
        // bloquear control del jugador
        if (inputRouter != null) inputRouter.DisableControl();

        // cambiar a UI map
        if (switchActionMaps && playerInput != null)
            playerInput.SwitchCurrentActionMap(uiMap);

        // cursor libre
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void CloseUI()
    {
        // reactivar control del jugador
        if (inputRouter != null) inputRouter.EnableControl();

        // volver a gameplay map
        if (switchActionMaps && playerInput != null)
            playerInput.SwitchCurrentActionMap(gameplayMap);

        // cursor bloqueado (FPS)
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
