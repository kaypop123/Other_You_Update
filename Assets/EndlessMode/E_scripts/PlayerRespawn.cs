using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerRespawn : MonoBehaviour
{
    public Transform spawnPoint;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindSpawnPoint(); // 씬 로드 후 스폰포인트 다시 찾기
        MoveToSpawnPoint();
    }

    public void FindSpawnPoint()
    {
        GameObject spawnObject = GameObject.FindWithTag("SpawnPoint");
        if (spawnObject != null)
            spawnPoint = spawnObject.transform;
        else
            Debug.LogWarning("SpawnPoint를 찾을 수 없습니다.");
    }

    public void MoveToSpawnPoint()
    {
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
            Debug.Log("플레이어가 스폰 위치로 이동했습니다.");
        }
    }
}
