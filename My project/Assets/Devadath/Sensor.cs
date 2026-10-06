using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Sensor : MonoBehaviour
{
    [Header("Beam Reference")]
    public BendingLightBeam lightBeam;   // Reference to your player's BendingLightBeam script
    public Animation sensorAnimation; // Reference to the sensor's animation component

    [Header("Door Target")]
    public SimpleTiledDoor targetDoor;   // Reference to the SimpleTiledDoor script

    [Header("Settings")]
    public Collider2D sensorCollider;
    private bool isTriggered = false;

    private void Awake()
    {
        if (lightBeam == null)
        {
            lightBeam = FindFirstObjectByType<BendingLightBeam>();
        }

        if (sensorCollider == null)
        {
            sensorCollider = GetComponentInChildren<Collider2D>();
        }
    }

    void Update()
    {
        bool currentlyHit = CheckIfBeamHitsSensor();

        // State changed: Active beam entered sensor
        if (currentlyHit && !isTriggered)
        {
            isTriggered = true;
            sensorAnimation["SensorActivated"].time = 0f; // Reset the animation to the start
            sensorAnimation.Play("SensorActivated");
            OpenDoor();
        }
        // State changed: Active beam exited sensor, finished retracting, or was broken
        else if (!currentlyHit && isTriggered)
        {
            isTriggered = false;
            sensorAnimation["SensorDeactivated"].time = 0f; // Reset the animation to the start
            sensorAnimation.Play("SensorDeactivated");
            CloseDoor();
        }
    }

    private bool CheckIfBeamHitsSensor()
    {
        if (lightBeam == null) return false;

        List<Vector3> points = lightBeam.GetPoints();
        // Return false if beam is destroyed, broken, or completely retracted
        if (points == null || points.Count < 2) return false;

        // Check each line segment between consecutive points
        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector2 p1 = points[i];
            Vector2 p2 = points[i + 1];

            if (SegmentIntersectsSensor(p1, p2))
            {
                return true; 
            }
        }

        return false;
    }

    private bool SegmentIntersectsSensor(Vector2 p1, Vector2 p2)
    {
        // 1. Check if either endpoint is inside the sensor trigger
        if (sensorCollider.OverlapPoint(p1) || sensorCollider.OverlapPoint(p2))
            return true;

        // 2. Check if the line segment passes through the trigger
        Vector2 dir = p2 - p1;
        float distance = dir.magnitude;
        if (distance <= 0.0001f) return false;

        RaycastHit2D[] hits = new RaycastHit2D[1];
        int hitCount = sensorCollider.Raycast(dir.normalized, hits, distance);
        return hitCount > 0;
    }

    private void OpenDoor()
    {
        if (targetDoor != null)
        {
            targetDoor.OpenDoor();
        }
    }

    private void CloseDoor()
    {
        if (targetDoor != null)
        {
            targetDoor.CloseDoor();
        }
    }
}