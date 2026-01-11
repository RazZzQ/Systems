using System.IO;
using Unity.Mathematics;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[System.Serializable]
public class InteractionEvents
{
    public UnityEvent onNoTarget;
    public UnityEvent<string, string, bool, float> onPromptChanged; // title, actionText, isHold, holdDuration
    public UnityEvent<bool, string> onCanInteractChanged; // can, reason
    public UnityEvent<float> onHoldProgress01; // para barra UI
    public UnityEvent onHoldStart;
    public UnityEvent onHoldCancel;
    public UnityEvent onHoldComplete;
    public UnityEvent onInteract;        // cuando se ejecuta
    public UnityEvent<string> onBlocked; // razón   
}

public class PlayerInteractor : MonoBehaviour, IPlayerModule
{
    [Header("Settings")]
    [SerializeField] private InteractionSettings settings;

    [Header("Ray Origin")]
    [SerializeField] private Transform rayOrigin; //cameraTarget o Eyes

    [Header("Events")]
    public InteractionEvents events =  new InteractionEvents();
    [Header("Debug Gizmos")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private Color gizmoIdleColor = Color.green;
    [SerializeField] private Color gizmoHitColor = Color.red;
    [SerializeField] private Color gizmoBlockedColor = Color.yellow;

    private PlayerContext ctx;
    private IInteractable current;
    private Collider currentCol;
    private InteractionPrompt currentPrompt;
    private string lastReason;
    private bool lastCan;

    private float holdTimer;
    private bool holding;

    private bool gizmoHasHit;
    private RaycastHit gizmoHit;

    public void Initialize(PlayerContext ctx)
    {
        this.ctx = ctx;
        if(!rayOrigin) rayOrigin = ctx.cameraPitchRoot; //fallback
        ClearTarget(); // estado limpio
    }
    
    public void Tick(float dt)
    {
        if (settings == null || rayOrigin == null || ctx.mainCamera == null) return;

        // Si control esta deshabilitado, limpia UI/estado.
        if(ctx.input != null && !ctx.input.controlEnabled){
            ClearTarget();
            return;
        }

        // 1. escanear objetivo
        var found = ScanBest(out var hit, out var col);

        //2. resolver target IInteractable
        IInteractable next = null;
        if (found) next = col.GetComponentInParent<IInteractable>();

        // 3. manejar enter/exit foco
        if(next != current){
            if(current != null)
                current.OnFocusExit(BuildCtx(hitPoint: hit.point, hitNormal: hit.normal));

            current = next;
            currentCol = col;

            lastCan = false;
            lastReason = null;
            
            if(current != null)
                current.OnFocusEnter(BuildCtx(hitPoint: hit.point, hitNormal: hit.normal));

            //reset hold
            CancelHold();
        }

        if(current == null){
            lastCan = false;
            lastReason = null;
            events?.onNoTarget?.Invoke();
            return;
        }

        //4. prompt + validacion
        var ictx = BuildCtx(hit.point, hit.normal);
        currentPrompt = current.GetPrompt(ictx);

        bool can = current.CanInteract(ictx, out string reason);
        if(currentPrompt.canInteract != can) currentPrompt.canInteract = can;
        if(!can) currentPrompt.blockedReason = reason;

        //emitir cambios de UI solo si cambian
        bool isHold = currentPrompt.type == InteractionType.Hold;
        events?.onPromptChanged?.Invoke(currentPrompt.title, currentPrompt.actionText, isHold, currentPrompt.holdDuration);

        if(can != lastCan || reason != lastReason)
        {
            lastCan = can;
            lastReason = reason;
            events?.onCanInteractChanged?.Invoke(can, reason);
        }

        //5. ejecutar interaccion
        HandleInteractionInput(dt, ictx, can, reason);
    }
    
    public void FixedTick(float fdt){}

    private void HandleInteractionInput(float dt, in InteractionContext ictx, bool can, string reason)
    {
        bool pressed = ctx.input.InteractPressed;
        bool held = ctx.input.InteractHeld;

        if (!can){
            if(pressed) events?.onBlocked?.Invoke(reason);
            
            return;
        }
        if (currentPrompt.type == InteractionType.Press){
            if (pressed){
                current.Interact(ictx);
                events?.onInteract?.Invoke();
            }

            return;
        }

        //hold
        float duration = Mathf.Max(0.01f, currentPrompt.holdDuration);

        if (held){
            if (!holding){
                holding = true;
                holdTimer = 0f;
                events?.onHoldStart?.Invoke();
            }

            holdTimer += dt;
            float p01 =  Mathf.Clamp01(holdTimer / duration);
            events?.onHoldProgress01?.Invoke(p01);

            if(holdTimer >= duration){
                current.InteractHoldComplete(ictx);
                events?.onHoldComplete?.Invoke();
                events?.onInteract?.Invoke();

                //reset para que no dispare repetido si mantiene
                holding = false;
                holdTimer = 0f;
            }
        }else{
            //si solto antes de completar
            if(holding) events?.onHoldCancel?.Invoke();
            holding = false;
            holdTimer = 0f;
            events?.onHoldProgress01?.Invoke(0f);    
        }
    }
    
    private bool ScanBest(out RaycastHit bestHit, out Collider bestCol)
    {
        bestHit = default;
        bestCol = null;

        Vector3 origin = rayOrigin.position;
        Vector3 dir = rayOrigin.forward;

        RaycastHit[] hits;

        if (settings.castMode == InteractionCastMode.Raycast)
        {
            hits = Physics.RaycastAll(origin, dir, settings.maxDistance, settings.interactableMask, settings.triggerMode);
        }
        else
        {
            hits = Physics.SphereCastAll(origin, settings.sphereRadius, dir, settings.maxDistance, settings.interactableMask, settings.triggerMode);
        }

        if (hits == null || hits.Length == 0)
            return false;

        float bestScore = float.NegativeInfinity;

        for (int i = 0; i < hits.Length; i++)
        {
            var h = hits[i];
            var col = h.collider;
            if (!col) continue;

            // Debe haber un IInteractable arriba (o en este)
            var interact = col.GetComponentInParent<IInteractable>();
            if (interact == null) continue;

            // Score por ángulo (dot) y distancia
            Vector3 to = (h.point - origin);
            float dist = to.magnitude;
            if (dist < 0.0001f) dist = 0.0001f;
            Vector3 toN = to / dist;

            float dot = Vector3.Dot(dir, toN);                 // 1 = frente perfecto
            float dist01 = 1f - Mathf.Clamp01(dist / settings.maxDistance); // 1 = muy cerca

            float score = settings.angleWeight * dot + (1f - settings.angleWeight) * dist01;

            // Bonus pequeño si es el collider “más cercano”
            score += dist01 * 0.05f;

            if (score > bestScore)
            {
                bestScore = score;
                bestHit = h;
                bestCol = col;
            }
        }
        
        if (bestCol != null)
        {
            gizmoHasHit = true;
            gizmoHit = bestHit;
            return true;
        }

        gizmoHasHit = false;
        return false;
    }

    private InteractionContext BuildCtx(Vector3 hitPoint, Vector3 hitNormal){
        return new InteractionContext
        {
            interactor = transform,
            camera = ctx.mainCamera,
            hitPoint = hitPoint,
            hitNormal = hitNormal,
        };
    }
    
    private void ClearTarget(){
        if(current != null) current.OnFocusExit(BuildCtx(transform.position, Vector3.up));

        current = null;
        currentCol = null;
        holding = false;
        holdTimer = 0f;

        events?.onHoldProgress01?.Invoke(0f);
        events?.onNoTarget?.Invoke();
    }

    private void CancelHold(){
        if (holding) events?.onHoldCancel?.Invoke();
        holding = false;
        holdTimer = 0f;
        events?.onHoldProgress01?.Invoke(0f);
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos || settings == null || rayOrigin == null)
            return;

        Vector3 origin = rayOrigin.position;
        Vector3 dir = rayOrigin.forward.normalized;
        float dist = settings.maxDistance;

        // Color base
        Gizmos.color = gizmoHasHit ? gizmoHitColor : gizmoIdleColor;

        if (settings.castMode == InteractionCastMode.Raycast)
        {
            // Ray principal
            Gizmos.DrawLine(origin, origin + dir * dist);

            // Punto de impacto
            if (gizmoHasHit)
            {
                Gizmos.DrawSphere(gizmoHit.point, 0.05f);
                Gizmos.DrawLine(gizmoHit.point, gizmoHit.point + gizmoHit.normal * 0.2f);
            }
        }
        else // SphereCast
        {
            float r = settings.sphereRadius;

            // Cápsula visual simple (inicio y fin)
            Gizmos.DrawWireSphere(origin, r);
            Gizmos.DrawWireSphere(origin + dir * dist, r);
            Gizmos.DrawLine(origin + Vector3.right * r, origin + dir * dist + Vector3.right * r);
            Gizmos.DrawLine(origin - Vector3.right * r, origin + dir * dist - Vector3.right * r);
            Gizmos.DrawLine(origin + Vector3.up * r, origin + dir * dist + Vector3.up * r);
            Gizmos.DrawLine(origin - Vector3.up * r, origin + dir * dist - Vector3.up * r);

            // Impacto
            if (gizmoHasHit)
            {
                Gizmos.DrawSphere(gizmoHit.point, 0.05f);
                Gizmos.DrawLine(gizmoHit.point, gizmoHit.point + gizmoHit.normal * 0.2f);
            }
        }
    }
}

