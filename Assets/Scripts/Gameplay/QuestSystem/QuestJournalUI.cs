using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class QuestJournalUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private CanvasGroup group;
    [SerializeField] private RectTransform panelRoot;
    [SerializeField] private QuestJournal journal;

    [Header("Events (hook UI control gate here)")]
    public UnityEvent onOpened;
    public UnityEvent onClosed;

    [Header("List UI")]
    [SerializeField] private Transform activeListRoot;
    [SerializeField] private Transform completedListRoot;
    [SerializeField] private QuestRowUI rowPrefab;

    [Header("Details UI")]
    [SerializeField] private TMP_Text detailsTitle;
    [SerializeField] private TMP_Text detailsDesc;
    [SerializeField] private TMP_Text detailsObjectives;

    [Header("Tween")]
    [SerializeField] private float openDuration = 0.18f;
    [SerializeField] private float closeDuration = 0.14f;
    [SerializeField] private float scaleFrom = 0.98f;

    private bool open;
    private Tween running;

    private float lastToggleTime;

    private void Awake()
    {
        HideInstant();
    }

    public void BindJournal(QuestJournal j)
    {
        journal = j;
        Refresh();
    }

    // ✅ Usa esto en PlayerInput (Invoke Unity Events)
    public void OnToggleJournal(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        // Debounce para evitar doble toggle accidental
        if (Time.unscaledTime - lastToggleTime < 0.15f) return;
        lastToggleTime = Time.unscaledTime;

        SetOpen(!open);
    }

    public void SetOpen(bool on)
    {
        if (open == on) return;
        open = on;

        if (open) Open();
        else Close();
    }

    private void Open()
    {
        if (!group || !panelRoot) return;

        Refresh(); // opcional: actualizar siempre al abrir

        running?.Kill();

        gameObject.SetActive(true);
        group.blocksRaycasts = true;
        group.interactable = true;

        group.alpha = 0f;
        panelRoot.localScale = Vector3.one * scaleFrom;

        onOpened?.Invoke();

        running = DOTween.Sequence()
            .Append(group.DOFade(1f, openDuration))
            .Join(panelRoot.DOScale(1f, openDuration).SetEase(Ease.OutBack));
    }

    private void Close()
    {
        if (!group || !panelRoot) return;

        running?.Kill();

        group.blocksRaycasts = false;
        group.interactable = false;

        running = DOTween.Sequence()
            .Append(group.DOFade(0f, closeDuration))
            .Join(panelRoot.DOScale(scaleFrom, closeDuration).SetEase(Ease.InSine))
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

    // ====== UI LIST ======

    public void Refresh()
    {
        if (!journal || !rowPrefab) return;

        Clear(activeListRoot);
        Clear(completedListRoot);

        foreach (var id in journal.ActiveIds)
        {
            var inst = journal.GetInstance(id);
            if (inst == null) continue;

            var row = Instantiate(rowPrefab, activeListRoot);
            row.Set(inst.definition.title, () => ShowDetails(inst));
        }

        foreach (var id in journal.CompletedIds)
        {
            var inst = journal.GetInstance(id);
            if (inst == null) continue;

            var row = Instantiate(rowPrefab, completedListRoot);
            row.Set(inst.definition.title, () => ShowDetails(inst));
        }

        if (detailsTitle) detailsTitle.text = "";
        if (detailsDesc) detailsDesc.text = "";
        if (detailsObjectives) detailsObjectives.text = "";
    }

    private void ShowDetails(QuestInstance inst)
    {
        if (detailsTitle) detailsTitle.text = inst.definition.title;
        if (detailsDesc) detailsDesc.text = inst.definition.description;

        if (detailsObjectives)
        {
            System.Text.StringBuilder sb = new();
            for (int i = 0; i < inst.definition.objectives.Count; i++)
            {
                var obj = inst.definition.objectives[i];
                int p = inst.progress[i];
                sb.AppendLine($"- {obj.objectiveTitle} ({p}/{obj.targetCount})");
            }
            detailsObjectives.text = sb.ToString();
        }
    }

    private void Clear(Transform root)
    {
        if (!root) return;
        for (int i = root.childCount - 1; i >= 0; i--)
            Destroy(root.GetChild(i).gameObject);
    }
}