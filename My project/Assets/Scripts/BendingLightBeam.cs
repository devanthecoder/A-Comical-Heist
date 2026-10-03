using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Presets;

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
    private Vector3 currPoint, currDirection;
    List<Vector3> points = new List<Vector3>();
    List<GameObject> BrokenBeams = new List<GameObject>();
    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isBending && canBend)
            {
                torch_anim.Play("CloseLight");
                Invoke("InitBending", torch_anim["CloseLight"].length); // Delay the bending initiation until the animation finishes
            } else
            {
                CleanBending();
            }
        }
        if (Input.GetKeyDown(KeyCode.V) && isBending)
        {
            BreakLightBeam();
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            foreach (GameObject brokenBeam in BrokenBeams)
            {
                brokenBeam.GetComponent<Animation>().Play("BrokenBeamDespawn"); // Play the fade-out animation for each broken beam
                Destroy(brokenBeam, 1f);
            }
            BrokenBeams.Clear();
            Battery.BlockBatteryLife(0f); // Reset blocked battery life when clearing broken beams
        }
    }

    void InitBending()
    {
        isBending = true; // Set bending flag to true

        playerMovement.enabled = false; // Disable player movement when bending starts
        // Bob.gameObject.SetActive(true); // Activate Bob when bending starts
        currPoint = transform.position; // Reset the current point to the start point when toggling bending
        currDirection = transform.up; // Reset the current direction to the initial direction
        points.Add(currPoint); // Add the start point to the list of points
        lineRenderer.SetPositions(points.ToArray());
        canBend = false;
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
        StartCoroutine(ClearLineSmoothly(points.Count, () => {
            ResetBeam(); // Reset the beam after clearing the line renderer
        }));
        
    }

    void ResetBeam()
    {
        isBending = false; // Set bending flag to false
        playerMovement.enabled = true; // Enable player movement when bending stops
        // Bob.gameObject.SetActive(false); // Deactivate Bob when bending stops
        points.Clear(); // Clear the points when stopping the bending
        lineRenderer.positionCount = 0; // Reset the line renderer
        lineRenderer.enabled = false; // Disable the line renderer when not bending
        torch_anim.Play("OpenLight"); // Play the OpenLight animation when bending stops
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
        Bob.position = currPoint; // Update Bob's position to the current point
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
        for (int i = 0; i < pointsToClear; i++)
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
        GameObject brokenBeam = Instantiate(BrokenBeamPrefab, Vector3.zero, Quaternion.identity); // Create a new GameObject for the broken beam
        LineRenderer brokenLineRenderer = brokenBeam.GetComponent<LineRenderer>();
        brokenLineRenderer.positionCount = points.Count;
        brokenLineRenderer.SetPositions(points.ToArray());
        // EdgeCollider2D brokenEdgeCollider = brokenBeam.AddComponent<EdgeCollider2D>();
        // brokenEdgeCollider.SetPoints(points.ConvertAll(p => (Vector2)p));
        // brokenEdgeCollider.edgeRadius = brokenLineRenderer.startWidth / 2f; // Set the edge radius to half the line width
        BrokenBeams.Add(brokenBeam);
        ResetBeam(); // Reset the beam after breaking it
        Battery.BlockBatteryLife(100f-Battery.GetBatteryLife());
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