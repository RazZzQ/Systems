using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class QuestRowUI : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private Button button;

    public void Set(string text, Action onClick)
    {
        if (label) label.text = text;
        if (button)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick?.Invoke());
        }
    }
}
