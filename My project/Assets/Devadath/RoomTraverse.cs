using UnityEngine;

public class RoomTraverse : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void OnTriggerEnter2D(Collider2D other){
        if(other.CompareTag("Player")){
            Camera mainCam = Camera.main;
            float height = mainCam.orthographicSize * 2f;
            float width = height * mainCam.aspect;
            Vector3 move = new Vector3(transform.right.x * width, transform.right.y * height, 0f);
            mainCam.transform.position += move;
        }
    }
}
