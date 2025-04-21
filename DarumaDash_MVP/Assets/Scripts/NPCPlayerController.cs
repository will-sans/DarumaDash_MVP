using UnityEngine;

public class NPCPlayerController : MonoBehaviour
{
    public PlayerState currentState = PlayerState.Human;
    public float moveSpeed = 5f;
    public Rigidbody2D rb { get; private set; }
    public float visionRadius = 6f;
    private Vector2 movement;
    private float moveTimer = 0f;
    private Vector2 moveTarget;
    private Transform nearestOni;
    private bool isMoving = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        moveTarget = transform.position;
        moveTimer = Random.Range(0.5f, 1.5f);
    }

    void Update()
    {
        if (GameManager.Instance.isDarumaMode)
        {
            if (!GameManager.Instance.isStopPhase)
            {
                isMoving = true;
            }
            else
            {
                isMoving = false;
                movement = Vector2.zero;
                rb.linearVelocity = Vector2.zero;
            }
        }
        else
        {
            isMoving = true;
        }

        if (!isMoving)
        {
            movement = Vector2.zero; 
            rb.linearVelocity = Vector2.zero;
            return;
        }

        nearestOni = FindNearestOni();
        moveTimer -= Time.deltaTime;
        if (moveTimer <= 0f)
        {
            if (currentState == PlayerState.Oni && nearestOni != null)
            {
                moveTarget = nearestOni.position;
            }
            else if (currentState == PlayerState.Human && nearestOni != null)
            {
                Vector2 hidePoint = FindHidePoint(nearestOni.position);
                moveTarget = hidePoint != Vector2.zero ? hidePoint : (Vector2)transform.position - ((Vector2)nearestOni.position - (Vector2)transform.position).normalized * 5f;
            }
            else
            {
                moveTarget = (Vector2)transform.position + Random.insideUnitCircle * 3f;
            }
            moveTimer = Random.Range(0.5f, 1.5f);
        }

        movement = (moveTarget - (Vector2)transform.position).normalized * moveSpeed;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = movement;
    }

    Transform FindNearestOni()
    {
        GameObject[] onis = GameObject.FindGameObjectsWithTag("Oni");
        float closestDist = visionRadius;
        Transform closest = null;
        foreach (GameObject oni in onis)
        {
            float dist = Vector2.Distance(transform.position, oni.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = oni.transform;
            }
        }
        return closest;
    }

    Vector2 FindHidePoint(Vector2 oniPosition)
    {
        for (int i = 0; i < 5; i++)
        {
            Vector2 testPoint = (Vector2)transform.position + Random.insideUnitCircle * 3f;
            RaycastHit2D hit = Physics2D.Linecast(oniPosition, testPoint, LayerMask.GetMask("Obstacle"));
            if (hit.collider != null)
            {
                Debug.Log($"[NPCPlayer] Hide point found at {testPoint}");
                return testPoint;
            }
        }
        return Vector2.zero;
    }

    public void Infect()
    {
        if (currentState == PlayerState.Oni) return;
        currentState = PlayerState.Oni;
        moveSpeed *= 1.1f;
        GetComponent<SpriteRenderer>().color = Color.magenta;
        gameObject.tag = "Oni";
        Debug.Log($"[{gameObject.name}] が感染してオニになった！");
    }

    public void UnInfect()
    {
        if (currentState == PlayerState.Human) return;
        currentState = PlayerState.Human;
        moveSpeed /= 1.1f;
        GetComponent<SpriteRenderer>().color = Color.white;
        gameObject.tag = "Human";
        Debug.Log($"[{gameObject.name}] が復活してヒトに戻った！");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController target = other.GetComponent<PlayerController>();
        if (target != null && this.currentState == PlayerState.Oni && target.currentState == PlayerState.Human)
        {
            target.Infect();
            this.UnInfect();
            ScoreManager.Instance.AddScore(10);
            Debug.Log($"[{gameObject.name}] Score add 10, 復活！");
        }

        NPCPlayerController npcTarget = other.GetComponent<NPCPlayerController>();
        if (npcTarget != null && this.currentState == PlayerState.Oni && npcTarget.currentState == PlayerState.Human)
        {
            npcTarget.Infect();
            this.UnInfect();
            ScoreManager.Instance.AddScore(10);
            Debug.Log($"[{gameObject.name}] Score add 10, 復活！");
        }

        if (GameManager.Instance.isDarumaMode && !GameManager.Instance.isStopPhase && currentState == PlayerState.Human)
        {
            if (other.CompareTag("Oni"))
            {
                NPCController npcOni = other.GetComponent<NPCController>();
                if (npcOni != null)
                {
                    ScoreManager.Instance.AddScore(20);
                    GameManager.Instance.LiberateAllOni();
                    Debug.Log($"[{gameObject.name}] NPC鬼タッチ！Score +20、全員解放");
                }
                else
                {
                    PlayerController playerOni = other.GetComponent<PlayerController>();
                    if (playerOni != null)
                    {
                        ScoreManager.Instance.AddScore(5);
                        Debug.Log($"[{gameObject.name}] Playerオニタッチ！Score +5");
                    }
                }
            }
        }
    }
}