using System.Collections.Generic;
using UnityEngine;

public class ObjectiveBinderHost : MonoBehaviour
{
    private readonly Dictionary<string, IObjectiveBinder> binders = new();

    private static string Key(string questId, int objectiveIndex) => $"{questId}::{objectiveIndex}";

    public void AddBinder(string questId, int objectiveIndex, IObjectiveBinder binder, GameEventHub hub)
    {
        string k = Key(questId, objectiveIndex);

        // si ya existe, reemplaza limpiamente
        if (binders.TryGetValue(k, out var existing))
        {
            existing.Unbind(hub);
            binders.Remove(k);
        }

        binders.Add(k, binder);
        binder.Bind(hub);
    }

    public void RemoveBinder(string questId, int objectiveIndex, GameEventHub hub)
    {
        string k = Key(questId, objectiveIndex);

        if (binders.TryGetValue(k, out var binder))
        {
            binder.Unbind(hub);
            binders.Remove(k);
        }
    }

    private void OnDestroy()
    {
        // si se destruye el journal, no hace falta unbind (escena cambia), pero es seguro.
    }
}
