using UnityEngine;

public class Note : MonoBehaviour
{
    public float targetTime;
    public int lane;
    // 【新增】状态标志：标记该音符是否已经被判定过（成功击中或Miss）
    private bool isJudged = false;

    // 【新增】屏幕外销毁的Y坐标。请根据你的场景调整，-5f 是一个合理的起始值。
    public float offScreenDestroyY = -5f;

    public void Init(NoteData data)
    {
        targetTime = data.time;
        lane = data.lane;
    }




    void Update()
    {
        float t = MusicManager.Instance.GetMusicTime();
        float delta = targetTime - t;

        // 自动按时间推进动画（向下移动）
        float spawnOffset = MusicManager.Instance.spawnOffset;
        float startY = NoteSpawner.Instance.spawnY;
        float endY = JudgeManager.Instance.judgeY;

        // 1. 计算 progress (与之前相同)
        float progress = 1f - (delta / spawnOffset); // progress < 0 或 progress > 1 都是允许的

        // 2. 【关键修改】：使用手动线性插值，**不钳制 progress**
        // 公式: V = A + (B - A) * t
        float posY = startY + (endY - startY) * progress;

        // 3. 应用位置
        transform.position = new Vector3(transform.position.x, posY, 0);

        // --- Miss 判定逻辑 (修改) ---
        // 只有当音符尚未被判定，且超过了 Miss 判定时间窗口时，才触发 Miss 结算。
        if (!isJudged && t > targetTime + JudgeManager.Instance.missTime)
        {
            // 1. 结算 Miss 分数，并从判定队列中移除（由 JudgeManager 处理）
            JudgeManager.Instance.MissNote(this);
            // 2. 标记音符为已判定 (Miss)，防止 JudgeManager 再次尝试判定它
            isJudged = true;
            // 【关键修改】：不再调用 Destroy(gameObject)，音符继续下落。
        }

        // --- 屏幕外销毁逻辑 (新增) ---
        // 音符到达屏幕外后，进行销毁
        if (transform.position.y < offScreenDestroyY)
        {
            Destroy(gameObject);
        }
    }

    public float GetTime() => targetTime;

    // 【新增】供 JudgeManager 调用，用于成功击中后的立即销毁
    public void DestroyOnHit()
    {
        isJudged = true; // 标记为已判定
        Destroy(gameObject); // 立即销毁
    }
}