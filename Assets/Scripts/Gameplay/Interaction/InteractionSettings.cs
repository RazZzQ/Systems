using UnityEngine;
public enum InteractionCastMode {Raycast, SphereCast}

[CreateAssetMenu(fileName = "InteractionSettings", menuName = "InteractionSettings")]
public class InteractionSettings : ScriptableObject
{
    [Header("Detection")]
    public InteractionCastMode castMode = InteractionCastMode.Raycast;
    public float maxDistance;
    public float sphereRadius;
    public LayerMask interactableMask;
    public QueryTriggerInteraction triggerMode = QueryTriggerInteraction.Ignore;

    [Header("Scoring & Priority")]
    [Tooltip("Que tanto pesa el angulo vs la distancia al elegir el mejor target")]
    [Range(0f, 1f)] public float angleWeight;

    [Header("Input")]
    public bool requireLineOfSight;

    [Header("Hold")]
    public float holdThreshold; //para considerar "hold" & "tap"
}
