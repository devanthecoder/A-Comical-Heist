using UnityEngine;
using UnityEngine.Events;

public class EnableOnTrigger : MonoBehaviour
{
    public GameObject target;
    public UnityEvent OnTrigger;
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            target.SetActive(true);
            OnTrigger.Invoke();
        }
    }
}
