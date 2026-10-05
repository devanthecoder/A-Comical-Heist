using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GuardScript : MonoBehaviour
{
    [Header("Target & Layers")]
    public Transform player;
    public LayerMask obstacleMask;

    [Header("Sweep Settings")]
    public float sweepAngle = 45f;       // Half-span angle
    public int stopCount = 3;            // Number of stops (Minimum 2)
    public float moveDuration = 0.8f;    // Time (seconds) spent traveling between stops
    public float pauseDuration = 0.5f;   // Seconds to pause at each stop

    private Light2D visionLight;
    private Transform lightTransform;
    private float baseZAngle;

    // State Variables
    private int currentIndex = 0;
    private int direction = 1;
    private float stateTimer = 0f;
    private bool isPausing = false;

    private float fromAngle;
    private float toAngle;

    void Awake()
    {
        visionLight = GetComponentInChildren<Light2D>();
        if (visionLight != null)
        {
            lightTransform = visionLight.transform;
            baseZAngle = lightTransform.localEulerAngles.z;
        }

        stopCount = Mathf.Max(2, stopCount);
        
        // Initialize first move targets
        fromAngle = GetAngleForStop(0);
        toAngle = GetAngleForStop(1);
    }

    void Update()
    {
        HandleSweep();

        if (CanSeePlayer())
        {
            Debug.Log("Player Detected!");
        }
    }

    private void HandleSweep()
    {
        if (lightTransform == null) return;

        stateTimer += Time.deltaTime;

        if (isPausing)
        {
            if (stateTimer >= pauseDuration)
            {
                // Done pausing, set up target for next step
                isPausing = false;
                stateTimer = 0f;

                fromAngle = GetAngleForStop(currentIndex);
                AdvanceIndex();
                toAngle = GetAngleForStop(currentIndex);
            }
        }
        else
        {
            // Normalize travel time from 0.0 to 1.0
            float t = Mathf.Clamp01(stateTimer / Mathf.Max(0.01f, moveDuration));

            // SmoothStep restores the organic deceleration as it approaches the stop
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            float currentAngle = Mathf.LerpAngle(fromAngle, toAngle, smoothT);
            lightTransform.localRotation = Quaternion.Euler(0f, 0f, currentAngle);

            if (t >= 1f)
            {
                // Reached target angle, enter pause state
                isPausing = true;
                stateTimer = 0f;
            }
        }
    }

    private void AdvanceIndex()
    {
        currentIndex += direction;

        if (currentIndex >= stopCount)
        {
            currentIndex = stopCount - 2;
            direction = -1;
        }
        else if (currentIndex < 0)
        {
            currentIndex = 1;
            direction = 1;
        }
    }

    private float GetAngleForStop(int index)
    {
        float totalArc = sweepAngle * 2f;
        float stepSize = totalArc / (stopCount - 1);
        return baseZAngle - sweepAngle + (index * stepSize);
    }

    public bool CanSeePlayer()
    {
        if (visionLight == null || player == null || lightTransform == null) return false;

        float viewRadius = visionLight.pointLightOuterRadius;
        float viewAngle = visionLight.pointLightOuterAngle;

        Vector2 lightPos = lightTransform.position;
        Vector2 dirToPlayer = ((Vector2)player.position - lightPos).normalized;
        float distanceToPlayer = Vector2.Distance(lightPos, player.position);

        if (distanceToPlayer <= viewRadius)
        {
            if (Vector2.Angle(lightTransform.up, dirToPlayer) <= viewAngle / 2f)
            {
                RaycastHit2D hit = Physics2D.Raycast(lightPos, dirToPlayer, distanceToPlayer, obstacleMask);
                if (hit.collider == null)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
