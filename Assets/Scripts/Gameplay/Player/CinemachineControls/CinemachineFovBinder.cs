using UnityEngine;
using Unity.Cinemachine;
public class CinemachineFovBinder : MonoBehaviour
{
    [SerializeField] private CinemachineCamera cam;
    [SerializeField] private float walkFov = 70f;
    [SerializeField] private float sprintFov = 80f;
    [SerializeField] private float speed = 8f;

    private float target;

    private void Awake()
    {
        target = walkFov;
    }

    private void Update()
    {
        cam.Lens.FieldOfView = Mathf.Lerp(cam.Lens.FieldOfView, target, 1f - Mathf.Exp(-speed * Time.deltaTime));
    }

    public void SetSprintFov() => target = sprintFov;
    public void SetWalkFov() => target = walkFov;
}
