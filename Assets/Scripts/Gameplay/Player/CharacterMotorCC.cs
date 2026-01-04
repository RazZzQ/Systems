using UnityEngine;

/// Implementación del motor usando CharacterController.
/// Es intercambiable (IMotor): puedes crear otro motor luego.
public class CharacterMotorCC : MonoBehaviour, IMotor
{
    [SerializeField] private CharacterController cc;

    // velocity incluye la componente Y (gravedad/salto)
    private Vector3 velocity; 
    public bool IsGrounded { get; private set; }
    public Vector3 Velocity => velocity;

    private void Reset() => cc = GetComponent<CharacterController>();
    private void Awake() { if (!cc) cc = GetComponent<CharacterController>(); }

    public void Move(Vector3 worldVelocity, float dt)
    {
        // worldVelocity es horizontal (XZ) deseada por módulos
        velocity.x = worldVelocity.x;
        velocity.z = worldVelocity.z;

        // CharacterController.Move recibe un desplazamiento (vel * dt)
        CollisionFlags flags = cc.Move(velocity * dt);

        // Grounded robusto (Below o isGrounded)
        IsGrounded = (flags & CollisionFlags.Below) != 0 || cc.isGrounded;

        // Si chocaste con techo, corta Y positiva para evitar “pegado”
        if ((flags & CollisionFlags.Above) != 0 && velocity.y > 0f)
            velocity.y = 0f;
    }

    public void AddVerticalVelocity(float delta) => velocity.y += delta;

    public void SetCapsule(float height, Vector3 center)
    {
        cc.height = height;
        cc.center = center;
    }
}
