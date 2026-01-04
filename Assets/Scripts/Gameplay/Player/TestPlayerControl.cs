using UnityEngine;
using System.Collections;
public class TestPlayerControl : MonoBehaviour
{
    private bool coroutineComplete = false;
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerInputRouter>().DisableControl();
            StartCoroutine(wait(other));
        }
    }
    public void Notifier (Collider collision)
    {
        collision.gameObject.GetComponent<PlayerInputRouter>().EnableControl();
    }
    public IEnumerator wait(Collider other)
    {
        yield return new WaitForSeconds(2f);
        Debug.Log("entre aca");
        this.gameObject.SetActive(false);
        coroutineComplete = true;
        Notifier(other);
    }
}
