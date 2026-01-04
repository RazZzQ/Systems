using UnityEngine;

/// Sprint como estado (hold) con reglas simples.
/// Deshabilita sprint si crouch está activo.
public class FirstPersonSprint : MonoBehaviour, IPlayerModule
{
    private PlayerContext ctx;
    private FirstPersonMovement move;
    private FirstPersonCrouch crouch;

    private bool sprinting;

    public void Initialize(PlayerContext ctx)
    {
        this.ctx = ctx;
        move = GetComponent<FirstPersonMovement>();
        crouch = GetComponent<FirstPersonCrouch>();
    }

    public void Tick(float dt)
    {
        bool canSprint = crouch == null || !crouch.IsCrouching;

        // Regla: sprint solo si se mantiene el botón y avanzas hacia adelante
        bool want = ctx.input.SprintHeld && canSprint && ctx.input.Move.y > 0.1f; // solo adelante

        if (want != sprinting)
        {
            sprinting = want;
            if (sprinting) ctx.events?.onSprintStart?.Invoke();
            else ctx.events?.onSprintEnd?.Invoke();
        }
        if (move != null) move.IsSprinting = sprinting;
    }

    public void FixedTick(float fdt) { }
}
