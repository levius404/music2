using System;
using System.Collections.Generic;

[Serializable]
public class NoteData
{
    public float time;// 音符开始时间
    public int lane;
    public float duration;  // 如果 duration > 0 则为长按音符
}

[Serializable]
public class NoteChart
{
    public List<NoteData> notes;
}

