using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitGame : MonoBehaviour
{
    public void QuitGameButton()
    {
#if UNITY_EDITOR
        // 如果是在编辑器中运行，退出播放模式
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // 如果是打包后的游戏，退出应用
            Application.Quit();
#endif
    }
}
