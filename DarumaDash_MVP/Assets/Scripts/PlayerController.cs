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

    public void Infect()//感染処理(スコア加算は削除)
    {
        if (currentState == PlayerState.Oni) return; // すでにオニならスキップ

        currentState = PlayerState.Oni;
        moveSpeed *= 1.1f;
    
        // オニ用のスプライトに変更したい場合
        GetComponent<SpriteRenderer>().color = Color.magenta;

        Debug.Log($"[{gameObject.name}] が感染してオニになった！");

        //ScoreManager.Instance.AddScore(10); // 感染で+10点
    }
    
    void OnTriggerEnter2D(Collider2D other)//当たり判定処理
    {
        PlayerController target = other.GetComponent<PlayerController>();

        if (target != null)
        {
            Debug.Log($"OnTriggerEnter2D called. other: {other.gameObject.name}, this: {this.gameObject.name}"); // 追加

            // 触れた側が鬼、触れられた側が人間の場合にのみ処理を行う
            if (this.currentState == PlayerState.Oni && target.currentState == PlayerState.Human)
            {
                target.Infect();
                ScoreManager.Instance.AddScore(10); // 感染させたプレイヤーにスコア加算
                Debug.Log("Score add 10");
            }
        }
    }

}
