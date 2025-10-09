using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartButton : MonoBehaviour
{
    public void RestartGame()
    {
        // 씬 Reload
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        // TimeScale 초기화
        Time.timeScale = 1f;

        // 씬 로드 완료 후 캐릭터 초기화
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 이벤트 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // 플레이어만 리스폰
        if (HurtPlayer.Instance != null)
            HurtPlayer.Instance.RespawnPlayer();

        // 데바는 상태 초기화 후 화면에서 비활성화
        if (HurtDeva.Instance != null)
        {
            HurtDeva.Instance.ResetState(); // 체력, 상태 초기화
            HurtDeva.Instance.gameObject.SetActive(false); // 화면에 안 나타나게
        }

        Debug.Log("씬 Reload 후 아담만 스폰, 데바 비활성화");
    }
}
