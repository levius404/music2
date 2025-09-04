using UnityEngine;

public class LongNote : MonoBehaviour
{
    private float startTime;
    private float endTime;
    private int lane;
    private bool started = false;
    private bool judged = false;

    private SpriteRenderer[] spriteRenderers;
    private Transform head, body, tail;
    private float initialHeight;
    private float fadeStartTime;
    private float fadeDuration;

    void Start()
    {
        head = transform.Find("Head");
        body = transform.Find("Body");
        tail = transform.Find("Tail");

        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        if (body == null)
        {
            Debug.LogError("Error: 'Body' child object not found!");
        }
    }

    public void Init(NoteData data)
    {
        startTime = data.time;
        endTime = data.time + data.duration;
        lane = data.lane;

        float offset = MusicManager.Instance.spawnOffset;
        float spawnY = NoteSpawner.Instance.spawnY;
        float judgeY = JudgeManager.Instance.judgeY;
        float speed = (spawnY - judgeY) / offset;

        // initialHeight 代表音符在完全展开时的长度
        initialHeight = speed * data.duration;

        Vector3 lanePos = NoteSpawner.Instance.lanes[data.lane].position;
        // 初始位置应该以音符的尾部作为基准，所以要加上一个偏移量
        transform.position = new Vector3(lanePos.x, spawnY + initialHeight, 0);

        fadeStartTime = endTime;
        fadeDuration = offset;

        Debug.Log($"✅ LongNote Init lane {lane}, height = {initialHeight}");
    }

    void Update()
    {
        float t = MusicManager.Instance.GetMusicTime();
        float offset = MusicManager.Instance.spawnOffset;
        float spawnY = NoteSpawner.Instance.spawnY;
        float judgeY = JudgeManager.Instance.judgeY;

        // 1. 下落逻辑：尾部在 startTime 到达 judgeY
        // 这次我们用尾部作为下落的基准点，它会从 spawnY 移动到 judgeY
        float tailFallProgress = Mathf.InverseLerp(startTime - offset, startTime, t);
        float y = Mathf.Lerp(spawnY, judgeY, tailFallProgress);
        transform.position = new Vector3(transform.position.x, y, 0);

        // 2. 长度收缩逻辑：长度在 startTime 到 endTime 之间收缩
        float lengthProgress = Mathf.InverseLerp(startTime, endTime, t);
        float currentHeight = Mathf.Max(Mathf.Lerp(initialHeight, 0f, lengthProgress), 0.01f);

        // 3. 调整 Body 和 Head 的位置，使它们跟随 Tail
        // **本次修改的重点：所有子对象的位置都改为正值，使其在Tail上方**
        if (body != null)
        {
            body.localScale = new Vector3(body.localScale.x, currentHeight / body.GetComponent<SpriteRenderer>().sprite.bounds.size.y, body.localScale.z);
            body.localPosition = new Vector3(0, currentHeight / 2f, 0);
        }

        if (head != null)
            head.localPosition = new Vector3(0, currentHeight, 0);

        // 4. 判定逻辑
        if (!started && Mathf.Abs(t - startTime) <= JudgeManager.Instance.perfectTime)
        {
            if (Input.GetKey(JudgeManager.Instance.keys[lane]))
            {
                started = true;
                Debug.Log("🟢 LongNote Hold Start");
            }
        }

        // 5. 长按结束判定
        if (!judged && t >= endTime)
        {
            judged = true;
            if (started && Input.GetKey(JudgeManager.Instance.keys[lane]))
                Debug.Log("✅ LongNote Success");
            else
                Debug.Log("❌ LongNote Failed");
        }

        // 6. 淡出
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

        // 7. 自动销毁
        if (t >= endTime + offset)
            Destroy(gameObject);
    }
}