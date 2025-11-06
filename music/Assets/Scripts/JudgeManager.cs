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

    // 短音符队列 (保持不变)
    private List<Note>[] noteQueues = new List<Note>[6];

    // 【新增】长音符队列：用于注册和Miss判定
    private List<LongNote>[] longNoteQueues = new List<LongNote>[6];

    // 【新增】追踪当前被按住的长音符实例
    private LongNote[] holdingLongNote = new LongNote[6];

    void Awake()
    {
        Instance = this;
        for (int i = 0; i < 6; i++)
        {
            noteQueues[i] = new List<Note>();
            longNoteQueues[i] = new List<LongNote>(); // 初始化长音符队列
            holdingLongNote[i] = null; // 初始化长按追踪
        }
    }

    void Update()
    {
        float currentTime = MusicManager.Instance.GetMusicTime();

        for (int i = 0; i < keys.Length; i++)
        {
            // 1. 处理按下 (GetKeyDown) - 用于短音符判定和长音符开始
            if (Input.GetKeyDown(keys[i]))
            {
                Judge(i, currentTime); // 短音符的判定逻辑
                TryStartLongNoteHold(i, currentTime); // 尝试开始长音符持有 (新增)
            }

            // 2. 处理抬起 (GetKeyUp) - 用于长音符提前结束
            if (Input.GetKeyUp(keys[i]))
            {
                TryEndLongNoteHold(i, currentTime); // 尝试结束长音符持有 (新增)
            }
        }
    }




    // JudgeManager.cs (新增方法)
    public void FinishLongNoteHold(LongNote longNote)
    {
        int lane = longNote.GetLane();

        // 1. 检查是否仍在按键 (Full Hold Check)
        if (Input.GetKey(keys[lane]))
        {
            // 玩家按住了整个长音符
            longNote.EndHold(longNote.GetEndTime(), false); // false = not early release
            Debug.Log("✅ LongNote Success - Full Hold");
        }
        else
        {
            // 玩家在结束时间之前或刚好结束时间松开了，但在 EndHold(earlyRelease: true) 中没有被捕获。
            // 这被视为失败（Bad/Miss）。
            longNote.EndHold(longNote.GetEndTime(), true); // true = treated as early release/failure
            Debug.Log("❌ LongNote Failed - Released at End Time");
        }

        // 2. 清理队列和追踪
        longNoteQueues[lane].Remove(longNote);

        // 只有当 holdingLongNote[lane] 确实是这个音符时才清理它
        if (holdingLongNote[lane] == longNote)
        {
            holdingLongNote[lane] = null;
        }
    }

    private void TryStartLongNoteHold(int lane, float currentTime)
    {
        // 如果该轨道上已有长音符被按住，则忽略新的按下
        if (holdingLongNote[lane] != null) return;

        if (longNoteQueues[lane].Count == 0) return;

        LongNote longNote = longNoteQueues[lane][0];

        // 长按开始时间窗口：从 startTime - missTime 延伸到 endTime

        // 允许中途持有的逻辑：
        // 我们需要判断当前时间是否位于长音符的有效持有窗口内。
        // 有效持有窗口：从长音符的头部(EndTime)经过判定线之前，到尾部(StartTime)经过判定线之后的一段判定范围。
        //
        // 为了实现“中途开始判定”，我们简化逻辑：
        // 只要玩家在长音符的头部（EndTime）到达判定线之前，且尾部（StartTime）已经过了判定线后的一段时间内按下，
        // 就可以开始持有。

        // 简单判定：长音符的有效持有时间范围
        float holdStartLimit = longNote.GetStartTime() - missTime; // 允许提前按下的时间
        float holdEndLimit = longNote.GetEndTime() + missTime;     // 允许按住的结束时间（可以延长一点）

        // 1. 玩家按下时间太早：不处理
        if (currentTime < holdStartLimit) return;

        // 2. 玩家按下时间太晚：头部已过判定窗口末尾，应由 LongNote.cs 的 Miss 逻辑处理
        if (currentTime > longNote.GetEndTime() + perfectTime)
        {
            // 如果长音符头部已经完全过了判定线，这次按下是无效的，直接返回。
            return;
        }

        // 3. 玩家按下时间在有效范围内：开始持有
        // 无论是在 startTime 附近按下的，还是在 startTime 和 endTime 之间的任意时间按下的

        // 标记开始持有
        longNote.StartHold(currentTime); // 【注意】：需要在 LongNote.cs 中添加 StartHold 方法
        holdingLongNote[lane] = longNote;

        // 成功开始持有的 Log 可以放在 StartHold 内部
        Debug.Log($"🟢 LongNote Hold Started at {currentTime} on lane {lane}");
    }

    private void TryEndLongNoteHold(int lane, float currentTime)
    {
        LongNote longNote = holdingLongNote[lane];

        if (longNote != null)
        {
            // 结束持有
            longNote.EndHold(currentTime, true); // 【注意】：需要在 LongNote.cs 中添加 EndHold 方法

            // 从判定队列中移除（已判定完成）
            longNoteQueues[lane].Remove(longNote);

            // 移除追踪
            holdingLongNote[lane] = null;
            Debug.Log($"🔴 LongNote Hold Ended Early at {currentTime} on lane {lane}");
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

    // 【新增】注册长音符的方法
    public void RegisterLongNote(LongNote longNote)
    {
        longNoteQueues[longNote.GetLane()].Add(longNote);
    }

    // 【TODO】 LongNote 的 Miss 逻辑：稍后在 LongNote.cs 中调用
    public void MissLongNote(LongNote longNote)
    {
        longNoteQueues[longNote.GetLane()].Remove(longNote);
        Debug.Log("❌ LongNote Miss");
    }

    private void Judge(int lane, float currentTime)
    {
        if (noteQueues[lane].Count == 0) return;

        Note note = noteQueues[lane][0];

        // 【关键】：如果发现它是 LongNote，则忽略短音符判定
        if (note is LongNote)
        {
            // 确保 LongNote 不会被短音符的判定逻辑意外销毁
            return;
        }

        // --- 变量声明和初始化放在最前面 ---
        float delta = Mathf.Abs(note.GetTime() - currentTime);
        bool isHit = false;
        // ------------------------------------

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

            // 调用 Note 上的销毁方法，让音符自行销毁并设置 isJudged
            note.DestroyOnHit();
        }
    }
}