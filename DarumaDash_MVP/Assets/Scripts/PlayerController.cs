using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerState currentState = PlayerState.Human;
    public float moveSpeed = 5f;
    public Rigidbody2D rb { get; private set; }
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (GameManager.Instance.isDarumaMode)
        {
            if (currentState == PlayerState.Oni || GameManager.Instance.isStopPhase)
            {
                movement = Vector2.zero;
            }
            else
            {
                movement.x = Input.GetAxisRaw("Horizontal");
                movement.y = Input.GetAxisRaw("Vertical");
            }
        }
        else
        {
            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = movement.normalized * moveSpeed;
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
        if (target != null && this.currentState == PlayerState.Oni && target.currentState == PlayerState.Human && !other.GetComponent<NPCController>())
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