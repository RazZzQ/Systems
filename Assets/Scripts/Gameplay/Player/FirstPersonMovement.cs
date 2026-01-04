using UnityEngine;

/// Movimiento con aceleración/deceleración y control en aire.
/// Produce una velocidad horizontal target y el motor la aplica.
public class FirstPersonMovement : MonoBehaviour, IPlayerModule
{
    private PlayerContext ctx;
    private Vector3 currentVel; // velocidad horizontal que vamos suavizando

    // Estados externos
    public bool IsSprinting { get; set; }
    public bool IsCrouching { get; set; }

    public void Initialize(PlayerContext ctx)
    {
        this.ctx = ctx;
        currentVel = Vector3.zero;
    }

    public void Tick(float dt)
    {
        // Nada aquí: movemos en Fixed para consistencia física con CC
    }

    public void FixedTick(float fdt)
    {
        // Input local (x=strafe, y=forward)
        Vector2 move = ctx.input.Move;
        Vector3 wishLocal = new Vector3(move.x, 0f, move.y);
        wishLocal = Vector3.ClampMagnitude(wishLocal, 1f);

        // Convertir a mundo usando yaw del player
        Vector3 wishWorld = ctx.cameraYawRoot.TransformDirection(wishLocal);

        // Elegir speed según estados
        float baseSpeed = ctx.settings.walkSpeed;
        if (IsSprinting) baseSpeed = ctx.settings.sprintSpeed;
        if (IsCrouching) baseSpeed *= ctx.settings.crouchSpeedMultiplier;

        Vector3 targetVel = wishWorld * baseSpeed;

        bool grounded = ctx.motor.IsGrounded;

        // En el aire, reduces control
        float accel = grounded ? ctx.settings.acceleration : ctx.settings.acceleration * ctx.settings.airControl;
        float decel = grounded ? ctx.settings.deceleration : ctx.settings.deceleration * ctx.settings.airControl;

        // Si no hay input, decel hacia 0; si hay, acel hacia target
        float rate = (targetVel.sqrMagnitude > 0.0001f) ? accel : decel;

        // MoveTowards = aceleración simple y estable
        currentVel = Vector3.MoveTowards(currentVel, targetVel, rate * fdt);

        // Reporte 0..1 (para UI)
        float speed01 = Mathf.InverseLerp(0f, ctx.settings.sprintSpeed, currentVel.magnitude);
        ctx.events?.onSpeedChanged01?.Invoke(speed01);

        // Pegado al suelo: si estás grounded y cayendo, fija Y a un valor leve hacia abajo
        if (grounded && ctx.motor.Velocity.y < 0f)
        {
            // leve fuerza hacia abajo para evitar “floating”
            ctx.motor.AddVerticalVelocity(ctx.settings.groundedStickForce - ctx.motor.Velocity.y);
        }

        // Aplicar movimiento al motor
        ctx.motor.Move(currentVel, fdt);
    }
}
