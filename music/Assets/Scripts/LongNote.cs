using UnityEngine;

public class LongNote : MonoBehaviour
{
    private float startTime;
    private float endTime;
    private int lane;
    private bool started = false;
    private bool judged = false;

    public Transform visual; // 显示体（包含 head, body, tail）
    private SpriteRenderer[] spriteRenderers;

    private float initialHeight;
    private float fadeStartTime;
    private float fadeDuration;

    void Start()
    {
        if (visual != null)
            spriteRenderers = visual.GetComponentsInChildren<SpriteRenderer>();
    }

    public void Init(NoteData data)
    {
        startTime = data.time;
        endTime = data.time + data.duration;
        lane = data.lane;

        float offset = MusicManager.Instance.spawnOffset;
        float spawnY = NoteSpawner.Instance.spawnY;
        float judgeY = JudgeManager.Instance.judgeY;
        float totalDistance = spawnY - judgeY;
        float speed = totalDistance / offset;

        // 视觉高度（用于 body 拉伸）
        initialHeight = speed * data.duration;

        if (visual != null)
        {
            Transform head = visual.Find("head");
            Transform body = visual.Find("body");
            Transform tail = visual.Find("tail");

            float bodyHeight = speed * data.duration;

            // 设置 body 高度（Y轴缩放），居中对齐
            if (body != null)
            {
                body.localScale = new Vector3(1, bodyHeight, 1);
                body.localPosition = new Vector3(0, -bodyHeight / 2f, 0);
            }

            // head 固定在顶部
            if (head != null)
                head.localPosition = Vector3.zero;

            // tail 固定在底部
            if (tail != null)
                tail.localPosition = new Vector3(0, -bodyHeight, 0);
        }
    }

    void Update()
    {
        float t = MusicManager.Instance.GetMusicTime();
        float offset = MusicManager.Instance.spawnOffset;

        float spawnY = NoteSpawner.Instance.spawnY;
        float judgeY = JudgeManager.Instance.judgeY;

        // 1. 整体下落
        float fallStart = startTime - offset;
        float fallEnd = endTime;
        float fallProgress = Mathf.InverseLerp(fallStart, fallEnd, t);
        float y = Mathf.Lerp(spawnY, judgeY, fallProgress);
        transform.position = new Vector3(transform.position.x, y, 0);

        // 2. 判定逻辑
        if (!started && Mathf.Abs(t - startTime) <= JudgeManager.Instance.perfectTime)
        {
            if (Input.GetKey(JudgeManager.Instance.keys[lane]))
            {
                started = true;
                Debug.Log("🟢 LongNote Hold Start");
            }
        }

        if (!judged && t >= endTime)
        {
            judged = true;
            if (started && Input.GetKey(JudgeManager.Instance.keys[lane]))
            {
                Debug.Log("✅ LongNote Success");
            }
            else
            {
                Debug.Log("❌ LongNote Failed");
            }
        }

        // 3. 尾部收缩（视觉高度变短）
        if (t >= startTime && t <= endTime)
        {
            float shrinkProgress = Mathf.InverseLerp(startTime, endTime, t);
            float currentHeight = Mathf.Lerp(initialHeight, 0f, shrinkProgress);

            Transform body = visual.Find("body");
            Transform tail = visual.Find("tail");

            if (body != null)
            {
                body.localScale = new Vector3(1, currentHeight, 1);
                body.localPosition = new Vector3(0, -currentHeight / 2f, 0);
            }

            if (tail != null)
            {
                tail.localPosition = new Vector3(0, -currentHeight, 0);
            }
        }

        // 4. 淡出效果（作用于全部 SpriteRenderer）
        if (t >= fadeStartTime && spriteRenderers != null)
        {
            float fadeProgress = Mathf.InverseLerp(fadeStartTime, fadeStartTime + fadeDuration, t);
            float alpha = 1f - fadeProgress;

            foreach (var sr in spriteRenderers)
            {
                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }
        }

        // 5. 超时销毁
        if (t >= endTime + offset)
        {
            Destroy(gameObject);
        }
    }
}
