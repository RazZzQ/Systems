using UnityEngine;
//IPlayerModule
/// Contrato para módulos del jugador.
/// - Tick: lógica por frame (Update).
/// - FixedTick: física (FixedUpdate).
public interface IPlayerModule
{
    void Initialize(PlayerContext ctx);
    void Tick(float dt);
    void FixedTick(float fdt);
}
