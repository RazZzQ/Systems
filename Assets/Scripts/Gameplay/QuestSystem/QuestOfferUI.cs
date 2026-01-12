using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;

public class QuestOfferUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup group;
    [SerializeField] private RectTransform panelRoot; // el panel que quieres “pop”
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button rejectButton;

    [Header("Tween")]
    [SerializeField] private float fadeIn = 0.12f;
    [SerializeField] private float fadeOut = 0.12f;
    [SerializeField] private float popScale = 1.0f;
    [SerializeField] private float popFrom = 0.96f;

    [Header("Events")]
    public UnityEvent onOpened;
    public UnityEvent onClosed;

    [Header("Runtime")]
    [SerializeField] private QuestJournal journal;

    private QuestDefinitionSO current;
    private Tween running;

    private void Awake()
    {
        if (acceptButton) acceptButton.onClick.AddListener(Accept);
        if (rejectButton) rejectButton.onClick.AddListener(Reject);
        HideInstant();
    }

    public void Bind(QuestJournal j) => journal = j;

    public void ShowOffer(QuestDefinitionSO quest)
    {
        if (!group || !panelRoot) return;

        current = quest;
        if (titleText) titleText.text = quest ? quest.title : "";
        if (descText) descText.text = quest ? quest.description : "";

        running?.Kill();

        gameObject.SetActive(true);
        group.blocksRaycasts = true;
        group.interactable = true;

        group.alpha = 0f;
        panelRoot.localScale = Vector3.one * popFrom;

        onOpened?.Invoke();

        running = DOTween.Sequence()
            .Append(group.DOFade(1f, fadeIn))
            .Join(panelRoot.DOScale(popScale, fadeIn).SetEase(Ease.OutBack));
    }

    public void HideOffer()
    {
        if (!group || !panelRoot) return;

        running?.Kill();

        // deshabilitar interacción inmediatamente para evitar doble click
        group.blocksRaycasts = false;
        group.interactable = false;

        running = DOTween.Sequence()
            .Append(group.DOFade(0f, fadeOut))
            .Join(panelRoot.DOScale(popFrom, fadeOut).SetEase(Ease.InSine))
            .OnComplete(() =>
            {
                HideInstant();
                onClosed?.Invoke();
            });
    }

    private void HideInstant()
    {
        if (!group) return;
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;
        gameObject.SetActive(false);
    }

    private void Accept()
    {
        if (journal == null || current == null) return;
        journal.AcceptQuest(current.questId);
        HideOffer();
    }

    private void Reject()
    {
        if (journal == null || current == null) return;
        journal.RejectQuest(current.questId);
        HideOffer();
    }
}
