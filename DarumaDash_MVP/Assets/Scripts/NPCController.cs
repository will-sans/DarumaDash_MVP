using UnityEngine;
using System.Collections; // 非ジェネリックIEnumerator用
using System.Collections.Generic; // List型を使用するために必要

public enum NPCState//Define the state as Enum
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
    public PlayerState playerState = PlayerState.Oni; // InspectorでOni設定

    private float idleTimer = 0f;
    private Vector2 idleTarget;
    private Vector2 lastSeenPosition;
    private bool hasLastSeenPosition = false;
    public Transform[] patrolPoints; // Inspectorから設定する巡回地点
    private int currentPatrolIndex = 0;
    private float patrolSpeedFactor = 0.5f;
    public float patrolReachDistance = 0.3f; // 巡回地点への到達判定距離
    private Vector2 initialPosition;
    public float idleMoveRadius = 3f;
    public float idleSpeedMin = 0.3f;
    public float idleSpeedMax = 0.7f;

    public AudioClip npcDarumaClip; // NPC鬼用の「だるま、セイ！」音声
    public GameObject stopText;

    void Start()
    {
        initialPosition = transform.position; // 初期位置を保存
    }
    
    void Update()
    {
        if (playerState == PlayerState.Oni && GameManager.Instance.isDarumaMode)
        {
            // ランダムや条件で音声再生（例: 5秒ごと）
            if (Random.value < 0.01f) // 仮のトリガー
            {
                AudioManager.Instance.PlayVoice(npcDarumaClip);
                StartCoroutine(TriggerStop(npcDarumaClip.length));
            }
        }
        if (playerState != PlayerState.Oni) return;

        Transform closest = FindClosestHuman();
        float distance = closest != null ? Vector2.Distance(transform.position, closest.position) : Mathf.Infinity;

        //State transition
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

        //Action execution
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
                DoChase(lastSeenPosition, speed * 2f);//見失ったらダッシュ
                if (Vector2.Distance(transform.position, lastSeenPosition) < 0.3f)
                {
                    hasLastSeenPosition = false;
                    currentState = NPCState.Idle;
                }
                break;
        }
    }
    System.Collections.IEnumerator TriggerStop(float delay)
    {
        yield return new WaitForSeconds(delay);
        stopText.SetActive(true);
        yield return new WaitForSeconds(1.0f);
        stopText.SetActive(false);
        Debug.Log("[NPC_Daruma] 判定トリガー！");
    }
    Transform FindClosestHuman()
    {
        List<Transform> visibleHumans = new List<Transform>(); // 視界内のHumanを格納するリスト
        float closestDist = Mathf.Infinity;
        Transform closest = null;

        foreach (Transform target in potentialTargets)
        {
            PlayerController player = target.GetComponent<PlayerController>();
            if (player != null && player.currentState == PlayerState.Human)
            {
                float dist = Vector2.Distance(transform.position, target.position);
                if (dist <= visionRadius)
                {
                    // 障害物チェック
                    RaycastHit2D hit = Physics2D.Linecast(transform.position, target.position, LayerMask.GetMask("Obstacle"));
                    if (hit.collider == null)
                    {
                        visibleHumans.Add(target); // 障害物がなければリストに追加
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            closest = target;
                        }
                    }
                }
            }
        }

        // visibleHumansリストが空でなければ、最も近いターゲットを返す
        // 視界内に障害物のないHumanがいなければnullを返す
        return visibleHumans.Count > 0 ? closest : null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, visionRadius);
    }

    void DoChase(Vector3 targetPosition, float chaseSpeed) // 引数の型をVector3に変更
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
            // 巡回ポイントが設定されていない場合は、既存のランダム移動
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

        // 障害物回避処理
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 1f, LayerMask.GetMask("Obstacle"));

        if (hit.collider == null)
        {
            // 障害物がなければそのまま進む
            transform.position += (Vector3)(direction * (speed * patrolSpeedFactor) * Time.deltaTime);
        }
        else
        {
            // 障害物があれば少しだけ方向転換
            Vector2 rightDir = new Vector2(-direction.y, direction.x);
            transform.position += (Vector3)(rightDir * speed * patrolSpeedFactor * 0.5f * Time.deltaTime);
        }

        if (Vector2.Distance(transform.position, targetPoint.position) < patrolReachDistance)
        {
            // 次の巡回ポイントをランダムに選択 (ただし直前のポイントと同じにならないように)
            int previousIndex = currentPatrolIndex;
            while (currentPatrolIndex == previousIndex)
            {
                currentPatrolIndex = Random.Range(0, patrolPoints.Length);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if(player != null && playerState == PlayerState.Oni && player.currentState == PlayerState.Human)
        {
                player.Infect();
                Debug.Log($"[NPC_Oni] {player.gameObject.name} を感染させた！");
        }
    }
}