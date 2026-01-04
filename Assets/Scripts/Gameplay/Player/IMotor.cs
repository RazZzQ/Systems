using UnityEngine;

/// Motor de movimiento desacoplado.
/// Los módulos calculan intención/velocidad, el motor la aplica.
/// Así puedes cambiar CharacterController por Rigidbody sin reescribir módulos.
public interface IMotor
{
    /// Mueve al personaje usando una velocidad en mundo (m/s) para XZ + Y interna.
    void Move(Vector3 worldVelocity, float dt);

    /// Ajusta la velocidad vertical (salto/gravedad/pegado al suelo)
    void AddVerticalVelocity(float delta);

    // Estado
    bool IsGrounded { get; }
    Vector3 Velocity { get; }

    /// Permite cambiar cápsula (crouch) sin que el módulo sepa del CC
    void SetCapsule(float height, Vector3 center);
}
