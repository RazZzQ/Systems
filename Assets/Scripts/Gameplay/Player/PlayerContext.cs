using UnityEngine;
using UnityEngine.Events;
using Unity.Cinemachine;

[System.Serializable]
/// UnityEvents para enganchar sistemas externos desde Inspector.
/// Ej: SFX, UI, anim, recoil, FOV.
public class PlayerEvents
{
    // Locomotion
    public UnityEvent onGrounded;
    public UnityEvent onAirborne;
    public UnityEvent onJump;
    public UnityEvent onLand;

    public UnityEvent onCrouchStart;
    public UnityEvent onCrouchEnd;

    public UnityEvent onSprintStart;
    public UnityEvent onSprintEnd;

    public UnityEvent<float> onSpeedChanged01;   // 0..1 (UI)
    public UnityEvent<float> onMoveMagnitude01;  // 0..1 (input magnitude)
    public UnityEvent<Vector2> onMoveInput;      // raw move input
    public UnityEvent<Vector2> onLookInput;      // raw look input

    // Control state
    public UnityEvent onControlEnabled;
    public UnityEvent onControlDisabled;

    // Health / state hooks (para escalar luego)
    public UnityEvent onDied;
    public UnityEvent onRespawned;
}

/// Contexto compartido entre módulos.
/// Es "inyección de dependencias" simple sin frameworks.
public class PlayerContext
{
    public Transform playerRoot;      // Root del player
    public Transform cameraYawRoot;   // Donde aplicas yaw (normalmente player root)
    public Transform cameraPitchRoot; // Donde aplicas pitch (CameraTarget)
    public CinemachineCamera mainCamera;

    public IMotor motor;
    public PlayerEvents events;

    public PlayerInputRouter input;
    public FPSettings settings;
}
