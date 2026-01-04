using UnityEngine;

/// Crouch con toggle + transición suave de cápsula.
/// Ajusta height/center vía motor para no acoplar al CharacterController.
public class FirstPersonCrouch : MonoBehaviour, IPlayerModule
{
    private PlayerContext ctx;
    private FirstPersonMovement move;

    [Header("Visual (opcional)")]
    [SerializeField] private Transform visualsRoot;   // PlayerBody3D
    [SerializeField] private Transform eyesRoot;      // PlayerEyes o CameraTarget
    [SerializeField] private float eyesStandY = 1.6f; // ajusta a tu gusto
    [SerializeField] private float eyesCrouchY = 1.0f;

    public bool crouching;
    public float currentHeight;
    private float visualsBaseY;
    public bool IsCrouching => crouching;

    public void Initialize(PlayerContext ctx)
    {
        this.ctx = ctx;
        move = GetComponent<FirstPersonMovement>();

        currentHeight = ctx.settings.standHeight;
        ApplyCapsule(currentHeight);
        if (visualsRoot != null)
            visualsBaseY = visualsRoot.localPosition.y;
    }

    public void Tick(float dt)
    {
        // Toggle
        if (ctx.settings.crouchToggle && ctx.input.CrouchPressed)
            SetCrouch(!crouching);
    }

    public void FixedTick(float fdt)
    {
        float target = crouching ? ctx.settings.crouchHeight : ctx.settings.standHeight;

        // suavizado estable con exp
        currentHeight = Mathf.Lerp(currentHeight, target, 1f - Mathf.Exp(-ctx.settings.crouchTransitionSpeed * fdt));

        ApplyCapsule(currentHeight);

        // t = 0 stand, 1 crouch (para interpolar ojos)
        float t = Mathf.InverseLerp(ctx.settings.standHeight, ctx.settings.crouchHeight, currentHeight);
        ApplyVisuals(t);

        if (move != null) move.IsCrouching = crouching;
    }

    private void SetCrouch(bool on)
    {
        if (crouching == on) return;
        crouching = on;

        if (crouching) ctx.events?.onCrouchStart?.Invoke();
        else ctx.events?.onCrouchEnd?.Invoke();
    }

    private void ApplyCapsule(float height)
    {
        // center = height/2 mantiene base en el suelo
        Vector3 center = new Vector3(0f, height * 0.5f, 0f);
        ctx.motor.SetCapsule(height, center);
    }

    private void ApplyVisuals(float t01)
    {
        // Baja la cámara/ojos
        if (eyesRoot != null)
        {
            Vector3 p = eyesRoot.localPosition;
            p.y = Mathf.Lerp(eyesStandY, eyesCrouchY, t01);
            eyesRoot.localPosition = p;
        }

        // Visual del cuerpo
        if (visualsRoot != null)
        {
            Vector3 p = visualsRoot.localPosition;
            p.y = Mathf.Lerp(visualsBaseY, visualsBaseY - 0.4f, t01);
            visualsRoot.localPosition = p;
        }
    }
}
