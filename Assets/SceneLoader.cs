using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; // 씬 이동용 네임스페이스

public class SceneLoader : MonoBehaviour
{
    // 메인화면으로 돌아가기
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("startscenes"); // ← 실제 메인화면 씬 이름으로 바꿔줘!
    }

    // 게임 재시작 (기존 기능 유지)
    public void RebootGame()
    {
#if UNITY_STANDALONE
        string[] executableEndings = new string[] { "exe", "x86", "x86_64", "app" };
        string executablePath = Application.dataPath + "/../";

        // 실행 파일 찾기
        foreach (string file in System.IO.Directory.GetFiles(executablePath))
        {
            foreach (string ending in executableEndings)
            {
                if (file.ToLower().EndsWith("." + ending))
                {
                    Debug.Log("[Reboot] 실행 시도: " + file);
                    System.Diagnostics.Process.Start(file); // 새 인스턴스 실행
                    Application.Quit(); // 현재 게임 종료
                    return;
                }
            }
        }

        Debug.LogError("[Reboot] 실행 파일을 찾을 수 없습니다.");
#else
        Debug.LogWarning("RebootGame()은 Standalone 환경에서만 작동합니다.");
#endif
    }
}
