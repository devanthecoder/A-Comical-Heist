using UnityEngine;

public class CameraPlacement : MonoBehaviour
{
    Camera cam;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        float height = cam.orthographicSize * 2f;
        float width = height * cam.aspect;
        Vector3 ps = cam.WorldToViewportPoint(transform.position);
        if(ps.x < 0f || ps.x > 1f || ps.y < 0f || ps.y > 1f){
            int xDir = ps.x < 0f ? -1 : (ps.x > 1f ? 1 : 0);
            int yDir = ps.y < 0f ? -1 : (ps.y > 1f ? 1 : 0);
            Vector3 move = new Vector3(xDir * width, yDir * height, 0f);
            cam.transform.position += move;
        }
    }
}
