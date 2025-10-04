using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBallSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnPhase
    {
        public int triggerScore;            // 몇 점일 때 스폰할지
        public Transform spawnPoint;        // 스폰 위치
    }

    [Header("FireBall Settings")]
    public GameObject fireBallPrefab;       // FireBall 프리팹
    public int maxFireBalls = 2;            // 동시에 존재 가능한 FireBall 수

    [Header("Spawn Phases (Customize Freely)")]
    public List<SpawnPhase> phases = new List<SpawnPhase>();

    private HashSet<int> spawnedPhases = new HashSet<int>(); // 중복 스폰 방지용

    void Update()
    {
        if (ScoreManager.instance == null) return;

        int currentScore = ScoreManager.instance.score;

        // 각 페이즈를 순회하며 트리거 점수 확인
        foreach (var phase in phases)
        {
            if (!spawnedPhases.Contains(phase.triggerScore) && currentScore >= phase.triggerScore)
            {
                TrySpawnFireBall(phase.spawnPoint);
                spawnedPhases.Add(phase.triggerScore);
            }
        }
    }

    void TrySpawnFireBall(Transform spawnPoint)
    {
        int currentFireBallCount = FindObjectsOfType<FireBall>().Length;
        if (currentFireBallCount >= maxFireBalls) return;

        if (spawnPoint == null)
        {
            Debug.LogWarning("스폰 포인트가 비어 있습니다!");
            return;
        }

        Instantiate(fireBallPrefab, spawnPoint.position, Quaternion.identity);
        Debug.Log($" FireBall Spawned at {spawnPoint.name}");
    }
}
