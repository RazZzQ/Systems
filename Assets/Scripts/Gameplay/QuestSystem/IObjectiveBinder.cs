using UnityEngine;

public interface IObjectiveBinder
{
    void Bind(GameEventHub hub);
    void Unbind(GameEventHub hub);
}
