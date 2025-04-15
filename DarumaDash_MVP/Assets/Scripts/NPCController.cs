using UnityEngine;
public enum NPCState//��Ԃ�Enum�Œ�`
{
    Idle,
    Chase,
    Rush
}


public class NPCController : MonoBehaviour
{
    //NPCController.cs�ɏ�ԊǗ���ǉ�
    public NPCState currentState = NPCState.Idle;
    private float idleTimer = 0f;
    private Vector2 idleTarget;
    //���싗�� visionRadius ��ϐ����i�����\�j
    public float visionRadius = 6f; // Inspector���璲����

    public float speed = 3.5f;
    //public Transform target;
    public Transform[] potentialTargets;
    public PlayerController selfPlayer;
    
    void Start()
    {
        selfPlayer = GetComponent<PlayerController>();
        selfPlayer.currentState = PlayerState.Oni; // NPC�͏�ɃI�j�I
    }

    //Update�ŏ�ԑJ�ڂƓ���ؑ�
    void Update()
    {
        if (selfPlayer.currentState != PlayerState.Oni) return;

        Transform closest = FindClosestHuman();
        float distance = closest != null ? Vector2.Distance(transform.position, closest.position) : Mathf.Infinity;

        // ��ԑJ��
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

        // �s�����s
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
    
    //�Օ����`�F�b�N�t���� FindClosestHuman() �ɏ��������I
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
                    // �Օ��`�F�b�N�iRaycast�j
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
    void OnDrawGizmosSelected()
{
    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere(transform.position, visionRadius);
}

    void DoChase(Transform target, float chaseSpeed)
    {
        if (target == null) return;
        Vector2 dir = (target.position - transform.position).normalized;
        transform.position += (Vector3)(dir * chaseSpeed * Time.deltaTime);
    }

    void DoIdle()
    {
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
