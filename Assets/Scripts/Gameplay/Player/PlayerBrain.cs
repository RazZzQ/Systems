using UnityEngine;
using Unity.Cinemachine;

/// Orquestador:
/// - arma el PlayerContext
/// - inicializa módulos
/// - ejecuta Tick/FixedTick
public class PlayerBrain : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraYawRoot;
    [SerializeField] private Transform cameraPitchRoot;
    [SerializeField] private CinemachineCamera mainCamera;

    [Header("Input")]
    [SerializeField] private PlayerInputRouter input;

    [Header("Settings")]
    [SerializeField] private FPSettings settings;

    [Header("Events")]
    public PlayerEvents events = new PlayerEvents();

    private PlayerContext ctx;
    private IPlayerModule[] modules;
    private IMotor motor;

    private bool lastControlEnabled;

    private void Awake()
    {
        motor = GetComponent<IMotor>();
        if (motor == null) motor = GetComponent<CharacterMotorCC>();

        ctx = new PlayerContext
        {
            playerRoot = transform,
            cameraYawRoot = cameraYawRoot ? cameraYawRoot : transform,
            cameraPitchRoot = cameraPitchRoot,
            mainCamera = mainCamera,
            motor = motor,
            events = events,
            input = input,
            settings = settings
        };

        modules = GetComponents<IPlayerModule>();
        for (int i = 0; i < modules.Length; i++)
            modules[i].Initialize(ctx);

        lastControlEnabled = input != null && input.controlEnabled;
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        // Emitir input para UI/anim (sin acoplar)
        if (input != null)
        {
            events?.onMoveInput?.Invoke(input.Move);
            events?.onLookInput?.Invoke(input.Look);

            float moveMag01 = Mathf.Clamp01(input.Move.magnitude);
            events?.onMoveMagnitude01?.Invoke(moveMag01);

            // eventos de enable/disable control
            if (input.controlEnabled != lastControlEnabled)
            {
                lastControlEnabled = input.controlEnabled;
                if (lastControlEnabled) events?.onControlEnabled?.Invoke();
                else events?.onControlDisabled?.Invoke();
            }
        }

        for (int i = 0; i < modules.Length; i++)
            modules[i].Tick(dt);

        // Importantísimo: consumir one-shots al FINAL
        ctx.input?.ConsumeOneFrameButtons();
    }

    private void FixedUpdate()
    {
        float fdt = Time.fixedDeltaTime;
        for (int i = 0; i < modules.Length; i++)
            modules[i].FixedTick(fdt);
    }

    // Hooks para futuro (si luego haces health/state)
    public void Die() => events?.onDied?.Invoke();
    public void Respawn() => events?.onRespawned?.Invoke();
}
