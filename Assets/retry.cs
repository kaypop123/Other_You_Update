using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class retry : MonoBehaviour
{
    public void Retry()
    {
        Time.timeScale = 1f;

        // 싱글톤 오브젝트 완전히 삭제
        if (HurtDeva.Instance != null)
            Destroy(HurtDeva.Instance.gameObject);

        if (DevaStats.Instance != null)
            Destroy(DevaStats.Instance.gameObject);

        // 씬 다시 로드
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }



}

