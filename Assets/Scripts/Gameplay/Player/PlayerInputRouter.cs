using UnityEngine;
using UnityEngine.InputSystem;

/// Recibe Input System por UnityEvents (PlayerInput).
/// Expone valores actuales para que los módulos lean sin acoplarse al Input System.
public class PlayerInputRouter : MonoBehaviour
{
    public Vector2 Move { get; private set; }
    public Vector2 Look { get; private set; }

    // One-shot (se consume por frame)
    public bool JumpPressed { get; private set; }
    public bool JumpHeld { get; private set; }

    //held
    public bool SprintHeld { get; private set; }
    public bool CrouchPressed { get; private set; }

    public bool controlEnabled { get; private set; } = true;
    // Llamados por PlayerInput (Invoke Unity Events)
    public void OnMove(InputAction.CallbackContext ctx) { 
        if (!controlEnabled) return; 
        Move = ctx.ReadValue<Vector2>();
    }
    public void OnLook(InputAction.CallbackContext ctx)  {
        if (!controlEnabled) return; 
        Look = ctx.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (!controlEnabled) return; 

        if (ctx.started) JumpPressed = true;
        JumpHeld = ctx.ReadValueAsButton();
    }

    public void OnSprint(InputAction.CallbackContext ctx) { 
        if (!controlEnabled) return; 
        SprintHeld = ctx.ReadValueAsButton();
    }

    public void OnCrouch(InputAction.CallbackContext ctx){
        if (!controlEnabled) return;
        if (ctx.started) CrouchPressed = true;
    }

    /// Se llama 1 vez por frame (usualmente en Jump.Tick)
    /// para que JumpPressed/CrouchPressed no queden pegados.
    public void ConsumeOneFrameButtons()
    {
        JumpPressed = false;
        CrouchPressed = false;
    }
    /// Desactiva todo el control del jugador (menús, cinemáticas, minijuegos)
    public void DisableControl()
    {
        Debug.Log("Control Desactivado");
        controlEnabled = false;
        ResetAllInput();
    }

    /// Reactiva el control del jugador
    public void EnableControl()
    {
        Debug.Log("Control Activado");
        controlEnabled = true;
        ResetAllInput(); // evita input "fantasma"
    }

     /// Resetea absolutamente todo el estado de input
    private void ResetAllInput()
    {
        Move = Vector2.zero;
        Look = Vector2.zero;

        JumpPressed = false;
        JumpHeld = false;

        CrouchPressed = false;
        SprintHeld = false;
    }
}
