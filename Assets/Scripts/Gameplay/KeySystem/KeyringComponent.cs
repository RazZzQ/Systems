using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

[System.Serializable] public class KeyEvent : UnityEvent<KeyDefinition>{}
[System.Serializable] public class KeyIdEvent : UnityEvent<string>{}

public class KeyringComponent : MonoBehaviour, IKeyring
{
    [Header("Events")]
    public KeyEvent onKeyAdded;
    public KeyEvent onKeyConsumed;
    public KeyIdEvent onKeyAddFailed; //por si se intenta agregar null/duplicada

    //guardamos por ID (rapido y simple)
    private readonly HashSet<string> keys = new HashSet<string>();
    private readonly Dictionary<string, KeyDefinition> defs = new Dictionary<string, KeyDefinition>();

    public bool HasKey(string keyId) => !string.IsNullOrEmpty(keyId) && keys.Contains(keyId);

    // Util si quieres mostrar icono/nombre en eventos: registras el definition
    public bool RegisterDefinition(KeyDefinition def)
    {
        if(def == null || string.IsNullOrEmpty(def.keyId)) return false;
        defs[def.keyId] = def;
        return true;
    }

    public bool TryAddKey(string keyId)
    {
        if(string.IsNullOrEmpty(keyId)) {onKeyAddFailed?.Invoke(keyId); return false;}
        bool added = keys.Add(keyId);
        if(added) onKeyAdded?.Invoke(GetDef(keyId));
        else onKeyAddFailed?.Invoke(keyId);
        return added;
    }

    public bool TryConsumeKey(string keyId)
    {
        if(!HasKey(keyId)) return false;
        keys.Remove(keyId);
        onKeyConsumed?.Invoke(GetDef(keyId));
        return true;
    }

    private KeyDefinition GetDef(string keyId)
    {
        if(string.IsNullOrEmpty(keyId)) return null;
        defs.TryGetValue(keyId, out var def);
        return def;
    }
}
