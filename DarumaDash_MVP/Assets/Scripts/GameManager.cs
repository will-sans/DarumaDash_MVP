using UnityEngine;
using UnityEngine.SceneManagement;//*
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool isDarumaMode = false;
    public bool isStopPhase = false;
    public AudioClip darumaFullClip;
    public GameObject stopText;

    public GameObject gameClearPanel;//*
    public GameObject gameOverPanel;//*
    public GameObject reviveText; // 新UI：復活チャンス通知

    private List<PlayerController> players;
    private List<NPCPlayerController> npcPlayers;
    private float darumaCooldown = 0f;
    private bool isNPCInitiatedDaruma = false;
    private bool isGameEnded = false;//*

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        players = new List<PlayerController>(Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None));
        npcPlayers = new List<NPCPlayerController>(Object.FindObjectsByType<NPCPlayerController>(FindObjectsSortMode.None));
        gameClearPanel.SetActive(false);//*
        gameOverPanel.SetActive(false);//*
        reviveText.SetActive(false);//*
    }

    void Update()
    {
        if (isGameEnded) return;//*

        if (!isDarumaMode)
        {
            darumaCooldown -= Time.deltaTime;
        }
        // 全員感染チェック//*
        bool allInfected = true;
        foreach (PlayerController player in players)
        {
            if (player.currentState == PlayerState.Human)
            {
                allInfected = false;
                break;
            }
        }
        foreach (NPCPlayerController npc in npcPlayers)
        {
            if (npc.currentState == PlayerState.Human)
            {
                allInfected = false;
                break;
            }
        }
        if (allInfected)
        {
            GameOver();
        }

    }

    public void EnterDarumaMode(bool isNPCInitiated = false)
    {
        if (darumaCooldown > 0 || isGameEnded) return;//*
        isDarumaMode = true;
        isNPCInitiatedDaruma = isNPCInitiated;
        AudioManager.Instance.PlayDarumaBGM();
        Debug.Log("[GameManager] だるまモード突入");
        StartCoroutine(PlayDarumaVoice());
    }

    public void ExitDarumaMode()
    {
        isDarumaMode = false;
        isStopPhase = false;
        stopText.SetActive(false);
        AudioManager.Instance.PlayBGM(AudioManager.Instance.grasslandClip);
        darumaCooldown = 10f;
        Debug.Log("[GameManager] 通常モードに戻る");
    }

    System.Collections.IEnumerator PlayDarumaVoice()
    {
        float[] speeds = { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.2f, 1.5f };
        AudioManager.Instance.PlayVoice(darumaFullClip, speeds[0]);
        yield return new WaitForSeconds(darumaFullClip.length / speeds[0]);

        isStopPhase = true;
        stopText.SetActive(true);
        Debug.Log("[GameManager] 動くな！");

        foreach (PlayerController player in players)
        {
            if (player.currentState == PlayerState.Human && player.rb.linearVelocity.magnitude > 0.1f)
            {
                player.Infect();
                if (!isNPCInitiatedDaruma)
                {
                    ScoreManager.Instance.AddScore(10);
                    Debug.Log($"01[GameManager] {player.gameObject.name} が動いてアウト！Score +10");
                }
                else
                {
                    Debug.Log($"[GameManager] {player.gameObject.name} が動いてアウト！");
                }
                if (player.gameObject.name == "Player_Red")//*
                {
                    reviveText.SetActive(true); // 復活チャンス通知
                }

            }
        }
        foreach (NPCPlayerController npc in npcPlayers)
        {
            if (npc.currentState == PlayerState.Human && npc.rb.linearVelocity.magnitude > 0.1f)
            {
                npc.Infect();
                if (!isNPCInitiatedDaruma)
                {
                    ScoreManager.Instance.AddScore(10);
                    Debug.Log($"02[GameManager] {npc.gameObject.name} が動いてアウト！Score +10");
                }
                else
                {
                    Debug.Log($"[GameManager] {npc.gameObject.name} が動いてアウト！");
                }
            }
        }

        yield return new WaitForSeconds(1.0f);
        stopText.SetActive(false);//*
        reviveText.SetActive(false);//*
        ExitDarumaMode();
    }

    public void LiberateAllOni()
    {
        foreach (PlayerController player in players)
        {
            if (player.currentState == PlayerState.Oni)
            {
                player.UnInfect();
            }
        }
        foreach (NPCPlayerController npc in npcPlayers)
        {
            if (npc.currentState == PlayerState.Oni)
            {
                npc.UnInfect();
            }
        }
        reviveText.SetActive(false); // 解放で復活チャンス通知オフ//*
    }

    public void AddSurvivalBonus()
    {
        foreach (PlayerController player in players)
        {
            if (player.currentState == PlayerState.Human)
            {
                ScoreManager.Instance.AddScore(50);
                Debug.Log($"03[GameManager] {player.gameObject.name} 生存ボーナス！Score +50");
            }
        }
        foreach (NPCPlayerController npc in npcPlayers)
        {
            if (npc.currentState == PlayerState.Human)
            {
                //NPCの生き残りはスコアに関係なしに修正
                //ScoreManager.Instance.AddScore(50);
                //Debug.Log($"04[GameManager] {npc.gameObject.name} 生存ボーナス！Score +50");
            }
        }
    }
    public void GameClear()//*
    {
        isGameEnded = true;
        gameClearPanel.SetActive(true);
        Debug.Log("[GameManager] ゲームクリア！");
        StartCoroutine(ReturnToTitle());
    }

    public void GameOver()//*
    {
        isGameEnded = true;
        gameOverPanel.SetActive(true);
        Debug.Log("[GameManager] ゲームオーバー！");
        StartCoroutine(ReturnToTitle());
    }

    System.Collections.IEnumerator ReturnToTitle()//*
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("Title");
    }
}