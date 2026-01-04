using UnityEngine;

/// Encargado de salto, gravedad, coyote time, jump buffer y eventos.
/// Nota: la gravedad se aplica en FixedTick para consistencia.
public class FirstPersonJump : MonoBehaviour, IPlayerModule
{
    private PlayerContext ctx;

    private float coyoteTimer;
    private float jumpBufferTimer;
    private bool wasGrounded;

    public void Initialize(PlayerContext ctx)
    {
        this.ctx = ctx;
        wasGrounded = ctx.motor.IsGrounded;
    }

    public void Tick(float dt)
    {
        // Jump buffer: si presionas antes de caer al suelo, guardamos la intención
        if (ctx.input.JumpPressed) jumpBufferTimer = ctx.settings.jumpBuffer;
        else jumpBufferTimer -= dt;

        // Coyote: si estás grounded, resetea; si no, cuenta hacia abajo
        if (ctx.motor.IsGrounded) coyoteTimer = ctx.settings.coyoteTime;
        else coyoteTimer -= dt;

        // Eventos de transición grounded/air/land
        bool grounded = ctx.motor.IsGrounded;

        if (grounded && !wasGrounded)
        {
            ctx.events?.onLand?.Invoke();
            ctx.events?.onGrounded?.Invoke();
        }
        else if (!grounded && wasGrounded)
        {
            ctx.events?.onAirborne?.Invoke();
        }
        wasGrounded = grounded;

        //ctx.input.ConsumeOneFrameButtons();
    }

    public void FixedTick(float fdt)
    {
        // Gravedad
        ctx.motor.AddVerticalVelocity(ctx.settings.gravity * fdt);

        bool canJump = coyoteTimer > 0f;
        bool wantsJump = jumpBufferTimer > 0f;

        if (canJump && wantsJump)
        {
            // v = sqrt(2*g*h) con g positiva en magnitud
            float jumpVel = Mathf.Sqrt(2f * Mathf.Abs(ctx.settings.gravity) * ctx.settings.jumpHeight);

            // Si venías cayendo, limpia "Y" negativa para que el salto sea consistente
            float currentY = ctx.motor.Velocity.y;
            if (currentY < 0f) ctx.motor.AddVerticalVelocity(-currentY);
            ctx.motor.AddVerticalVelocity(jumpVel);

            // consume timers
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;

            ctx.events?.onJump?.Invoke();
        }
    }
}
