using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerState currentState = PlayerState.Human;
    public float moveSpeed = 5f;
    public Rigidbody2D rb { get; private set; }
    private Vector2 movement;
    private bool isContacting = false;//接触を判定するフラグ


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
        if (gameObject.name == "Player_Red")
        {
            GameManager.Instance.reviveText.SetActive(true); // 復活チャンス通知
        }

    }

    public void UnInfect()
    {
        if (currentState == PlayerState.Human) return;
        currentState = PlayerState.Human;
        moveSpeed /= 1.1f;
        GetComponent<SpriteRenderer>().color = Color.white;
        gameObject.tag = "Human";
        Debug.Log($"[{gameObject.name}] が復活してヒトに戻った！");
        if (gameObject.name == "Player_Red")
        {
            GameManager.Instance.reviveText.SetActive(false); // 通知オフ
        }

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isContacting)
        {
            PlayerController target = other.GetComponent<PlayerController>();
            if (target != null && this.currentState == PlayerState.Oni && target.currentState == PlayerState.Human && !other.GetComponent<NPCController>())
            {
                target.Infect();
                this.UnInfect();
                ScoreManager.Instance.AddScore(10);
                Debug.Log($"10[{gameObject.name}] Score add 10, 復活！");
                isContacting = true;
            }

            NPCPlayerController npcTarget = other.GetComponent<NPCPlayerController>();
            if (npcTarget != null && this.currentState == PlayerState.Oni && npcTarget.currentState == PlayerState.Human)
            {
                npcTarget.Infect();
                this.UnInfect();
                ScoreManager.Instance.AddScore(10);
                Debug.Log($"11[{gameObject.name}] Score add 10, 復活！");
                isContacting = true;
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
                        Debug.Log($"12[{gameObject.name}] NPC鬼タッチ！Score +20、全員解放");
                        isContacting = true;
                    }
                    else
                    {
                        PlayerController playerOni = other.GetComponent<PlayerController>();
                        if (playerOni != null)
                        {
                            ScoreManager.Instance.AddScore(5);
                            Debug.Log($"13[{gameObject.name}] Playerオニタッチ！Score +5");
                            isContacting = true;
                        }
                    }
                }
            }
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        NPCPlayerController npcPlayer = other.GetComponent<NPCPlayerController>();
        if (player != null || npcPlayer != null)
        {
            isContacting = false;
        }
    }
}