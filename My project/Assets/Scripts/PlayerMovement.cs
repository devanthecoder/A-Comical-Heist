using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float turnSpeed = 5f;
    public Transform lightHolder;
    public Animator animator;
    public BendingLightBeam bendingLightBeam; // Reference to the BendingLightBeam script

    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    Vector3 normalized;

    bool isCaught = false; // Flag to check if the player is caught
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isCaught)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }  // If the player is caught, skip movement and rotation
            
        var hor = Input.GetAxisRaw("Horizontal");
        var vert = Input.GetAxisRaw("Vertical");
        normalized = new Vector3(hor, vert, 0).normalized;
        if(normalized.magnitude > 0) lightHolder.localRotation = Quaternion.Lerp(lightHolder.localRotation, Quaternion.LookRotation(Vector3.forward, normalized), Time.deltaTime * turnSpeed);
        // transform.Translate(normalized * Time.deltaTime * speed);
        
        animator.SetFloat("Right", Vector2.Dot(lightHolder.up, Vector2.right));
        animator.SetFloat("Up", Vector2.Dot(lightHolder.up, Vector2.up));

        if (Vector2.Dot(lightHolder.up, Vector2.right) < 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (Vector2.Dot(lightHolder.up, Vector2.right) > 0)
        {
            spriteRenderer.flipX = false;
        }
    }
    void FixedUpdate()
    {
        animator.SetFloat("Speed", rb.linearVelocity.magnitude);
        rb.linearVelocity = normalized * speed;
        // rb.MovePosition(rb.position + new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")) * speed * Time.fixedDeltaTime);
    }

    public void Stop()
    {
        rb.linearVelocity = Vector2.zero;
        animator.SetFloat("Speed", 0f);
    }

    public void GotCaught()
    {
        isCaught = true; // Set the caught flag to true
        Stop();
        bendingLightBeam.enabled = false; // Disable the BendingLightBeam script
        Invoke("AfterCaught", 1f); // Call AfterCaught after 1 second
    }

    void AfterCaught()
    {
        GameManager.instance.GotCaught(); // Call the GotCaught method in GameManager
    }
}
