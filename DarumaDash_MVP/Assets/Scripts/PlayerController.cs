using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerState currentState = PlayerState.Human;
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 movement;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = movement.normalized * moveSpeed;
    }

    public void Infect()//感染処理
    {
        if (currentState == PlayerState.Oni) return; // すでにオニならスキップ

        currentState = PlayerState.Oni;
        moveSpeed *= 1.1f;
    
        // オニ用のスプライトに変更したい場合
        GetComponent<SpriteRenderer>().color = Color.magenta;

        Debug.Log($"[{gameObject.name}] が感染してオニになった！");
    }
    
    void OnTriggerEnter2D(Collider2D other)//当たり判定処理
    {
        PlayerController target = other.GetComponent<PlayerController>();

        if (target != null && this.currentState == PlayerState.Oni && target.currentState == PlayerState.Human)
        {
            target.Infect();
        }
    }

}
