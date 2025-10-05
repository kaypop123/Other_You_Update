using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap), typeof(TilemapRenderer), typeof(TilemapCollider2D))]
public class FadingPlatformTilemap : MonoBehaviour
{
    [Header("블럭 타이밍 조절")]
    public float disappearDelay = 1.5f;   // 플레이어 접촉 후 사라지기까지 대기
    public float warningTime = 0.5f;      // 사라지기 전 깜빡임 시간 (0으로 하면 없음)
    public float blinkInterval = 0.1f;    // 깜빡임 빈도
    public float fadeDuration = 0.5f;     // 페이드 아웃/인 시간
    public float reappearDelay = 2f;      // 완전히 사라진 후 다시 나타나기까지 대기

    Tilemap tilemap;
    TilemapRenderer tRenderer;
    TilemapCollider2D tCollider;
    Color originalColor;
    bool isProcessing = false;

    void Awake()
    {
        tilemap = GetComponent<Tilemap>();
        tRenderer = GetComponent<TilemapRenderer>();
        tCollider = GetComponent<TilemapCollider2D>();
        originalColor = tilemap != null ? tilemap.color : Color.white;
    }

    // 플레이어가 올라갔을 때 감지하려면 Collider가 트리거가 아닌 상태여야 하고
    // 적어도 하나(Rigidbody2D)가 있어야 함. (Tilemap에 Static Rigidbody2D)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isProcessing) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        // 위에서 밟았을 때만 작동
        bool hitFromAbove = false;
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // contact.normal이 아래쪽(-Y)으로 향하면 Player가 위에서 내려왔다는 뜻임
            if (Vector2.Dot(contact.normal, Vector2.down) > 0.7f)
            {
                hitFromAbove = true;
                break;
            }
        }

        if (hitFromAbove)
        {
            StartCoroutine(ProcessFadeCycle());
        }
    }

    IEnumerator ProcessFadeCycle()
    {
        isProcessing = true;

        // 1) 기다리기 (사라지기 전 전체 대기 - warningTime을 뺀 나머지)
        float waitBeforeWarning = Mathf.Max(0f, disappearDelay - warningTime);
        yield return new WaitForSeconds(waitBeforeWarning);

        // 2) 경고(깜빡임) - warningTime 동안 렌더러 ON/OFF
        if (warningTime > 0f)
        {
            float elapsed = 0f;
            bool visible = true;
            while (elapsed < warningTime)
            {
                visible = !visible;
                tRenderer.enabled = visible;
                float dt = Mathf.Min(blinkInterval, warningTime - elapsed);
                yield return new WaitForSeconds(dt);
                elapsed += dt;
            }
            tRenderer.enabled = true; // 깜빡임 끝나면 켬
        }

        // 3) 부드러운 페이드 아웃 (alpha 1 -> 0)
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, Mathf.Clamp01(t / fadeDuration));
            SetTilemapAlpha(a);
            yield return null;
        }
        SetTilemapAlpha(0f);

        // 4) 실제 충돌 OFF (플레이어가 떨어지게 됨)
        if (tCollider != null) tCollider.enabled = false;
        // 추가로 렌더러를 끄면 완전히 보이지 않음 (선택적)
        tRenderer.enabled = false;

        // 5) 기다리기 (사라진 상태 유지)
        yield return new WaitForSeconds(reappearDelay);

        // 6) 페이드 인 (0 -> 1)
        tRenderer.enabled = true; // 보이게 하고
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(0f, 1f, Mathf.Clamp01(t / fadeDuration));
            SetTilemapAlpha(a);
            yield return null;
        }
        SetTilemapAlpha(1f);

        // 7) 충돌 복원
        if (tCollider != null) tCollider.enabled = true;

        isProcessing = false;
    }

    void SetTilemapAlpha(float a)
    {
        if (tilemap != null)
        {
            Color c = originalColor;
            c.a = a;
            tilemap.color = c;
        }
    }
}
