using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    public int score = 0;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI finalScoreText;//*

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    void Start()//*
    {
        UpdateScoreUI();
        if (finalScoreText != null)
        {
            finalScoreText.text = "";
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
        //Debug.Log($"[ScoreManager] AddScore: {amount}, Total: {score}, Caller: {new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name}");
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        scoreText.text = $"SCORE: {score}";
        if (finalScoreText != null)//*
        {
            finalScoreText.text = $"FINAL SCORE: {score}";
        }
    }
}