using UnityEngine;

public class Note : MonoBehaviour
{
    public float targetTime;
    public int lane;

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
        float progress = 1f - (delta / spawnOffset); // 0 → 1
        float startY = NoteSpawner.Instance.spawnY;
        float endY = JudgeManager.Instance.judgeY;
        float posY = Mathf.Lerp(startY, endY, progress);

        transform.position = new Vector3(transform.position.x, posY, 0);

        // 超时自动销毁
        if (t > targetTime + JudgeManager.Instance.missTime)
        {
            JudgeManager.Instance.MissNote(this);
            Destroy(gameObject);
        }
    }

    public float GetTime() => targetTime;
}