using UnityEngine;

public class LongNote : MonoBehaviour
{
    private float startTime;
    private float endTime;
    private int lane;

    // 【修改】：started 现在表示 "正在被持有"
    private bool started = false;
    private bool judged = false;

    private SpriteRenderer[] spriteRenderers;
    private Transform head, body, tail;
    private float initialHeight;

    // 屏幕外销毁的Y坐标
    public float offScreenDestroyY = -20f;

    void Start()
    {
        // ... (Start 方法保持不变) ...
        head = transform.Find("Head");
        body = transform.Find("Body");
        tail = transform.Find("Tail");

        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        if (body == null)
        {
            Debug.LogError("Error: 'Body' child object not found!");
        }

        // 确保 Head 和 Tail 存在
        if (head != null) head.localPosition = Vector3.zero;
        if (tail != null) tail.localPosition = Vector3.zero;
    }

    public void Init(NoteData data)
    {
        // ... (Init 方法保持不变) ...
        startTime = data.time;
        endTime = data.time + data.duration;
        lane = data.lane;
        started = false;
        judged = false;

        float offset = MusicManager.Instance.spawnOffset;
        float spawnY = NoteSpawner.Instance.spawnY;
        float judgeY = JudgeManager.Instance.judgeY;
        float speed = (spawnY - judgeY) / offset;

        initialHeight = speed * data.duration;

        Vector3 lanePos = NoteSpawner.Instance.lanes[data.lane].position;
        transform.position = new Vector3(lanePos.x, spawnY, 0);

        // 【新增】：将长音符注册到 JudgeManager
        JudgeManager.Instance.RegisterLongNote(this);

        Debug.Log($"✅ LongNote Init lane {lane}, duration = {data.duration}");
    }

    void Update()
    {
        float t = MusicManager.Instance.GetMusicTime();
        float offset = MusicManager.Instance.spawnOffset;
        float spawnY = NoteSpawner.Instance.spawnY;
        float judgeY = JudgeManager.Instance.judgeY;
        float speed = (spawnY - judgeY) / offset;




        // 1. 下落逻辑：
        float timeSinceJudgeLine = t - startTime;
        float y = judgeY - (speed * timeSinceJudgeLine);
        transform.position = new Vector3(transform.position.x, y, 0);

        // 检查 y 的计算值：
         Debug.Log($"Calculated Y: {y:F2}"); // 如果需要，可以启用这个

        // 2. 长度收缩逻辑：
        float timeSinceHeadJudgeLine = t - endTime;
        float headY_Absolute = judgeY - (speed * timeSinceHeadJudgeLine);// 头部绝对位置 = 判定线位置 - (下落速度 * 经过判定线后的时间)
        float currentHeight = headY_Absolute - y;// 当前长度 = 头部绝对位置 - 尾部绝对位置
        currentHeight = Mathf.Max(currentHeight, 0.01f);// 限制最小长度


        // 3. 调整 Body 和 Head 的位置 (保持不变)
        if (body != null)
        {
            body.localScale = new Vector3(body.localScale.x, currentHeight / body.GetComponent<SpriteRenderer>().sprite.bounds.size.y, body.localScale.z);
            body.localPosition = new Vector3(0, currentHeight / 2f, 0);
        }

        if (head != null)
            head.localPosition = new Vector3(0, currentHeight, 0);

        // 🔴 持续日志：追踪位置变化
        Debug.Log($"Lane {lane} T={t:F2} Y={transform.position.y:F2} Height={currentHeight:F2}");
        // ==========================================================
        // 【判定逻辑区】

        // 4. Miss 判定逻辑
        // 如果头部已经完全经过判定窗口，且音符尚未被持有或判定，则判定 Miss。

        if (!judged && !started && t > startTime + JudgeManager.Instance.missTime)
        {
            // 尾部已经错过判定窗口，且玩家没有开始持有。
            // 🚨 调试日志：确认消失前是否触发 Miss
            Debug.LogError($"🚨 LongNote MISS TRIGGERED on lane {lane} at time {t}. Y={transform.position.y}");

            JudgeManager.Instance.MissLongNote(this); // 通知 JudgeManager 结算 Miss
            judged = true;
            // 【注意】：音符继续下落到 offScreenDestroyY
        }

        // 5. 自动成功/失败判定 (如果玩家一直按住，到结束时间自动结算)
        if (started && !judged && t >= endTime)
        {
            // 在结束时间 t >= endTime 时，检查玩家是否仍然按着键。
            // 实际上，JudgeManager 会通过 TryEndLongNoteHold (在 KeyUp 时) 或
            // 通过 Update 检查 Key (在长按音符结束时) 来处理。

            // 为了简化和保持结构，我们让 LongNote 在时间到时通知 JudgeManager 正常结束。
            // JudgeManager 将负责检查 GetKey 并设置 holdingLongNote[lane] = null。
            JudgeManager.Instance.FinishLongNoteHold(this); // 【新增方法】：用于自动结算
        }

        // ==========================================================

        // 6. 自动销毁 (过线不消失)
        if (transform.position.y < offScreenDestroyY)
        {
            Debug.LogError($"💥 LongNote DESTROYED off screen on lane {lane} at Y={transform.position.y}");
            Destroy(gameObject);
        }
    }

    // --- 【JudgeManager 调用方法】 ---

    // 【修改】：长按开始
    public void StartHold(float hitTime)
    {
        if (judged) return; // 已经被判定过（Miss）则忽略

        this.started = true; // 标记为正在被持有
        // TODO: 可以在这里启动长音符的视觉反馈 (例如变亮)
        Debug.Log("🟢 LongNote Held Start/Mid-Press");
    }

    // 【修改】：长按结束 (提前抬起时由 JudgeManager 调用)
    public  void EndHold(float releaseTime, bool earlyRelease)
    {
        if (judged) return; // 已经被判定过则忽略

        // 如果是提前抬起，判定失败（部分分数或失败）
        if (earlyRelease)
        {
            // TODO: LongNote 提前失败的结算逻辑
            Debug.Log("❌ LongNote Failed - Early Release");
        }
        // 正常结束判定将由 JudgeManager.FinishLongNoteHold 处理

        this.started = false; // 不再被持有
        this.judged = true; // 标记为已判定，防止再次开始持有
    }

    private void OnDestroy()
    {
        // 【关键】：这里应该记录是谁触发了销毁
        Debug.LogError($"🔥 LongNote OnDestroy called on lane {lane} at Y={transform.position.y}!");
    }


    // ... (Getter 方法保持不变) ...
    public float GetStartTime() => startTime;
    public float GetEndTime() => endTime;
    public int GetLane() => lane;
}