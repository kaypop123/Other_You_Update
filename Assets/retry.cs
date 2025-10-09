using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class retry : MonoBehaviour
{
    public void Retry()
    {
        Time.timeScale = 1f;

        // HurtDeva와 관련 싱글톤 오브젝트를 삭제하지 않고 비활성화
        if (HurtDeva.Instance != null)
            HurtDeva.Instance.gameObject.SetActive(false);

        if (DevaStats.Instance != null)
            DevaStats.Instance.gameObject.SetActive(false);

        // 씬 다시 로드
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
