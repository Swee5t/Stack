using UnityEngine;

public class Main : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnGameStart()
    {
        Application.targetFrameRate = Application.platform == RuntimePlatform.IPhonePlayer ? 60 : -1;
    }
}