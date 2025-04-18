using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool isDarumaMode = false;
    public bool isStopPhase = false;
    public AudioClip darumaFullClip;
    public GameObject stopText;
    private List<PlayerController> players;
    private List<NPCPlayerController> npcPlayers;
    private float darumaCooldown = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        players = new List<PlayerController>(Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None));
        npcPlayers = new List<NPCPlayerController>(Object.FindObjectsByType<NPCPlayerController>(FindObjectsSortMode.None));
    }

    void Update()
    {
        if (!isDarumaMode)
        {
            darumaCooldown -= Time.deltaTime;
        }
    }

    public void EnterDarumaMode()
    {
        if (darumaCooldown > 0) return;
        isDarumaMode = true;
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
                ScoreManager.Instance.AddScore(10); // アウトで+10
                Debug.Log($"[GameManager] {player.gameObject.name} が動いてアウト！Score +10");
            }
        }
        foreach (NPCPlayerController npc in npcPlayers)
        {
            if (npc.currentState == PlayerState.Human && npc.rb.linearVelocity.magnitude > 0.1f)
            {
                npc.Infect();
                ScoreManager.Instance.AddScore(10); // アウトで+10
                Debug.Log($"[GameManager] {npc.gameObject.name} が動いてアウト！Score +10");
            }
        }

        yield return new WaitForSeconds(1.0f);
        ExitDarumaMode();
    }

    public void LiberateAllOni()
    {
        foreach (PlayerController player in players)
        {
            if (player.currentState == PlayerState.Oni)
            {
                player.currentState = PlayerState.Human;
                player.moveSpeed /= 1.1f;
                player.GetComponent<SpriteRenderer>().color = Color.white;
                player.gameObject.tag = "Human";
                Debug.Log($"[GameManager] {player.gameObject.name} が解放された！");
            }
        }
        foreach (NPCPlayerController npc in npcPlayers)
        {
            if (npc.currentState == PlayerState.Oni)
            {
                npc.currentState = PlayerState.Human;
                npc.moveSpeed /= 1.1f;
                npc.GetComponent<SpriteRenderer>().color = Color.white;
                npc.gameObject.tag = "Human";
                Debug.Log($"[GameManager] {npc.gameObject.name} が解放された！");
            }
        }
    }

    public void AddSurvivalBonus()
    {
        foreach (PlayerController player in players)
        {
            if (player.currentState == PlayerState.Human)
            {
                ScoreManager.Instance.AddScore(50);
                Debug.Log($"[GameManager] {player.gameObject.name} 生存ボーナス！Score +50");
            }
        }
        foreach (NPCPlayerController npc in npcPlayers)
        {
            if (npc.currentState == PlayerState.Human)
            {
                ScoreManager.Instance.AddScore(50);
                Debug.Log($"[GameManager] {npc.gameObject.name} 生存ボーナス！Score +50");
            }
        }
    }
}