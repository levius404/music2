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

    private List<Note>[] noteQueues = new List<Note>[6];

    void Awake()
    {
        Instance = this;
        for (int i = 0; i < 6; i++) noteQueues[i] = new List<Note>();
    }

    void Update()
    {
        float currentTime = MusicManager.Instance.GetMusicTime();

        for (int i = 0; i < keys.Length; i++)
        {
            if (Input.GetKeyDown(keys[i]))
            {
                Judge(i, currentTime);
            }
        }
    }

    public void RegisterNote(Note note)
    {
        noteQueues[note.lane].Add(note);
    }

    public void MissNote(Note note)
    {
        noteQueues[note.lane].Remove(note);
        Debug.Log("❌ Miss");
    }

    private void Judge(int lane, float currentTime)
    {
        if (noteQueues[lane].Count == 0) return;

        Note note = noteQueues[lane][0];
        float delta = Mathf.Abs(note.GetTime() - currentTime);

        if (delta <= perfectTime)
        {
            Debug.Log("Perfect");
        }
        else if (delta <= goodTime)
        {
            Debug.Log("Good");
        }
        else if (delta <= missTime)
        {
            Debug.Log("Bad");
        }
        else
        {
            Debug.Log("Too late");
            return;
        }

        noteQueues[lane].Remove(note);
        Destroy(note.gameObject);
    }
}




