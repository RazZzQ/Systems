using UnityEngine;
using TMPro;
using DG.Tweening;

public class QuestToastUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup group;
    [SerializeField] private TMP_Text headerText;
    [SerializeField] private TMP_Text bodyText;

    [Header("Tween")]
    [SerializeField] private float fadeIn = 0.15f;
    [SerializeField] private float hold = 2.0f;
    [SerializeField] private float fadeOut = 0.25f;

    private Tween running;

    private void Awake()
    {
        ForceHidden();
    }

    public void ShowToast(string type, string message)
    {
        if (!group) return;

        if (headerText) headerText.text = TypeToHeader(type);
        if (bodyText) bodyText.text = message ?? "";

        // mata animación anterior para no mezclar alphas
        running?.Kill();

        // estado inicial
        group.gameObject.SetActive(true);
        group.blocksRaycasts = false;
        group.interactable = false;

        group.alpha = 0f;

        // secuencia: fade in -> hold -> fade out -> apagar
        running = DOTween.Sequence()
            .Append(group.DOFade(1f, fadeIn))
            .AppendInterval(hold)
            .Append(group.DOFade(0f, fadeOut))
            .OnComplete(ForceHidden);
    }

    private void ForceHidden()
    {
        if (!group) return;
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;
        group.gameObject.SetActive(false);
    }

    private string TypeToHeader(string type)
    {
        return type switch
        {
            "NEW" => "Nueva misión",
            "UPDATE" => "Misión actualizada",
            "COMPLETE" => "Misión completada",
            "REJECT" => "Misión rechazada",
            "OFFER" => "Misión disponible",
            _ => "Misión"
        };
    }
}
