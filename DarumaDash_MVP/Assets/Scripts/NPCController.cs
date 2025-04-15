using UnityEngine;
public enum NPCState//状態をEnumで定義
{
    Idle,
    Chase,
    Rush
}


public class NPCController : MonoBehaviour
{
    //NPCController.csに状態管理を追加
    public NPCState currentState = NPCState.Idle;
    private float idleTimer = 0f;
    private Vector2 idleTarget;
    //視野距離 visionRadius を変数化（調整可能）
    public float visionRadius = 6f; // Inspectorから調整可

    public float speed = 3.5f;
    //public Transform target;
    public Transform[] potentialTargets;
    public PlayerController selfPlayer;
    
    void Start()
    {
        selfPlayer = GetComponent<PlayerController>();
        selfPlayer.currentState = PlayerState.Oni; // NPCは常にオニ！
    }

    //Updateで状態遷移と動作切替
    void Update()
    {
        if (selfPlayer.currentState != PlayerState.Oni) return;

        Transform closest = FindClosestHuman();
        float distance = closest != null ? Vector2.Distance(transform.position, closest.position) : Mathf.Infinity;

        // 状態遷移
        if (closest != null)
        {
            if (distance < 2f)
            {
                currentState = NPCState.Rush;
            }
            else
            {
                currentState = NPCState.Chase;
            }
        }
        else
        {
            currentState = NPCState.Idle;
        }

        // 行動実行
        switch (currentState)
        {
            case NPCState.Idle:
                DoIdle();
                break;
            case NPCState.Chase:
                DoChase(closest, speed);
                break;
            case NPCState.Rush:
                DoChase(closest, speed * 1.5f);
                break;
        }
    }
    
    //遮蔽物チェック付きの FindClosestHuman() に書き換え！
    Transform FindClosestHuman()
    {
        Transform closest = null;
        float closestDist = Mathf.Infinity;

        foreach (Transform t in potentialTargets)
        {
            PlayerController p = t.GetComponent<PlayerController>();
            if (p != null && p.currentState == PlayerState.Human)
            {
                float dist = Vector2.Distance(transform.position, t.position);

                if (dist < closestDist && dist <= visionRadius)
                {
                    // 遮蔽チェック（Raycast）
                    RaycastHit2D hit = Physics2D.Linecast(transform.position, t.position, LayerMask.GetMask("Obstacle"));

                    if (hit.collider == null)
                    {
                        closestDist = dist;
                        closest = t;
                    }
                }
            }
        }

        return closest;
    }


    void DoChase(Transform target, float chaseSpeed)
    {
        if (target == null) return;
        Vector2 dir = (target.position - transform.position).normalized;
        transform.position += (Vector3)(dir * chaseSpeed * Time.deltaTime);
    }

    void DoIdle()
    {
        Debug.Log("[NPC] DoIdle");
        idleTimer -= Time.deltaTime;

        if (idleTimer <= 0f)
        {
            idleTarget = (Vector2)transform.position + Random.insideUnitCircle * 2f;
            idleTimer = Random.Range(1.5f, 3f);
        }

        Vector2 dir = (idleTarget - (Vector2)transform.position).normalized;
        transform.position += (Vector3)(dir * (speed * 0.5f) * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null && player.currentState == PlayerState.Human)
        {
            player.Infect();
        }
    }
}
