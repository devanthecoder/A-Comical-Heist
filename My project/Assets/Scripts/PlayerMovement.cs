using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float turnSpeed = 5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var hor = Input.GetAxisRaw("Horizontal");
        var vert = Input.GetAxisRaw("Vertical");
        var normalized = new Vector3(hor, vert, 0).normalized;
        if(normalized.magnitude > 0) transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.LookRotation(Vector3.forward, normalized), Time.deltaTime * turnSpeed);
        transform.Translate(normalized * Time.deltaTime * speed, Space.World);
    }
}
