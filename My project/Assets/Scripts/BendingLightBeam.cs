using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(LineRenderer))]
public class BendingLightBeam : MonoBehaviour
{
    public BatteryScript Battery;
    public float deplishRate = 1f; // Rate at which the battery depletes when bending the light beam
    public LayerMask ObstacleLayerMask; // Layer mask for obstacles
    public PlayerMovement playerMovement; // Reference to the PlayerMovement script
    public Transform Bob; // Reference to the Bob transform
    public float beamSpeed = 5f;
    public float turnRate = 1f;
    [Header("Beam Breaking")]
    public GameObject BrokenBeamPrefab; // Reference to the BreakableBeamPreset scriptable object
    [Header("Animations")]
    public Animation torch_anim;
    public float ClearingTime = 1f; // Time in seconds to clear the line renderer


    private LineRenderer lineRenderer;
    private bool isBending = false;
    private bool canBend = true;
    [SerializeField] private bool bendingUnlocked;
    [SerializeField] private bool bridgeUnlocked;
    private Vector3 currPoint, currDirection;
    private float batterBeforeBendingStarted;
    List<Vector3> points = new List<Vector3>();
    List<GameObject> BrokenBeams = new List<GameObject>();
    public List<Vector3> GetPoints() => points;
    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (bendingUnlocked && !isBending && canBend)
            {
                torch_anim.Play("CloseLight");
                Invoke("InitBending", torch_anim["CloseLight"].length); // Delay the bending initiation until the animation finishes
            }
            else if (isBending)
            {
                CleanBending();
            }
        }
        if (Input.GetKeyDown(KeyCode.V) && bridgeUnlocked && isBending)
        {
            CleanBending();
            BreakLightBeam();
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            if (IsPlayerOnVoid()) return;

            foreach (GameObject brokenBeam in BrokenBeams)
            {
                brokenBeam.GetComponent<Animation>().Play("BrokenBeamDespawn"); // Play the fade-out animation for each broken beam
                Destroy(brokenBeam, 1f);
            }

            BrokenBeams.Clear();
            Battery.SetBlockedBatteryLife(0f); // Reset blocked battery life when clearing broken beams
        }
    }

    public void UnlockBending()
    {
        bendingUnlocked = true;
    }

    public void UnlockBridge()
    {
        bridgeUnlocked = true;
    }

    void InitBending()
    {
        isBending = true; // Set bending flag to true

        playerMovement.enabled = false; // Disable player movement when bending starts
        playerMovement.Stop(); // Stop the player's movement immediately when bending starts
        Bob.gameObject.SetActive(true); // Activate Bob when bending starts
        currPoint = transform.position; // Reset the current point to the start point when toggling bending
        currDirection = transform.up; // Reset the current direction to the initial direction
        points.Add(currPoint); // Add the start point to the list of points
        lineRenderer.SetPositions(points.ToArray());
        canBend = false;

        batterBeforeBendingStarted = Battery.GetBatteryLife(); // Store the battery life before bending starts
    }

    void FixedUpdate()
    {
        if (isBending)
        {
            SimulateLightBeam();            
            lineRenderer.enabled = true; // Enable the line renderer when bending starts
        }
    }

    void CleanBending()
    {
        isBending = false; // Set bending flag to false
        playerMovement.enabled = true; // Enable player movement when bending stops
        torch_anim.Play("OpenLight"); // Play the OpenLight animation when bending stops
        StartCoroutine(ClearLineSmoothly(points.Count, () => {
            ResetBeam(); // Reset the beam after clearing the line renderer
        }));
        
    }

    void ResetBeam()
    {
        isBending = false; // Set bending flag to false
        if (playerMovement != null) playerMovement.enabled = true;
        // Bob.gameObject.SetActive(false); // Deactivate Bob when bending stops
        Bob.gameObject.SetActive(false); // Deactivate Bob when bending stops
        points.Clear(); // Clear the points when stopping the bending
        lineRenderer.positionCount = 0; // Reset the line renderer
        lineRenderer.enabled = false; // Disable the line renderer when not bending
        canBend = true;
    }


    private void SimulateLightBeam()
    {
        float steer = 0f;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) steer += 1f;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) steer -= 1f;

        if (steer != 0f)
        {
            // 1. Scale degrees by turnRate and Time.deltaTime inside Euler
            float angle = steer * turnRate * Time.fixedDeltaTime;
            currDirection = Quaternion.Euler(0, 0, angle) * currDirection;
            
            // 2. Keep the magnitude exactly 1 to avoid floating-point decay
            currDirection.Normalize(); 
        }
        currPoint += currDirection.normalized * Time.fixedDeltaTime * beamSpeed; // Move the current point in the current direction
        if (Bob != null) Bob.position = currPoint;
        points.Add(currPoint);

        CheckForCollisions();

        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());

        Battery.DepleteBattery(deplishRate * Time.fixedDeltaTime); // Deplete battery over time while bending
        // Check if Battery is depleted and stop bending if it is
        if (Battery.GetBatteryLife() <= 0)
        {
            CleanBending();
        }
    }

    IEnumerator ClearLineSmoothly(int pointsToClear, Action onComplete)
    {
        for (int i = 0; i < pointsToClear && points.Count > 0; i++)
        {
            points.RemoveAt(0);
            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPositions(points.ToArray());
            yield return new WaitForSeconds(ClearingTime* 1.0f / pointsToClear); // Wait for 1 second
        }

        onComplete?.Invoke(); // Call the onComplete action if it's not null
    }

    void BreakLightBeam()
    {
        // 1. Instantiate the broken beam
        GameObject brokenBeam = Instantiate(BrokenBeamPrefab, Vector3.zero, Quaternion.identity); 

        LineRenderer brokenLineRenderer = brokenBeam.GetComponent<LineRenderer>();
        brokenLineRenderer.positionCount = points.Count;
        brokenLineRenderer.SetPositions(points.ToArray());

        // 2. Add a PolygonCollider2D instead of EdgeCollider2D
        PolygonCollider2D polyCollider = brokenBeam.AddComponent<PolygonCollider2D>();

        // 3. Convert line points into a closed ribbon polygon with thickness
        float thickness = brokenLineRenderer.startWidth > 0 ? brokenLineRenderer.startWidth : 0.5f;
        Vector2[] ribbonPoints = GenerateRibbonPoints(points, thickness * (2f/3f));
        polyCollider.SetPath(0, ribbonPoints);

        Tilemap voidTilemap = FindVoidTilemap(points);
        CompositeCollider2D voidComposite = EnsureVoidComposite(voidTilemap);

        // Parent to the target tilemap so its collider joins that composite.
        if (voidTilemap != null && voidComposite != null)
        {
            brokenBeam.transform.SetParent(voidTilemap.transform, true);

            polyCollider.compositeOperation = Collider2D.CompositeOperation.Difference;
            polyCollider.compositeOrder = 1;
            Physics2D.SyncTransforms();
            voidComposite.GenerateGeometry();
        }
        else
        {
            Debug.LogError("Cannot create a bridge: no active Void Tilemap with a CompositeCollider2D was found.", this);
        }

        BrokenBeams.Add(brokenBeam);
        ResetBeam(); 
        Battery.BlockBatteryLife(batterBeforeBendingStarted - Battery.GetBatteryLife());
    }

    private Tilemap FindVoidTilemap(List<Vector3> beamPoints)
    {
        Tilemap bestTilemap = null;
        int bestPointCount = 0;

        foreach (Tilemap tilemap in FindObjectsByType<Tilemap>(FindObjectsSortMode.None))
        {
            if (!tilemap.gameObject.activeInHierarchy || tilemap.name != "Tilemap_Void") continue;

            int pointCount = 0;
            foreach (Vector3 point in beamPoints)
            {
                if (tilemap.HasTile(tilemap.WorldToCell(point))) pointCount++;
            }

            if (pointCount > bestPointCount)
            {
                bestTilemap = tilemap;
                bestPointCount = pointCount;
            }
        }

        if (bestTilemap != null) return bestTilemap;

        GameObject taggedVoid = GameObject.FindWithTag("Void");
        return taggedVoid != null ? taggedVoid.GetComponent<Tilemap>() : null;
    }

    private CompositeCollider2D EnsureVoidComposite(Tilemap tilemap)
    {
        if (tilemap == null) return null;

        GameObject tilemapObject = tilemap.gameObject;
        Rigidbody2D body = tilemapObject.GetComponent<Rigidbody2D>();
        if (body == null) body = tilemapObject.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Static;
        body.gravityScale = 0f;

        CompositeCollider2D composite = tilemapObject.GetComponent<CompositeCollider2D>();
        if (composite == null) composite = tilemapObject.AddComponent<CompositeCollider2D>();

        TilemapCollider2D tilemapCollider = tilemapObject.GetComponent<TilemapCollider2D>();
        if (tilemapCollider == null) tilemapCollider = tilemapObject.AddComponent<TilemapCollider2D>();
        tilemapCollider.compositeOperation = Collider2D.CompositeOperation.Merge;

        Physics2D.SyncTransforms();
        composite.GenerateGeometry();
        return composite;
    }

    private bool IsPlayerOnVoid()
    {
        if (playerMovement == null) return false;

        Collider2D playerCollider = playerMovement.GetComponent<Collider2D>();
        if (playerCollider == null) return false;

        Physics2D.SyncTransforms();
        Bounds playerBounds = playerCollider.bounds;
        Vector2[] footPoints =
        {
            new Vector2(playerBounds.center.x, playerBounds.min.y - 0.02f),
            new Vector2(playerBounds.min.x + 0.05f, playerBounds.min.y - 0.02f),
            new Vector2(playerBounds.max.x - 0.05f, playerBounds.min.y - 0.02f)
        };

        foreach (Tilemap tilemap in FindObjectsByType<Tilemap>(FindObjectsSortMode.None))
        {
            if (!tilemap.gameObject.activeInHierarchy || tilemap.name != "Tilemap_Void") continue;

            foreach (Vector2 footPoint in footPoints)
            {
                if (tilemap.HasTile(tilemap.WorldToCell(footPoint))) return true;
            }
        }

        return false;
    }

    // Helper method: Creates a closed polygon shape wrapping around line points
    private Vector2[] GenerateRibbonPoints(List<Vector3> linePoints, float offset)
    {
        if (linePoints.Count < 2) return new Vector2[0];

        List<Vector2> leftSide = new List<Vector2>();
        List<Vector2> rightSide = new List<Vector2>();

        for (int i = 0; i < linePoints.Count; i++)
        {
            Vector2 current = linePoints[i];
            Vector2 dir;

            if (i == 0)
                dir = ((Vector2)linePoints[1] - current).normalized;
            else if (i == linePoints.Count - 1)
                dir = (current - (Vector2)linePoints[i - 1]).normalized;
            else
                dir = ((Vector2)linePoints[i + 1] - (Vector2)linePoints[i - 1]).normalized;

            Vector2 normal = new Vector2(-dir.y, dir.x) * offset;

            leftSide.Add(current + normal);
            rightSide.Add(current - normal);
        }

        // Combine left side (forward) and right side (reversed) into a closed loop
        rightSide.Reverse();
        leftSide.AddRange(rightSide);
        return leftSide.ToArray();
    }

    private void CheckForCollisions()
    {
        Collider2D hit = Physics2D.OverlapCircle(currPoint, 0.1f, ObstacleLayerMask, 0, 0);
        if (hit != null)
        {
            isBending = false; // Stop bending on collision
            CleanBending();
        }
    }
}