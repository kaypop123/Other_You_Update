using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverRetryHandler : MonoBehaviour
{
    public void RetryGame()
    {
        // 현재 씬 이름을 가져와 다시 로드
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);

        Debug.Log("씬을 다시 로드하여 게임 재시작!");
    }
}
