using UnityEngine;
using System.Collections;

public class DarumaVoiceController : MonoBehaviour
{
    public AudioClip[] darumaClips; // 各キャラ用だるま音声（6つ）
    public string characterColor = "red"; // キャラ識別
    public GameObject stopText;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            PlayDarumaVoice();
        }
    }

    public void PlayDarumaVoice()
    {
        int index = GetVoiceIndexByColor(characterColor);
        if (index >= 0 && index < darumaClips.Length)
        {
            AudioManager.Instance.PlayVoice(darumaClips[index]);
            float delay = darumaClips[index].length;
            StartCoroutine(TriggerStop(delay));
        }
    }
    IEnumerator TriggerStop(float delay)
    {
        yield return new WaitForSeconds(delay);
        stopText.SetActive(true);
        yield return new WaitForSeconds(1.0f);
        stopText.SetActive(false);
        Debug.Log("[Daruma] 判定トリガー！");
    }

    int GetVoiceIndexByColor(string color)
    {
        switch (color)
        {
            case "red": return 0;
            case "blue": return 1;
            case "green": return 2;
            case "yellow": return 3;
            case "pink": return 4;
            case "npc": return 5;
            default: return -1;
        }
    }
}

