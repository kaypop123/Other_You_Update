using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

[CreateAssetMenu(fileName = "BlinkingTile", menuName = "CustomTiles/BlinkingTile")]
public class BlinkingTile : Tile
{
    public float visibleDuration = 3f;   // 보이는 시간
    public float hiddenDuration = 2f;    // 안 보이는 시간
    public float fadeTime = 0.5f;        // 깜빡/페이드 시간

    private Tilemap tilemap;
    private bool isRunning = false;

    public override bool StartUp(Vector3Int position, ITilemap tilemapInstance, GameObject go)
    {
        base.StartUp(position, tilemapInstance, go);

        if (!isRunning)
        {
            isRunning = true;
            tilemap = tilemapInstance.GetComponent<Tilemap>();
            var mono = tilemap.GetComponent<MonoBehaviour>();
            if (mono != null)
                mono.StartCoroutine(BlinkRoutine(position));
        }

        return true;
    }

    private IEnumerator BlinkRoutine(Vector3Int pos)
    {
        while (true)
        {
            // 보이는 상태
            SetAlpha(pos, 1f);
            yield return new WaitForSeconds(visibleDuration - fadeTime);

            // 깜빡임(페이드 아웃)
            yield return FadeOut(pos);

            // 숨김 상태
            SetAlpha(pos, 0f);
            yield return new WaitForSeconds(hiddenDuration);

            // 페이드 인
            yield return FadeIn(pos);
        }
    }

    private IEnumerator FadeOut(Vector3Int pos)
    {
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            float a = Mathf.Lerp(1f, 0f, t / fadeTime);
            SetAlpha(pos, a);
            yield return null;
        }
        SetAlpha(pos, 0f);
    }

    private IEnumerator FadeIn(Vector3Int pos)
    {
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            float a = Mathf.Lerp(0f, 1f, t / fadeTime);
            SetAlpha(pos, a);
            yield return null;
        }
        SetAlpha(pos, 1f);
    }

    private void SetAlpha(Vector3Int pos, float alpha)
    {
        if (tilemap == null) return;
        Color c = tilemap.GetColor(pos);
        c.a = alpha;
        tilemap.SetColor(pos, c);
    }
}
