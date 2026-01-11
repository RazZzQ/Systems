using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.Rendering;
using System.Runtime.CompilerServices;
public class InteractionPromptUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup group;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text actionText;
    [SerializeField] private TMP_Text reasonText;
    [SerializeField] private TMP_Text holdHintText;
    [SerializeField] private Image holdFill;

    private void Awake()
    {
        SetVisible(false);
        SetHold(0f);
        SetReason("", false);
    }

    public void OnNoTarget()
    {
        SetVisible(false);
        SetHold(0f);
        SetReason("", false);
    }
    public void OnPromptChanged(string title, string action, bool isHold, float holdDuration)
    {
         SetVisible(true);

        if (titleText) titleText.text = title ?? "";
        if (actionText) actionText.text = action ?? "";

        // Texto tipo: "F: Abrir" o "Mantén F: Forzar"
        if (holdHintText)
            holdHintText.text = isHold ? $"Mantén F: {action}" : $"F: {action}";

        // Barra solo si es hold
        if (holdFill)
            holdFill.gameObject.SetActive(isHold);

        // Reset barra cuando cambia target
        SetHold(0f);
    }

    public void OnCanInteractChanged(bool can, string reason)
    {
        SetReason(reason, !can);

    }
    public void OnHoldProgress(float p01) => SetHold(p01);

    private void SetVisible (bool on)
    {
        if(!group) return;
        group.alpha = on ? 1f : 0f;
        group.blocksRaycasts = on;
        group.interactable = on;
    }
    private void SetHold(float p01)
    {
        if(holdFill) holdFill.fillAmount = Mathf.Clamp01(p01);
    }
    private void SetReason(string text, bool show)
    {
        if(!reasonText) return;
        reasonText.gameObject.SetActive(show);
        reasonText.text = show ? text : "";
    }
}
