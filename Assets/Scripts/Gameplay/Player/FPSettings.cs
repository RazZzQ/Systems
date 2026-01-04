using UnityEngine;

[CreateAssetMenu(menuName = "FP/Settings", fileName = "FPSettings")]

/// Settings de primera persona.
/// ScriptableObject = editable sin tocar código + puedes tener presets distintos.
public class FPSettings : ScriptableObject
{
    [Header("Look")]
    public float mouseSensitivity = 1.0f;
    public float gamepadSensitivity = 120.0f; // grados/seg
    public bool invertY = false;
    public float pitchMin = -80f;
    public float pitchMax = 80f;
    public float lookSmoothing = 0f; // 0 = raw

    [Header("Move")]
    public float walkSpeed = 4.5f;
    public float acceleration = 20f;
    public float deceleration = 18f;
    public float airControl = 0.35f;

    [Header("Sprint")]
    public float sprintSpeed = 7.0f;

    [Header("Jump / Gravity")]
    public float jumpHeight = 1.35f;
    public float gravity = -25f;
    public float coyoteTime = 0.12f;    // margen después de despegar
    public float jumpBuffer = 0.12f;    // margen si presionas antes de tocar suelo

    [Header("Crouch")]
    public bool crouchToggle = true;
    public float crouchSpeedMultiplier = 0.65f;
    public float crouchHeight = 1.2f;
    public float standHeight = 1.8f;
    public float crouchTransitionSpeed = 12f;

    [Header("Grounding")]
    public float groundedStickForce = -2.0f; // fuerza leve hacia abajo para pegarse al suelo
}
