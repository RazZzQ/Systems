using UnityEngine;

public class DebugGiveApple : MonoBehaviour
{
    [SerializeField] private GameEventHub hub;

    // Presiona K para sumar 1 apple
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
            hub.RaiseItemAdded("apple", 1);
    }
}
