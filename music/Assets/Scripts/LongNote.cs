using UnityEngine;

public class LongNote : MonoBehaviour
{
    private float startTime;
    private float endTime;
    private int lane;
    private bool started = false;
    private bool judged = false;

    public Transform visual; // 子物体：SpriteRenderer显示体
    private SpriteRenderer sr;

    private float initialHeight;
    private float fadeStartTime;
    private float fadeDuration;

    void Start()
    {
        if (visual != null)
            sr = visual.GetComponent<SpriteRenderer>();
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

        // 初始视觉高度（根据持续时间）
        initialHeight = speed * data.duration;

        if (visual != null)
        {
            visual.localScale = new Vector3(visual.localScale.x, initialHeight, visual.localScale.z);
            visual.localPosition = new Vector3(0, -initialHeight / 2f, 0);
        }

        // 设置初始位置在顶部
        transform.position = new Vector3(transform.position.x, spawnY, 0);

        fadeStartTime = endTime;
        fadeDuration = offset;
    }

    void Update()
    {
        float t = MusicManager.Instance.GetMusicTime();
        float offset = MusicManager.Instance.spawnOffset;

        float spawnY = NoteSpawner.Instance.spawnY;
        float judgeY = JudgeManager.Instance.judgeY;

        // ⬇️ 1. 整体本体移动：从 spawnY → judgeY
        float fallStart = startTime - offset;
        float fallEnd = endTime;
        float fallProgress = Mathf.InverseLerp(fallStart, fallEnd, t);
        float y = Mathf.Lerp(spawnY, judgeY, fallProgress);
        transform.position = new Vector3(transform.position.x, y, 0);

        // ✅ 2. 判定逻辑
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

        // 🟦 3. 缩短尾部（视觉变短）
        if (t >= startTime && t <= endTime)
        {
            float shrinkProgress = Mathf.InverseLerp(startTime, endTime, t);
            float currentHeight = Mathf.Lerp(initialHeight, 0f, shrinkProgress);

            if (visual != null)
            {
                visual.localScale = new Vector3(visual.localScale.x, currentHeight, visual.localScale.z);
                visual.localPosition = new Vector3(0, -currentHeight / 2f, 0);
            }
        }

        // 🟨 4. 淡出（透明度）
        if (t >= fadeStartTime)
        {
            float fadeProgress = Mathf.InverseLerp(fadeStartTime, fadeStartTime + fadeDuration, t);
            float alpha = 1f - fadeProgress;

            if (sr != null)
            {
                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }
        }

        // ❌ 5. 超时销毁
        if (t >= endTime + offset)
        {
            Destroy(gameObject);
        }
    }
}
