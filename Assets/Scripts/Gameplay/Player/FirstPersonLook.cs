using UnityEngine;
using UnityEngine.InputSystem;

/// Controla la mirada:
/// - Yaw (izq/der) rota el player.
/// - Pitch (arriba/abajo) rota el CameraTarget.
/// Cinemachine sigue y mira ese CameraTarget.
public class FirstPersonLook : MonoBehaviour, IPlayerModule
{
    private PlayerContext ctx;
    private float pitch;
    private Vector2 smoothVel;  // para smoothing simple

    public void Initialize(PlayerContext ctx)
    {
        this.ctx = ctx;
        pitch = 0f;

        // FPS típico
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Tick(float dt)
    {
        Vector2 look = ctx.input.Look;

        // Mouse delta, Gamepad depende de binding (puede ser delta o stick)
        // Para hacerlo universal detecta si hay gamepad activo.
        // Heurística simple si hay gamepad activo y no hubo mouse este frame
        bool usingGamepad = Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame && !Mouse.current.wasUpdatedThisFrame;

        float sens = usingGamepad ? ctx.settings.gamepadSensitivity : ctx.settings.mouseSensitivity;

        // Stick se trata como grados/seg => multiplicamos dt
        if (usingGamepad) look *= dt;

        // Smoothing opcional
        if (ctx.settings.lookSmoothing > 0f)
        {
            // SmoothDamp desde 0 hacia look crea un filtro suave
            look = Vector2.SmoothDamp(Vector2.zero, look, ref smoothVel, 1f / ctx.settings.lookSmoothing);
        }

        float yawDelta = look.x * sens;
        float pitchDelta = look.y * sens * (ctx.settings.invertY ? 1f : -1f);

        // Yaw: gira el root
        ctx.cameraYawRoot.Rotate(0f, yawDelta, 0f, Space.Self);

        // Pitch: solo el target (local)
        pitch = Mathf.Clamp(pitch + pitchDelta, ctx.settings.pitchMin, ctx.settings.pitchMax);
        ctx.cameraPitchRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    public void FixedTick(float fdt) { }
}
