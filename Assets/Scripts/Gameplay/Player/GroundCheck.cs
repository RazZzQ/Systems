using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    [SerializeField] private CharacterController cc;
    public bool Grounded => cc != null && cc.isGrounded;

    private void Reset() => cc = GetComponentInParent<CharacterController>();
}
