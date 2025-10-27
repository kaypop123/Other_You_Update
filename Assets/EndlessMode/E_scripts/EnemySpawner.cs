using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject[] enemyPrefab;       // 소환할 적 프리팹
    public int maxSpawnCount = 5;        // 최대 스폰 수 (인스펙터에서 설정)
    public float spawnInterval = 2f;     // 스폰 간격

    [Header("Portal Settings")]
    public GameObject portalPrefab;      // 모든 적 처치 후 생성될 포탈

    private int currentSpawned = 0;      // 현재 스폰된 적 수
    private int deadCount = 0;           // 사망한 적 수
    public bool spawning = false;       // 스폰 진행 여부

    public EnemySpawner otherES;       //다른 스포너

    private Coroutine spawnRoutine;

    private Transform thisPos;

    private void Start()
    {
        thisPos = GetComponent<Transform>();
    }


    // 외부에서 트리거가 불리면 스폰 시작
    public void StartSpawning()
    {
        if (spawning) return;
        spawning = true;
        spawnRoutine = StartCoroutine(SpawnEnemies());
    }

    IEnumerator SpawnEnemies()
    {
        while (currentSpawned < maxSpawnCount)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null) return;

        GameObject enemy;
        if (StageManager.currentStage <= 5)
        {
            enemy = Instantiate(enemyPrefab[0], thisPos.position, Quaternion.identity);
        }
        else if(StageManager.currentStage <= 10)
        {
            enemy = Instantiate(enemyPrefab[1], thisPos.position, Quaternion.identity);
        }
        else if (StageManager.currentStage <= 15)
        {
            enemy = Instantiate(enemyPrefab[2], thisPos.position, Quaternion.identity);
        }
        else if (StageManager.currentStage <= 20)
        {
            enemy = Instantiate(enemyPrefab[3], thisPos.position, Quaternion.identity);
        }
        else
        {
            enemy = Instantiate(enemyPrefab[4], thisPos.position, Quaternion.identity);
        }

        currentSpawned++;

        // enemyTest에서 이 스포너로 접근할 수 있게 등록
        enemyTest enemyScript = enemy.GetComponent<enemyTest>();
        BossHurt bossHurt = enemy.GetComponent<BossHurt>();
        if (bossHurt != null)
        {
            bossHurt.mySpawner = this;
        }
        if (enemyScript != null)
        {
            enemyScript.mySpawner = this;
        }
    }

    // enemyTest에서 호출됨 (적 사망 시)
    public void OnEnemyDied()
    {
        deadCount++;

        if (deadCount >= maxSpawnCount)
        {
            Debug.Log($"모든 적 처치 완료! ({deadCount}/{maxSpawnCount})");
            StopAllCoroutines();
            spawning = false;

        }
        if (otherES != null)
        {
            if (!spawning && !otherES.spawning)
            {
                SpawnPortal();
            }
        }
        else if (!spawning)
        {
            SpawnPortal();
        }

    }

    void SpawnPortal()
    {
        if (portalPrefab != null)
        {
            portalPrefab.SetActive(true);
            Debug.Log("포탈 생성 완료!");
        }
    }

    // [추가] 스테이지 초기화용 함수
public void ResetSpawner()
{
    // 현재 스테이지의 적들을 전부 제거
    foreach (Transform child in transform)
    {
        if (child.CompareTag("Enemy"))
            Destroy(child.gameObject);
    }

    // 스폰 관련 변수 초기화
    currentSpawned = 0;
    deadCount = 0;
    spawning = false;

    // 포탈 비활성화 (다시 클리어해야 열리도록)
    if (portalPrefab != null)
        portalPrefab.SetActive(false);

    Debug.Log($"[{gameObject.name}] 스테이지 초기화 완료");
}

}



/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform SpawnPoint;
    public float spawnInterval = 2f;
    private bool canSpawn = true;
    private Coroutine spawnCoroutine;

    void Start()
    {
        spawnCoroutine = StartCoroutine(SpawnEnemies());
    }

    IEnumerator SpawnEnemies()
    {
        while (canSpawn)
        {
            // 🔹 플레이어 사망 체크
            if (HurtPlayer.Instance != null && HurtPlayer.Instance.IsDead())
            {
                Debug.Log("플레이어 사망! 스폰 중단");
                yield break; // 코루틴 종료
            }

            SpawnEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab != null)
        {
            Instantiate(enemyPrefab, SpawnPoint.position, Quaternion.identity);
           
        }
    }

    public void StopSpawning()
    {
        canSpawn = false;

        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);

        Debug.Log($"[{gameObject.name}] Enemy Spawning Stopped!");
    }

    public void ResumeSpawning()
    {
        if (!canSpawn)
        {
            canSpawn = true;
            spawnCoroutine = StartCoroutine(SpawnEnemies());
        }
    }
}




*/