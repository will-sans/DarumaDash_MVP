// GameManager.cs
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool isDarumaMode = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void EnterDarumaMode()
    {
        isDarumaMode = true;
        Debug.Log("[GameManager] だるまモード突入");
    }

    public void ExitDarumaMode()
    {
        isDarumaMode = false;
        Debug.Log("[GameManager] 通常モードに戻る");
    }
}
