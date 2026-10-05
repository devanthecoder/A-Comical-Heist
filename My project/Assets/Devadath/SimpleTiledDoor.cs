using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class SimpleTiledDoor : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private float fullWidth = 1.5f;
    [SerializeField] private float speed = 5f;
    public bool isOpen = false;

    private SpriteRenderer sr;
    private BoxCollider2D col;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();
        fullWidth = sr.size.x;
    }

    private void Update()
    {
        float targetWidth = isOpen ? 0f : fullWidth;
        float currentWidth = Mathf.MoveTowards(sr.size.x, targetWidth, speed * Time.deltaTime);

        // Crop visual sprite
        sr.size = new Vector2(currentWidth, sr.size.y);

        // Resize & offset collider to match remaining door
        col.size = sr.size;
        col.offset = new Vector2(currentWidth * 0.5f, col.offset.y);
    }

    public void ToggleDoor() => isOpen = !isOpen;
    public void OpenDoor() => isOpen = true;
    public void CloseDoor() => isOpen = false;
}