using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBallSpawner : MonoBehaviour
{
    public Transform spawnPoint1;  // 스폰 위치
    public Transform spawnPoint2;  // 스폰 위치
    public float cool1;    // 쿨타임 최소 시간
    public float cool2;    // 쿨타임 최대 시간


    [Header("FireBall Settings")]
    public GameObject fireBallPrefab;  // FireBall 프리팹
    public GameObject warning1;
    public GameObject warning2;

    private Coroutine _coolTimeRoutine;
    private Coroutine fierBallSpawn;

    private void OnEnable()
    {
        if (_coolTimeRoutine == null)
            _coolTimeRoutine = StartCoroutine(coolTime());
    }

    private void OnDisable()
    {
        if (_coolTimeRoutine != null)
        {
            StopCoroutine(_coolTimeRoutine);
            _coolTimeRoutine = null;
        }
    }

    IEnumerator coolTime()
    {
        while (true)
        {
            float t = 0f;
            t = Random.Range(cool1, cool2);
            while (0f < t)
            {
                t -= Time.deltaTime;
                yield return null;
            }

            float rand = Random.Range(0f, 1f);
            if (rand < 0.5f)
            {
                TrySpawnFireBall(spawnPoint1, 1);
            }
            else
            {
                TrySpawnFireBall(spawnPoint2, 2);
            }
            
        }
    }
    void TrySpawnFireBall(Transform spawnPoint, int whatSpawnPoint)
    {
        if (StageManager.currentStage <= 0) return;

        if (spawnPoint == null)
        {
            Debug.LogWarning("스폰 포인트가 비어 있습니다!");
            return;
        }
        fierBallSpawn = StartCoroutine(SpawnFierBall(spawnPoint, whatSpawnPoint));
    }

    IEnumerator SpawnFierBall(Transform spawnPoint, int whatSpawnPoint)
    {
        if (whatSpawnPoint == 1)
        {
            Debug.Log("fhrm1");
            warning1.SetActive(true);
            yield return new WaitForSeconds(1f);
            warning1.SetActive(false);

        }
        else
        {
            Debug.Log("fhrm2");
            warning2.SetActive(true);
            yield return new WaitForSeconds(1f);
            warning2.SetActive(false);
            
        }
        
        GameObject fb;
        fb = Instantiate(fireBallPrefab, spawnPoint.position, Quaternion.identity);
        FireBall fbScript = fb.GetComponent<FireBall>();
        if (whatSpawnPoint == 1)
        {
            fbScript.isRight = true;
        }
        else
        {
            fbScript.isRight = false;
        }

        Debug.Log($" FireBall Spawned at {spawnPoint.name}");
    }
}
