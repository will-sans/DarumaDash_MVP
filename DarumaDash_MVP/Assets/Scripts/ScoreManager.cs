using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    public int score = 0;
    public TextMeshProUGUI scoreText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddScore(int amount)
    {
        score += amount;
        Debug.Log($"[ScoreManager] AddScore: {amount}, Total: {score}, Caller: {new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name}");
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        scoreText.text = $"SCORE: {score}";
    }
}