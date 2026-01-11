using UnityEngine;

[CreateAssetMenu(fileName = "KeyDefinition", menuName = "Keys/KeyDefinition")]
public class KeyDefinition : ScriptableObject
{
    [Header("ID")]
    public string keyId;

    [Header("UI")]
    public string displayName;
    [TextArea(1, 3)] public string description;

    [Header("Optional")]
    public Sprite icon;
}
