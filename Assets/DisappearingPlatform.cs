using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisappearingPlatform : MonoBehaviour
{
    public SpriteRenderer sr;
    public Collider2D col;
    public float activeTime = 3f;
    public float warningTime = 0.5f;
    public float blinkInterval = 0.1f;
    public float fadeDuration = 0.5f;
    public float inactiveTime = 2f;

    private Color baseColor;

    void Start()
    {
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        if (col == null) col = GetComponent<Collider2D>();
        baseColor = sr.color;
        StartCoroutine(Loop());
    }

    IEnumerator Loop()
    {
        while (true)
        {
            // 활성 상태
            sr.enabled = true;
            SetAlpha(1f);
            if (col) col.enabled = true;

            yield return new WaitForSeconds(Mathf.Max(0f, activeTime - warningTime));

            // 경고(깜빡임)
            float elapsed = 0f;
            bool visible = true;
            while (elapsed < warningTime)
            {
                visible = !visible;
                sr.enabled = visible;
                float dt = Mathf.Min(blinkInterval, warningTime - elapsed);
                yield return new WaitForSeconds(dt);
                elapsed += dt;
            }
            sr.enabled = true;

            // Fade Out
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                SetAlpha(Mathf.Lerp(1f, 0f, t / fadeDuration));
                yield return null;
            }
            SetAlpha(0f);
            if (col) col.enabled = false;
            sr.enabled = false;

            // 비활성
            yield return new WaitForSeconds(inactiveTime);

            // Fade In
            sr.enabled = true;
            t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                SetAlpha(Mathf.Lerp(0f, 1f, t / fadeDuration));
                yield return null;
            }
            SetAlpha(1f);
            if (col) col.enabled = true;
        }
    }

    void SetAlpha(float a)
    {
        if (sr != null)
        {
            Color c = baseColor;
            c.a = a;
            sr.color = c;
        }
    }
}
