// using UnityEngine;
// using UnityEngine.Rendering.Universal;

// [RequireComponent(typeof(Rigidbody2D))]
// public class GuardScript : MonoBehaviour
// {
//     [Header("Target & Layers")]
//     public Transform player;
//     public LayerMask obstacleMask;

//     [Header("Run / Chase Settings")]
//     public bool canRun = true;
//     public float moveSpeed = 5f;
//     public float stoppingDistance = 0.4f; // Keep small so he stays glued to player

//     [Header("Sweep Settings")]
//     public float sweepAngle = 45f;
//     public int stopCount = 3;
//     public float moveDuration = 0.8f;
//     public float pauseDuration = 0.5f;

//     private Animator animator;
//     private Light2D visionLight;
//     private Transform lightTransform;
//     private Rigidbody2D rb;
//     private float baseZAngle;

//     private int currentIndex = 0;
//     private int direction = 1;
//     private float stateTimer = 0f;
//     private bool isPausing = false;
//     private bool isBlinded = false;
//     private bool isChasing = false;

//     private float fromAngle;
//     private float toAngle;

//     void Awake()
//     {
//         animator = GetComponent<Animator>();
//         rb = GetComponent<Rigidbody2D>();

//         rb.gravityScale = 0f;
//         rb.freezeRotation = true;

//         visionLight = GetComponentInChildren<Light2D>();
//         if (visionLight != null)
//         {
//             lightTransform = visionLight.transform;
//             baseZAngle = lightTransform.localEulerAngles.z;
//         }

//         stopCount = Mathf.Max(2, stopCount);
//         fromAngle = GetAngleForStop(0);
//         toAngle = GetAngleForStop(1);
//     }

//     void Update()
//     {
//         if (!isBlinded)
//         {
//             if (CanSeePlayer())
//             {
//                 if (canRun) isChasing = true;
//             }

//             if (isChasing && player != null && lightTransform != null)
//             {
//                 Vector2 guardPos2D = transform.position;
//                 Vector2 playerPos2D = player.position;

//                 if (Vector2.Distance(guardPos2D, playerPos2D) > 0.1f)
//                 {
//                     Vector2 dirToPlayer = (playerPos2D - guardPos2D).normalized;
//                     float angle = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg - 90f;
//                     lightTransform.rotation = Quaternion.Euler(0, 0, angle);
//                 }
//             }
//             else
//             {
//                 HandleSweep();
//             }
//         }

//         // Animator updates
//         if (animator != null)
//         {
//             animator.SetFloat("Speed", rb.linearVelocity.magnitude);
//             if (lightTransform != null)
//             {
//                 animator.SetFloat("Right", lightTransform.up.x);
//                 animator.SetFloat("Up", lightTransform.up.y);
//             }
//         }
//     }

//     void FixedUpdate()
//     {
//         if (canRun && isChasing && !isBlinded && player != null)
//         {
//             Vector2 guardPos2D = rb.position;
//             Vector2 playerPos2D = player.position;
//             float dist = Vector2.Distance(guardPos2D, playerPos2D);

//             if (dist > stoppingDistance)
//             {
//                 // Directly move physics body toward player live position
//                 Vector2 targetPos = Vector2.MoveTowards(guardPos2D, playerPos2D, moveSpeed * Time.fixedDeltaTime);
//                 rb.MovePosition(targetPos);

//                 // Recalculate linearVelocity for Animator Speed parameter
//                 rb.linearVelocity = (targetPos - guardPos2D) / Time.fixedDeltaTime;
//             }
//             else
//             {
//                 rb.linearVelocity = Vector2.zero;
//             }
//         }
//     }

//     private void HandleSweep()
//     {
//         if (isChasing || lightTransform == null) return;

//         stateTimer += Time.deltaTime;

//         if (isPausing)
//         {
//             if (stateTimer >= pauseDuration)
//             {
//                 isPausing = false;
//                 stateTimer = 0f;

//                 fromAngle = GetAngleForStop(currentIndex);
//                 AdvanceIndex();
//                 toAngle = GetAngleForStop(currentIndex);
//             }
//         }
//         else
//         {
//             float t = Mathf.Clamp01(stateTimer / Mathf.Max(0.01f, moveDuration));
//             float smoothT = Mathf.SmoothStep(0f, 1f, t);

//             float currentAngle = Mathf.LerpAngle(fromAngle, toAngle, smoothT);
//             lightTransform.localRotation = Quaternion.Euler(0f, 0f, currentAngle);

//             if (t >= 1f)
//             {
//                 isPausing = true;
//                 stateTimer = 0f;
//             }
//         }
//     }

//     private void AdvanceIndex()
//     {
//         currentIndex += direction;

//         if (currentIndex >= stopCount)
//         {
//             currentIndex = stopCount - 2;
//             direction = -1;
//         }
//         else if (currentIndex < 0)
//         {
//             currentIndex = 1;
//             direction = 1;
//         }
//     }

//     private float GetAngleForStop(int index)
//     {
//         float totalArc = sweepAngle * 2f;
//         float stepSize = totalArc / (stopCount - 1);
//         return baseZAngle - sweepAngle + (index * stepSize);
//     }

//     public bool CanSeePlayer()
//     {
//         if (visionLight == null || player == null || lightTransform == null) return false;

