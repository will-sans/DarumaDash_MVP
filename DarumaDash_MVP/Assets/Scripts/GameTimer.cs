using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    public float timeLeft = 60f; // 制限時間（秒）
    public TextMeshProUGUI timerText;

    private bool isCounting = true;

    void Update()
    {
        if (!isCounting) return;

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            isCounting = false;
            timerText.text = "00:00";
            Debug.Log("時間切れ！");

            // ここに生存ボーナスを加算する処理を追加！
            ScoreManager.Instance.AddScore(50); // 生存ボーナス
            Debug.Log($"05[GameTimer]  生存ボーナス！Score +50");
            // TODO: ゲーム終了処理
        }
        else
        {
            int minutes = Mathf.FloorToInt(timeLeft / 60f);
            int seconds = Mathf.FloorToInt(timeLeft % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }
}
