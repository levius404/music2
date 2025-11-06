using UnityEngine;
using System.Collections.Generic;

public class JudgeManager : MonoBehaviour
{
    public static JudgeManager Instance;

    public KeyCode[] keys = { KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.J, KeyCode.K, KeyCode.L };
    public float perfectTime = 0.05f;
    public float goodTime = 0.1f;
    public float missTime = 0.2f;

    public float judgeY = -3.5f;

    private List<Note>[] noteQueues = new List<Note>[6];

    void Awake()
    {
        Instance = this;
        for (int i = 0; i < 6; i++) noteQueues[i] = new List<Note>();
    }

    void Update()
    {
        float currentTime = MusicManager.Instance.GetMusicTime();

        for (int i = 0; i < keys.Length; i++)
        {
            if (Input.GetKeyDown(keys[i]))
            {
                Judge(i, currentTime);
            }
        }
    }

    public void RegisterNote(Note note)
    {
        noteQueues[note.lane].Add(note);
    }

    // --- Miss Note 逻辑 (修改) ---
    public void MissNote(Note note)
    {
        // 关键：只负责从队列中移除音符，ScoreManager.Instance.AddMiss() 逻辑也在这里。
        noteQueues[note.lane].Remove(note);
        Debug.Log("❌ Miss");
        // 【注意】：不再调用 Destroy(gameObject)
    }

    private void Judge(int lane, float currentTime)
    {
        if (noteQueues[lane].Count == 0) return;

        Note note = noteQueues[lane][0];
        float delta = Mathf.Abs(note.GetTime() - currentTime);

        bool isHit = false;

        // 1. 执行判定
        if (delta <= perfectTime)
        {
            Debug.Log("Perfect");
            isHit = true;
        }
        else if (delta <= goodTime)
        {
            Debug.Log("Good");
            isHit = true;
        }
        else if (delta <= missTime)
        {
            Debug.Log("Bad");
            isHit = true;
        }
        else
        {
            // Too late: 音符已经超过判定窗口。
            Debug.Log("Too late");
            return;
        }

        // 2. 成功击中后的处理 (修改)
        if (isHit)
        {
            noteQueues[lane].Remove(note);

            // 【关键修改】：调用 Note 上的销毁方法，让音符自行销毁并设置 isJudged
            note.DestroyOnHit();
        }
    }
}