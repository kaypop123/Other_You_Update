using UnityEngine;
using System.Collections;
using System.Linq;

public class AfterImageManager : MonoBehaviour
{
    public static AfterImageManager Instance;

    private bool bossWasAlive = false;

    void Awake()
    {
        Instance = this;
        StartCoroutine(CheckBossExistence());
    }

    private IEnumerator CheckBossExistence()
    {
        while (true)
        {

            GameObject[] bosses = FindObjectsOfType<GameObject>()
                .Where(obj => obj.name.Contains("AngryGod_Frame_E"))
                .ToArray();

            if (bosses.Length > 0)
            {
                bossWasAlive = true;
            }
            else if (bossWasAlive && bosses.Length == 0)
            {

                Debug.Log("[AfterImageManager] 보스 전멸 감지 → AfterImage 삭제 실행");
                ClearAllAfterImages();
                bossWasAlive = false;
            }

            yield return new WaitForSeconds(0.5f); 
        }
    }


    private void ClearAllAfterImages()
    {
        GameObject[] afterImages = GameObject.FindGameObjectsWithTag("AfterImage");

        foreach (GameObject img in afterImages)
        {
            if (img != null)
                Destroy(img);
        }

        Debug.Log($"[AfterImageManager] AfterImage {afterImages.Length}개 삭제 완료");
    }
}
