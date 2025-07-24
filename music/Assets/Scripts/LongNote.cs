using UnityEngine;

public class LongNote : MonoBehaviour
{
    private float startTime;
    private float endTime;
    private int lane;
    private bool started = false;
    private bool judged = false;

    private SpriteRenderer[] spriteRenderers;
    private SpriteRenderer bodySpriteRenderer;

    private Transform head, body, tail;
    private float initialHeight;
    private float fadeStartTime;
    private float fadeDuration;

    void Start()
    {
        head = transform.Find("Head");
        body = transform.Find("Body");
        tail = transform.Find("Tail");

        bodySpriteRenderer = body.GetComponent<SpriteRenderer>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
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

        initialHeight = speed * data.duration;

        // 设置轨道位置
        Vector3 lanePos = NoteSpawner.Instance.lanes[data.lane].position;
        transform.position = new Vector3(lanePos.x, spawnY, 0);

        // 设置 Body size（使用 Sliced 模式的 SpriteRenderer）
        if (bodySpriteRenderer != null)
        {
            float width = bodySpriteRenderer.size.x;
            bodySpriteRenderer.size = new Vector2(width, initialHeight);
            body.localPosition = new Vector3(0, -initialHeight / 2f, 0);
        }

        if (head != null)
            head.localPosition = Vector3.zero;

        if (tail != null)
            tail.localPosition = new Vector3(0, -initialHeight, 0);

        fadeStartTime = endTime;
        fadeDuration = offset;

        Debug.Log($"✅ LongNote Init lane {lane}, height = {initialHeight}");

        // 可选调试 Cube
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = transform.position;
        cube.transform.localScale = Vector3.one * 0.1f;
    }

    void Update()
    {
        float t = MusicManager.Instance.GetMusicTime();
        float offset = MusicManager.Instance.spawnOffset;
        float spawnY = NoteSpawner.Instance.spawnY;
        float judgeY = JudgeManager.Instance.judgeY;

        // 1. 让 Head 在 startTime 到达 judgeY
        float fallProgress = Mathf.InverseLerp(startTime - offset, startTime, t);
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
                Debug.Log("✅ LongNote Success");
            else
                Debug.Log("❌ LongNote Failed");
        }

        // 3. 收缩视觉长度（使用 .size 而不是 .localScale）
        if (started && t <= endTime && bodySpriteRenderer != null)
        {
            float shrinkProgress = Mathf.InverseLerp(startTime, endTime, t);
            float currentHeight = Mathf.Max(Mathf.Lerp(initialHeight, 0f, shrinkProgress), 0.01f);

            float width = bodySpriteRenderer.size.x;
            bodySpriteRenderer.size = new Vector2(width, currentHeight);
            body.localPosition = new Vector3(0, -currentHeight / 2f, 0);

            if (tail != null)
                tail.localPosition = new Vector3(0, -currentHeight, 0);
        }

        // 4. 淡出
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

        // 5. 自动销毁
        if (t >= endTime + offset)
            Destroy(gameObject);
    }
}
