using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum NPCState
{
    Idle,
    Chase,
    Rush,
    ChaseToLastSeen
}

public class NPCController : MonoBehaviour
{
    public float visionRadius = 6f;
    public NPCState currentState = NPCState.Idle;
    public float speed = 3.5f;
    public Transform[] potentialTargets;
    public PlayerState playerState = PlayerState.Oni;
    public AudioClip npcDarumaFullClip;
    private float idleTimer = 0f;
    private Vector2 idleTarget;
    private Vector2 lastSeenPosition;
    private bool hasLastSeenPosition = false;
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;
    private float patrolSpeedFactor = 0.5f;
    public float patrolReachDistance = 0.3f;
    private Vector2 initialPosition;
    public float idleMoveRadius = 3f;
    public float idleSpeedMin = 0.3f;
    public float idleSpeedMax = 0.7f;

    void Start()
    {
        initialPosition = transform.position;
    }

    void Update()
    {
        if (GameManager.Instance.isDarumaMode)
        {
            return;
        }

        if (!GameManager.Instance.isDarumaMode && Random.value < 0.00167f)
        {
            GameManager.Instance.EnterDarumaMode(true);
            Debug.Log("[NPC_Oni] だるまモード開始！");
        }

        Transform closest = FindClosestHuman();
        float distance = closest != null ? Vector2.Distance(transform.position, closest.position) : Mathf.Infinity;

        if (closest != null)
        {
            currentState = (distance < 2f) ? NPCState.Rush : NPCState.Chase;
            lastSeenPosition = closest.position;
            hasLastSeenPosition = true;
        }
        else if (hasLastSeenPosition)
        {
            currentState = NPCState.ChaseToLastSeen;
        }
        else
        {
            currentState = NPCState.Idle;
        }

        switch (currentState)
        {
            case NPCState.Idle:
                DoIdle();
                break;
            case NPCState.Chase:
                DoChase(closest.position, speed);
                break;
            case NPCState.Rush:
                DoChase(closest.position, speed * 1.5f);
                break;
            case NPCState.ChaseToLastSeen:
                DoChase(lastSeenPosition, speed * 0.9f);
                if (Vector2.Distance(transform.position, lastSeenPosition) < 0.3f)
                {
                    hasLastSeenPosition = false;
                    currentState = NPCState.Idle;
                }
                break;
        }
    }

    void FixedUpdate()
    {
        if (GameManager.Instance.isDarumaMode)
        {
            GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        }
    }

    Transform FindClosestHuman()
    {
        List<Transform> visibleHumans = new List<Transform>();
        float closestDist = Mathf.Infinity;
        Transform closest = null;

        foreach (Transform target in potentialTargets)
        {
            PlayerController player = target.GetComponent<PlayerController>();
            NPCPlayerController npcPlayer = target.GetComponent<NPCPlayerController>();
            if ((player != null && player.currentState == PlayerState.Human) ||
                (npcPlayer != null && npcPlayer.currentState == PlayerState.Human))
            {
                float dist = Vector2.Distance(transform.position, target.position);
                if (dist <= visionRadius)
                {
                    RaycastHit2D hit = Physics2D.Linecast(transform.position, target.position, LayerMask.GetMask("Obstacle"));
                    Debug.DrawLine(transform.position, target.position, hit.collider == null ? Color.green : Color.red, 1f);
                    if (hit.collider == null)
                    {
                        visibleHumans.Add(target);
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            closest = target;
                        }
                    }
                    else
                    {
                        Debug.Log($"[NPC_Oni] Obstacle detected: {hit.collider.name}");
                    }
                }
            }
        }
        return visibleHumans.Count > 0 ? closest : null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, visionRadius);
    }

    void DoChase(Vector3 targetPosition, float chaseSpeed)
    {
        Vector2 dir = ((Vector2)targetPosition - (Vector2)transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, 1f, LayerMask.GetMask("Obstacle"));
        if (hit.collider == null)
        {
            transform.position += (Vector3)(dir * chaseSpeed * Time.deltaTime);
        }
        else
        {
            Vector2 rightDir = new Vector2(-dir.y, dir.x);
            transform.position += (Vector3)(rightDir * chaseSpeed * 0.5f * Time.deltaTime);
        }
    }

    void DoIdle()
    {
        if (patrolPoints.Length == 0)
        {
            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0f)
            {
                idleTarget = initialPosition + Random.insideUnitCircle * idleMoveRadius;
                idleTimer = Random.Range(1.5f, 3f);
            }
            Vector2 dir = (idleTarget - (Vector2)transform.position).normalized;
            transform.position += (Vector3)(dir * (speed * patrolSpeedFactor) * Time.deltaTime);
            return;
        }

        Transform targetPoint = patrolPoints[currentPatrolIndex];
        Vector2 direction = ((Vector2)targetPoint.position - (Vector2)transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 1f, LayerMask.GetMask("Obstacle"));
        if (hit.collider == null)
        {
            transform.position += (Vector3)(direction * (speed * patrolSpeedFactor) * Time.deltaTime);
        }
        else
        {
            Vector2 rightDir = new Vector2(-direction.y, direction.x);
            transform.position += (Vector3)(rightDir * speed * patrolSpeedFactor * 0.5f * Time.deltaTime);
        }

        if (Vector2.Distance(transform.position, targetPoint.position) < patrolReachDistance)
        {
            int previousIndex = currentPatrolIndex;
            while (currentPatrolIndex == previousIndex)
            {
                currentPatrolIndex = Random.Range(0, patrolPoints.Length);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (GameManager.Instance.isDarumaMode)
        {
            return; // ダルマモード中は感染しない
        }

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null && playerState == PlayerState.Oni && player.currentState == PlayerState.Human)
        {
            player.Infect();
            Debug.Log($"[NPC_Oni] {player.gameObject.name} を感染させた！");
        }

        NPCPlayerController npcPlayer = other.GetComponent<NPCPlayerController>();
        if (npcPlayer != null && playerState == PlayerState.Oni && npcPlayer.currentState == PlayerState.Human)
        {
            npcPlayer.Infect();
            Debug.Log($"[NPC_Oni] {npcPlayer.gameObject.name} を感染させた！");
        }
    }
}