//         float viewRadius = visionLight.pointLightOuterRadius;
//         float viewAngle = visionLight.pointLightOuterAngle;

//         Vector2 lightPos = lightTransform.position;
//         Vector2 playerPos = player.position;
//         Vector2 dirToPlayer = (playerPos - lightPos).normalized;
//         float distanceToPlayer = Vector2.Distance(lightPos, playerPos);

//         if (distanceToPlayer <= viewRadius)
//         {
//             if (Vector2.Angle(lightTransform.up, dirToPlayer) <= viewAngle / 2f)
//             {
//                 RaycastHit2D hit = Physics2D.Raycast(lightPos, dirToPlayer, distanceToPlayer, obstacleMask);
//                 if (hit.collider == null)
//                 {
//                     return true;
//                 }
//             }
//         }

//         return false;
//     }

//     public void Blind(float duration)
//     {
//         isBlinded = true;
//         if (rb != null) rb.linearVelocity = Vector2.zero;
//         Invoke("RecoverVision", duration);
//     }

//     private void RecoverVision()
//     {
//         isBlinded = false;
//     }
// }
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Rigidbody2D))]
public class GuardScript : MonoBehaviour
{
    [Header("Target & Layers")]
    private Transform player;
    public LayerMask obstacleMask;

    [Header("Run / Chase Settings")]
    public bool canRun = true;           // Toggle running ability per guard instance
    public float runAcceleration = 25f;  // Push force towards player
    public float maxSpeed = 6f;          // Top running speed
    public float linearDrag = 2f;        // Low drag = more slide/drift, High drag = snappy turns
    public float stoppingDistance = 0.8f; // Stops applying chase force when close to player

    [Header("Sweep Settings")]
    public float sweepAngle = 45f;       // Half-span angle
    public int stopCount = 3;            // Number of stops (Minimum 2)
    public float moveDuration = 0.8f;    // Time (seconds) spent traveling between stops
    public float pauseDuration = 0.5f;   // Seconds to pause at each stop

    private Animator animator;
    private Light2D visionLight;
    private Transform lightTransform;
    private Rigidbody2D rb;
    private float baseZAngle;

    // State Variables
    private int currentIndex = 0;
    private int direction = 1;
    private float stateTimer = 0f;
    private bool isPausing = false;
    private bool isBlinded = false;
    private bool isChasing = false;

    private float fromAngle;
    private float toAngle;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // Unity 6 Physics Setup
        rb.gravityScale = 0f;
        rb.linearDamping = linearDrag; // Drag/Damping controls drift speed
        rb.freezeRotation = true;

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
        if (!isBlinded)
        {
            if (CanSeePlayer())
            {
                if (canRun)
                {
                    isChasing = true;
                }
            }

            if (isChasing)
            {
                if (player != null && lightTransform != null)
                {
                    float distToPlayer = Vector2.Distance(transform.position, player.position);
                    if (distToPlayer > 0.3f) 
                    {
                        Vector2 dirToPlayer = ((Vector2)player.position - (Vector2)transform.position).normalized;
                        float angle = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg - 90f;
                        lightTransform.rotation = Quaternion.Euler(0, 0, angle);
                    }
                }
            }
            else
            {
                HandleSweep();
            }
        }

        // Animator updates
        if (animator != null)
        {
            float currentSpeed = rb.linearVelocity.magnitude;
            animator.SetFloat("Speed", currentSpeed);

            if (lightTransform != null)
            {
                animator.SetFloat("Right", lightTransform.up.x);
                animator.SetFloat("Up", lightTransform.up.y);
            }
        }
    }

    void FixedUpdate()
    {
        if (canRun && isChasing && !isBlinded && player != null)
        {
            float distToPlayer = Vector2.Distance(rb.position, (Vector2)player.position);

            if (distToPlayer > stoppingDistance)
            {
                Vector2 dirToPlayer = ((Vector2)player.position - rb.position).normalized;

                // Push guard using force physics
                rb.AddForce(dirToPlayer * runAcceleration);

                // Cap speed
                if (rb.linearVelocity.magnitude > maxSpeed)
                {
                    rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
                }
            }
            else
            {
                // Decelerate near player without jittering
                rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.fixedDeltaTime * 10f);
            }
        }
    }

    private void HandleSweep()
    {
        if (isChasing || lightTransform == null) return;

        stateTimer += Time.deltaTime;

        if (isPausing)
        {
            if (stateTimer >= pauseDuration)
            {
                isPausing = false;
                stateTimer = 0f;

                fromAngle = GetAngleForStop(currentIndex);
                AdvanceIndex();
                toAngle = GetAngleForStop(currentIndex);
            }
        }
        else
        {
            float t = Mathf.Clamp01(stateTimer / Mathf.Max(0.01f, moveDuration));
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            float currentAngle = Mathf.LerpAngle(fromAngle, toAngle, smoothT);
            lightTransform.localRotation = Quaternion.Euler(0f, 0f, currentAngle);

            if (t >= 1f)
            {
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

    public void Blind(float duration)
    {
        isBlinded = true;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        Invoke("RecoverVision", duration);
    }

    private void RecoverVision()
    {
        isBlinded = false;
    }
}