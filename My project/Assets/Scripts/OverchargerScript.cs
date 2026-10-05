using UnityEngine;

public class OverchargerScript : MonoBehaviour
{
    public float overchargeRange = 9f;
    public float blindDuration = 5f; // Duration for which the guards will be blinded
    Animation anim;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = GetComponent<Animation>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            anim["Overcharge"].time = 0f;
            anim.Play();
            Blind();
        }
    }

    void Blind()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, overchargeRange);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Enemy") 
                && Physics2D.Linecast(transform.position, collider.transform.position).collider == collider)
            {
                GuardScript enemy = collider.GetComponent<GuardScript>();
                if (enemy != null)
                {
                    enemy.Blind(blindDuration);
                }
            }
        }
    }
}
