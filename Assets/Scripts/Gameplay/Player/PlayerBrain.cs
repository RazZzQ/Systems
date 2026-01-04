using UnityEngine;
using Unity.Cinemachine;

/// Orquestador:
/// - arma el PlayerContext
/// - inicializa módulos
/// - ejecuta Tick/FixedTick
public class PlayerBrain : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraYawRoot;   // normalmente el Player root
    [SerializeField] private Transform cameraPitchRoot; // CameraTarget
    [SerializeField] private CinemachineCamera MainCamera;

    [Header("Input")]
    [SerializeField] private PlayerInputRouter input;

    [Header("Settings")]
    [SerializeField] private FPSettings settings;

    [Header("Events")]
    public PlayerEvents events = new PlayerEvents();

    private PlayerContext ctx;
    private IPlayerModule[] modules;
    private IMotor motor;

    private void Awake()
    {
        // Obtiene el motor por interfaz (no se acopla a CharacterMotorCC)
        motor = GetComponent<IMotor>();
        if (motor == null)
        {
            // fallback: motor CC
            motor = GetComponent<CharacterMotorCC>();
        }

        ctx = new PlayerContext
        {
            playerRoot = transform,
            cameraYawRoot = cameraYawRoot ? cameraYawRoot : transform,
            cameraPitchRoot = cameraPitchRoot,
            mainCamera = MainCamera,
            motor = motor,
            events = events,
            input = input,
            settings = settings
        };
        
        // Encuentra todos los módulos en el mismo GO
        modules = GetComponents<IPlayerModule>();
        for (int i = 0; i < modules.Length; i++)
            modules[i].Initialize(ctx);
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        for (int i = 0; i < modules.Length; i++)
            modules[i].Tick(dt);

        ctx.input.ConsumeOneFrameButtons();
    }

    private void FixedUpdate()
    {
        float fdt = Time.fixedDeltaTime;
        for (int i = 0; i < modules.Length; i++)
            modules[i].FixedTick(fdt);
    }
}
