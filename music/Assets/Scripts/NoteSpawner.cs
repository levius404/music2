using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    public static NoteSpawner Instance;

    public Transform[] lanes;
    public GameObject notePrefab;
    public GameObject longNotePrefab;

    public float spawnY = 5f; // 生成起点

    void Awake() => Instance = this;

    public void SpawnNote(NoteData data)
    {
        Transform lane = lanes[data.lane];
        Vector3 spawnPos = new Vector3(lane.position.x, spawnY, 0);

        if (data.duration > 0)
        {
            GameObject obj = Instantiate(longNotePrefab, spawnPos, Quaternion.identity);
            LongNote longNote = obj.GetComponent<LongNote>();
            longNote.Init(data);

            // 【关键修改】：将长音符注册到 JudgeManager
            JudgeManager.Instance.RegisterLongNote(longNote);
        }
        else
        {
            GameObject obj = Instantiate(notePrefab, spawnPos, Quaternion.identity);
            obj.GetComponent<Note>().Init(data);
            JudgeManager.Instance.RegisterNote(obj.GetComponent<Note>());
        }
    }
}