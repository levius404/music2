using UnityEngine;
using System.Collections.Generic;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    public AudioSource audioSource;
    public NoteSpawner spawner;
    public TextAsset chartJSON;
    public float spawnOffset = 2.0f; // 提前几秒生成音符

    private List<NoteData> notes;
    private int nextIndex = 0;

    void Awake() => Instance = this;

    void Start()
    {
        notes = JsonUtility.FromJson<NoteChart>(chartJSON.text).notes;
        audioSource.Play();
    }

    void Update()
    {
        float musicTime = GetMusicTime();

        while (nextIndex < notes.Count && musicTime >= notes[nextIndex].time - spawnOffset)
        {
            spawner.SpawnNote(notes[nextIndex]);
            nextIndex++;
        }
    }

    public float GetMusicTime() => audioSource.time;
}